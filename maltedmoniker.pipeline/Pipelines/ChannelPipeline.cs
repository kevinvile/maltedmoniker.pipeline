using maltedmoniker.pipeline.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Pipelines
{
    public sealed class ChannelPipeline<TIn, TOut> : Pipeline<TIn, TOut>
    {
        public int ChannelSize { get; set; } = 150;
        public ChannelPipeline(List<IPipe> pipes, IPipeline<(TIn, Exception), TOut>? exceptionPipeline = null) : base(pipes, exceptionPipeline) { }

        public override async IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, [EnumeratorCancellation] CancellationToken token = default)
        {
            var multiReaders = GetReaders(items, token);
            var outReader = WriteResultsFromReaders(multiReaders, token);

            await foreach (var item in outReader.ReadAllAsync(token))
            {
                yield return item;
            }
        }

        private List<ChannelReader<TIn>> GetReaders(IEnumerable<TIn> items, CancellationToken token)
        {
            var inChannels = BuildInChannels();

            Task.Run(async () =>
            {
                int index = 0;
                foreach (var item in items)
                {
                    await inChannels[index].Writer.WriteAsync(item, token);
                    index = (index + 1) % ChannelSize;
                }

                inChannels.CloseAllWriters();

            }, token);


            return inChannels.GetAllReaders();
        }

        private List<Channel<TIn>> BuildInChannels()
        {
            var inChannels = new List<Channel<TIn>>(ChannelSize);
            for (int i = 0; i < ChannelSize; i += 1)
            {
                inChannels.Add(Channel.CreateUnbounded<TIn>());
            }
            return inChannels;
        }

        private ChannelReader<TOut> WriteResultsFromReaders(List<ChannelReader<TIn>> multiReaders, CancellationToken token)
        {
            var outChannel = Channel.CreateUnbounded<TOut>();
            int done = 0;
            int indexer = 0;
            foreach (var reader in multiReaders)
            {
                var readIndex = indexer;
                var task = Task.Run(async () =>
                {
                    await foreach (var item in reader.ReadAllAsync())
                    {
                        var result = await Process(item, token);
                        if (result is null) continue;

                        await outChannel.Writer.WriteAsync(result, token);
                    }

                    Interlocked.Increment(ref done);
                    if (done == multiReaders.Count) outChannel.Writer.Complete();

                }, token);
                indexer += 1;
            }
            return outChannel.Reader;
        }

    }

}

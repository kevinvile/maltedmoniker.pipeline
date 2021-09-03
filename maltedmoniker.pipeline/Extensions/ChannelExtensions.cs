using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Extensions
{
    public static class ChannelExtensions
    {
        public static void CloseAllWriters<T>(this IEnumerable<Channel<T>> channels)
        {
            foreach (var channel in channels)
            {
                channel.Writer.Complete();
            }
        }

        public static List<ChannelReader<T>> GetAllReaders<T>(this IEnumerable<Channel<T>> channels)
        {
            return channels.Select(c => c.Reader).ToList();
        }

        public static IList<ChannelReader<T>> Split<T>(this ChannelReader<T> ch, int n)
        {
            var outputs = new Channel<T>[n];

            for (int i = 0; i < n; i++)
                outputs[i] = Channel.CreateUnbounded<T>();

            Task.Run(async () =>
            {
                var index = 0;
                await foreach (var item in ch.ReadAllAsync())
                {
                    await outputs[index].Writer.WriteAsync(item);
                    index = (index + 1) % n;
                }

                foreach (var ch in outputs)
                {
                    ch.Writer.Complete();
                }
            });

            return outputs.Select(ch => ch.Reader).ToArray();
        }

        public static ChannelReader<T> Merge<T>(this IEnumerable<ChannelReader<T>> inputs)
        {
            var output = Channel.CreateUnbounded<T>();

            Task.Run(async () =>
            {
                async Task Redirect(ChannelReader<T> input)
                {
                    await foreach (var item in input.ReadAllAsync())
                    {
                        await output.Writer.WriteAsync(item);
                    }
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());

                output.Writer.Complete();
            });

            return output;
        }

        public static ChannelReader<T> SplitAndMerge<T>(this ChannelReader<T> ch, int n)
        {
            var channels = ch.Split(n);
            return channels.Merge();
        }
    }
}

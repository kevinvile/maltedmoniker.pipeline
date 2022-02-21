using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Pipelines
{
    public sealed class ParallelPipeline<TIn, TOut> : Pipeline<TIn, TOut>
    {
        public int ParallelSize { get; set; } = 150;
        public ParallelPipeline(List<IPipe> pipes, IPipeline<(TIn, Exception), TOut>? exceptionPipeline = null) : base(pipes, exceptionPipeline)
        {
        }

        public override async IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, [EnumeratorCancellation] CancellationToken token = default)
        {

            List<TIn> captured = new();
            foreach (var item in items)
            {
                if (captured.Count == ParallelSize)
                {
                    await foreach (var i in ProcessBatch(captured, token))
                    {
                        yield return i;
                    }
                    captured = new();

                }
                captured.Add(item);

            }
            if (captured.Count > 0)
            {
                await foreach (var i in ProcessBatch(captured, token))
                {
                    yield return i;
                }
            }
        }

        private async IAsyncEnumerable<TOut> ProcessBatch(List<TIn> items, [EnumeratorCancellation] CancellationToken token)
        {
            var bag = new ConcurrentQueue<TOut>();
            var tasks = items.Select(async item =>
            {
                var result = await Process(item, token);
                if (result is not null) bag.Enqueue(result);
            });
            await Task.WhenAll(tasks);
            foreach (var i in bag)
            {
                yield return i;
            }
        }

    }

}

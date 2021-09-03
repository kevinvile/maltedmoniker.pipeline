using System.Collections.Generic;
using System.Threading;

namespace maltedmoniker.pipeline.Pipelines
{
    public interface IPipeline<T>
    {
        IAsyncEnumerable<T> Process(IEnumerable<T> items, CancellationToken token = default);
    }

    public interface IPipeline<TIn, TOut>
    {
        IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, CancellationToken token = default);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;

namespace maltedmoniker.pipeline.Pipelines
{
    public interface IPipeline<T> : IDisposable
    {
        IAsyncEnumerable<T> Process(IEnumerable<T> items, CancellationToken token = default);
    }

    public interface IPipeline<TIn, TOut> : IDisposable
    {
        IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, CancellationToken token = default);
    }
}

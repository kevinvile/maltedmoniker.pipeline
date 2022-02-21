using System;
using System.Collections.Generic;

namespace maltedmoniker.pipeline.Pipelines
{
    public sealed class ImmutablePipeline<T> : Pipeline<T> where T : Immutable
    {
        public ImmutablePipeline(List<IPipe<T>> pipes, IPipeline<(T, Exception), T>? exceptionPipeline = null) : base(pipes, exceptionPipeline)
        {
            ChangeItemPreProcessor((i) => i with { });
        }
    }

    public sealed class ImmutablePipeline<TIn, TOut> : Pipeline<TIn, TOut> where TIn : Immutable
    {
        public ImmutablePipeline(List<IPipe> pipes, IPipeline<(TIn, Exception), TOut>? exceptionPipeline = null) : base(pipes, exceptionPipeline)
        {
            ChangeItemPreProcessor((i) => i with { });
        }
    }

}

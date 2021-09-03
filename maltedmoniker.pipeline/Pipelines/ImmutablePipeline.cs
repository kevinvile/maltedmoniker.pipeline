using System.Collections.Generic;

namespace maltedmoniker.pipeline.Pipelines
{
    public sealed class ImmutablePipeline<T> : Pipeline<T> where T : Immutable
    {
        public ImmutablePipeline(List<IPipe<T>> pipes) : base(pipes)
        {
            ChangeItemPreProcessor((i) => i with { });
        }
    }

    public sealed class ImmutablePipeline<TIn, TOut> : Pipeline<TIn, TOut> where TIn : Immutable
    {
        public ImmutablePipeline(List<IPipe> pipes) : base(pipes)
        {
            ChangeItemPreProcessor((i) => i with { });
        }
    }

}

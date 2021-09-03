using maltedmoniker.pipeline.Builders;
using maltedmoniker.pipeline.Pipelines;

namespace maltedmoniker.pipeline.Factories
{
    public sealed class PipelineBuilderFactory : IPipelineBuilderFactory
    {
        public IPipelineBuilder<TPipeline, TIn, TOut> GetBuilder<TIn, TOut, TPipeline>() where TPipeline : IPipeline<TIn, TOut>
        {
            return new PipelineBuilder<TPipeline, TIn, TOut>();
        }

        public IPipelineBuilder<TPipeline, TIn> GetBuilder<TIn, TPipeline>() where TPipeline : IPipeline<TIn>
        {
            return new PipelineBuilder<TPipeline, TIn>();
        }

        public IPipelineBuilder<ChannelPipeline<TIn, TOut>, TIn, TOut> GetChannelBuilder<TIn, TOut>()
        {
            return new PipelineBuilder<ChannelPipeline<TIn, TOut>, TIn, TOut>();
        }

        public IPipelineBuilder<ParallelPipeline<TIn, TOut>, TIn, TOut> GetParallelBuilder<TIn, TOut>()
        {
            return new PipelineBuilder<ParallelPipeline<TIn, TOut>, TIn, TOut>();
        }

        public IPipelineBuilder<ImmutablePipeline<TIn, TOut>, TIn, TOut> GetImmutableBuilder<TIn, TOut>() where TIn : Immutable
        {
            return new PipelineBuilder<ImmutablePipeline<TIn, TOut>, TIn, TOut>();
        }

        public IPipelineBuilder<ImmutablePipeline<TIn>, TIn> GetImmutableBuilder<TIn>() where TIn : Immutable
        {
            return new PipelineBuilder<ImmutablePipeline<TIn>, TIn>();
        }

        public IPipelineBuilder<Pipeline<TIn, TOut>, TIn, TOut> GetPipelineBuilder<TIn, TOut>() where TIn : Immutable
        {
            return new PipelineBuilder<Pipeline<TIn, TOut>, TIn, TOut>();
        }

        public IPipelineBuilder<Pipeline<TIn>, TIn> GetPipelineBuilder<TIn>() where TIn : Immutable
        {
            return new PipelineBuilder<Pipeline<TIn>, TIn>();
        }
    }
}

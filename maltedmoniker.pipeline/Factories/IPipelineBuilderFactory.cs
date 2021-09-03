using maltedmoniker.pipeline.Builders;
using maltedmoniker.pipeline.Pipelines;

namespace maltedmoniker.pipeline.Factories
{
    public interface IPipelineBuilderFactory
    {
        IPipelineBuilder<TPipeline, TIn, TOut> GetBuilder<TIn, TOut, TPipeline>() where TPipeline : IPipeline<TIn, TOut>;
        IPipelineBuilder<TPipeline, TIn> GetBuilder<TIn, TPipeline>() where TPipeline : IPipeline<TIn>;
        IPipelineBuilder<ChannelPipeline<TIn, TOut>, TIn, TOut> GetChannelBuilder<TIn, TOut>();
        IPipelineBuilder<ImmutablePipeline<TIn, TOut>, TIn, TOut> GetImmutableBuilder<TIn, TOut>() where TIn : Immutable;
        IPipelineBuilder<ImmutablePipeline<TIn>, TIn> GetImmutableBuilder<TIn>() where TIn : Immutable;
        IPipelineBuilder<ParallelPipeline<TIn, TOut>, TIn, TOut> GetParallelBuilder<TIn, TOut>();
        IPipelineBuilder<Pipeline<TIn, TOut>, TIn, TOut> GetPipelineBuilder<TIn, TOut>() where TIn : Immutable;
        IPipelineBuilder<Pipeline<TIn>, TIn> GetPipelineBuilder<TIn>() where TIn : Immutable;
    }
}
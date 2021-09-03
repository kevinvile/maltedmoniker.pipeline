using maltedmoniker.pipeline.Pipelines;

namespace maltedmoniker.pipeline.Builders
{
    public interface IPipelineBuilder<TPipeline, T>
        where TPipeline : IPipeline<T>
    {
        IPipelineBuilder<TPipeline, T> WithStep(IPipe<T> step);
        TPipeline Build();
    }

    public interface IPipelineBuilder<TPipeline, TIn, TOut>
        where TPipeline : IPipeline<TIn, TOut>
    {
        IPipeBuilder<TPipeline, TNextOut, TIn, TOut> WithStep<TNextOut>(IPipe<TIn, TNextOut> step);
    }


}

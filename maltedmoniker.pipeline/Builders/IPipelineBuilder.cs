using maltedmoniker.pipeline.Pipelines;
using System;

namespace maltedmoniker.pipeline.Builders
{
    public interface IPipelineBuilder<TPipeline, T>
        where TPipeline : IPipeline<T>
    {
        IPipelineBuilder<TPipeline, T> WithStep(IPipe<T> step);
        IPipelineBuilder<TPipeline, T> UsingExceptionPipeline(IPipeline<(T, Exception), T> exceptionPipeline);
        TPipeline Build();
    }

    public interface IPipelineBuilder<TPipeline, TIn, TOut>
        where TPipeline : IPipeline<TIn, TOut>
    {
        IPipeBuilder<TPipeline, TNextOut, TIn, TOut> WithStep<TNextOut>(IPipe<TIn, TNextOut> step);
        IPipeBuilder<TPipeline, TNextOut, TIn, TOut> UsingExceptionPipeline<TNextOut>(IPipeline<(TIn, Exception), TOut> exceptionPipeline);
    }


}

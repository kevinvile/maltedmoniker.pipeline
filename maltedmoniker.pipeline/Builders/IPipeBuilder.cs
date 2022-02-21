using maltedmoniker.pipeline.Pipelines;
using System;

namespace maltedmoniker.pipeline.Builders
{
    public interface IPipeBuilder<TPipeline, TIn, TFirstIn, TLastOut>
        where TPipeline : IPipeline<TFirstIn, TLastOut>
    {
        IPipeBuilder<TPipeline, TOut, TFirstIn, TLastOut> WithStep<TOut>(IPipe<TIn, TOut> step);
        IPipeBuilder<TPipeline, TOut, TFirstIn, TLastOut> UsingExceptionPipeline<TOut>(IPipeline<(TFirstIn, Exception), TLastOut> exceptionPipeline);
        TPipeline Build();
    }


}

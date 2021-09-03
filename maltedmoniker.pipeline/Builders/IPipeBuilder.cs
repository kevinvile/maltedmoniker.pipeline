using maltedmoniker.pipeline.Pipelines;

namespace maltedmoniker.pipeline.Builders
{
    public interface IPipeBuilder<TPipeline, TIn, TFirstIn, TLastOut>
        where TPipeline : IPipeline<TFirstIn, TLastOut>
    {
        IPipeBuilder<TPipeline, TOut, TFirstIn, TLastOut> WithStep<TOut>(IPipe<TIn, TOut> step);
        TPipeline Build();
    }


}

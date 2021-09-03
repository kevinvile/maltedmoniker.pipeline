using maltedmoniker.pipeline.Pipelines;
using System;
using System.Collections.Generic;

namespace maltedmoniker.pipeline.Builders
{
    public class PipelineBuilder<TPipeline, T> : IPipelineBuilder<TPipeline, T>
        where TPipeline : IPipeline<T>
    {
        private readonly List<IPipe<T>> steps = new();

        public TPipeline Build()
        {
            var obj = Activator.CreateInstance(typeof(TPipeline), steps);
            if (obj is null) throw new Exception("Unable to create a pipeline!");

            return (TPipeline)obj;
        }

        public IPipelineBuilder<TPipeline, T> WithStep(IPipe<T> step)
        {
            steps.Add(step);
            return this;
        }
    }

    public class PipelineBuilder<TPipeline, TIn, TOut> : IPipelineBuilder<TPipeline, TIn, TOut>
        where TPipeline : IPipeline<TIn, TOut>
    {

        public IPipeBuilder<TPipeline, TNextOut, TIn, TOut> WithStep<TNextOut>(IPipe<TIn, TNextOut> step)
        {
            var steps = new List<IPipe>() { step };

            var obj = Activator.CreateInstance(typeof(PipeBuilder<TPipeline, TNextOut, TIn, TOut>), steps);
            if (obj is null) throw new Exception("Unable to create a new pipe builder!");

            return (PipeBuilder<TPipeline, TNextOut, TIn, TOut>)obj;
        }
    }
}

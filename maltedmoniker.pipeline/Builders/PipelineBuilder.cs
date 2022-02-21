using maltedmoniker.pipeline.Pipelines;
using System;
using System.Collections.Generic;

namespace maltedmoniker.pipeline.Builders
{
    public class PipelineBuilder<TPipeline, T> : IPipelineBuilder<TPipeline, T>
        where TPipeline : IPipeline<T>
    {
        private readonly List<IPipe<T>> steps = new();
        private IPipeline<(T, Exception), T>? _exceptionPipeline;

        public TPipeline Build()
        {
            var obj = Activator.CreateInstance(typeof(TPipeline), steps, _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a pipeline!");

            return (TPipeline)obj;
        }

        public IPipelineBuilder<TPipeline, T> UsingExceptionPipeline(IPipeline<(T, Exception), T> exceptionPipeline)
        {
            _exceptionPipeline = exceptionPipeline;
            return this;
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
        private IPipeline<(TIn, Exception), TOut>? _exceptionPipeline;

        public IPipeBuilder<TPipeline, TNextOut, TIn, TOut> UsingExceptionPipeline<TNextOut>(IPipeline<(TIn, Exception), TOut> exceptionPipeline)
        {
            _exceptionPipeline= exceptionPipeline;
            var obj = Activator.CreateInstance(typeof(PipeBuilder<TPipeline, TNextOut, TIn, TOut>), new List<IPipe>(), _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a new pipe builder!");

            return (PipeBuilder<TPipeline, TNextOut, TIn, TOut>)obj;
        }

        public IPipeBuilder<TPipeline, TNextOut, TIn, TOut> WithStep<TNextOut>(IPipe<TIn, TNextOut> step)
        {
            var steps = new List<IPipe>() { step };

            var obj = Activator.CreateInstance(typeof(PipeBuilder<TPipeline, TNextOut, TIn, TOut>), steps, _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a new pipe builder!");

            return (PipeBuilder<TPipeline, TNextOut, TIn, TOut>)obj;
        }
    }
}

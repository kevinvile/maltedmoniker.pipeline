using maltedmoniker.pipeline.Pipelines;
using System;
using System.Collections.Generic;

namespace maltedmoniker.pipeline.Builders
{
    internal class PipeBuilder<TPipeline, TIn, TFirstIn, TLastOut> : IPipeBuilder<TPipeline, TIn, TFirstIn, TLastOut>
        where TPipeline : IPipeline<TFirstIn, TLastOut>
    {
        private readonly List<IPipe> _steps;
        private IPipeline<(TFirstIn, Exception), TLastOut>? _exceptionPipeline;

        public PipeBuilder(List<IPipe> steps, IPipeline<(TFirstIn, Exception), TLastOut>? exceptionPipeline=null)
        {
            _steps = steps;
            _exceptionPipeline = exceptionPipeline;
        }

        public TPipeline Build()
        {
            if (typeof(TIn) != typeof(TLastOut)) throw new Exception($"Can not build this pipeline, the steps do not end in {typeof(TLastOut).Name}");
            var obj = Activator.CreateInstance(typeof(TPipeline), _steps, _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a pipeline!");

            return (TPipeline)obj;
        }

        public IPipeBuilder<TPipeline, TOut, TFirstIn, TLastOut> UsingExceptionPipeline<TOut>(IPipeline<(TFirstIn, Exception), TLastOut> exceptionPipeline)
        {
            _exceptionPipeline = exceptionPipeline;

            var obj = Activator.CreateInstance(typeof(PipeBuilder<TPipeline, TOut, TFirstIn, TLastOut>), _steps, _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a new pipe builder!");

            return (PipeBuilder<TPipeline, TOut, TFirstIn, TLastOut>)obj;
        }

        public IPipeBuilder<TPipeline, TOut, TFirstIn, TLastOut> WithStep<TOut>(IPipe<TIn, TOut> step)
        {
            _steps.Add(step);
            var obj = Activator.CreateInstance(typeof(PipeBuilder<TPipeline, TOut, TFirstIn, TLastOut>), _steps, _exceptionPipeline);
            if (obj is null) throw new Exception("Unable to create a new pipe builder!");

            return (PipeBuilder<TPipeline, TOut, TFirstIn, TLastOut>)obj;
        }
    }
}

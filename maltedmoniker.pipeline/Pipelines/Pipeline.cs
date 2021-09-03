using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Pipelines
{
    public abstract class BasePipeline<TIn, TOut>
    {
        private Func<TIn, TIn> _preProcessItem = PassThrough;
        private Func<TOut, TOut> _postProcessItem = PassThrough;
        protected Func<TIn, TIn> PreProcessItem => _preProcessItem;
        protected Func<TOut, TOut> PostProcessItem => _postProcessItem;

        public void ChangeItemPreProcessor(Func<TIn, TIn> preProcessItem)
        {
            _preProcessItem = preProcessItem;
        }

        public void ChangeItemPostProcessor(Func<TOut, TOut> postProcessItem)
        {
            _postProcessItem = postProcessItem;
        }
        protected static T PassThrough<T>(T item)
            => item;
    }

    public class Pipeline<T> : BasePipeline<T, T>, IPipeline<T>
    {
        private readonly List<Func<T, CancellationToken, Task<T>>> _pipes;

        public Pipeline(List<IPipe<T>> pipes)
        {
            _pipes = pipes
                .Select<IPipe<T>, Func<T, CancellationToken, Task<T>>>((step) =>
                {
                    if (step is IAsyncPipe<T> asyncStep)
                    {
                        return (item, token) => asyncStep.ExecuteAsync(item, token);
                    }

                    if (step is ISyncPipe<T> syncStep)
                    {
                        return (item, token) => Task.FromResult(syncStep.Execute(item));
                    }

                    return (item, token) => Task.FromResult(item);

                }).ToList();
        }

        public async IAsyncEnumerable<T> Process(IEnumerable<T> items, [EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var item in items)
            {
                var useItem = PreProcessItem.Invoke(item);
                foreach (var step in _pipes)
                {
                    useItem = await step.Invoke(useItem, token);
                }
                var result = PostProcessItem.Invoke(useItem);
                yield return result;
            }
        }
    }

    public class Pipeline<TIn, TOut> : BasePipeline<TIn, TOut>, IPipeline<TIn, TOut>
    {

        private readonly List<PipeAndType> _pipeAndTypes;
        private static readonly Type _asyncStepDefinition = typeof(IAsyncPipe<,>);
        private static readonly Type _syncStepDefinition = typeof(ISyncPipe<,>);

        public Pipeline(List<IPipe> pipes)
        {
            _pipeAndTypes = pipes
                .Select(pipe =>
                {
                    var type = pipe.GetType();
                    var typeInterface = type.GetInterfaces().FirstOrDefault();
                    if (typeInterface is null || !typeInterface.IsGenericType) return null;

                    var genericDefinition = typeInterface.GetGenericTypeDefinition();
                    var stepType = genericDefinition switch
                    {
                        var s when s == _asyncStepDefinition => PipeType.Async,
                        var s when s == _syncStepDefinition => PipeType.Sync,
                        _ => PipeType.Unknown
                    };

                    return new PipeAndType(pipe, stepType);
                })
                .Where(s => s is not null && s.Type != PipeType.Unknown)
                .Select(s => s!)
                .ToList();
        }

        public async virtual IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, [EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var item in items)
            {
                var result = await Process(item, token);
                if (result is null) continue;

                yield return result;
            }
        }

        protected async Task<TOut?> Process(TIn item, CancellationToken token)
        {
            var preProcessed = PreProcessItem.Invoke(item);
            if (preProcessed is null) return default;

            var useItem = (dynamic)preProcessed;
            foreach (var pipeAndType in _pipeAndTypes)
            {
                useItem = pipeAndType.Type switch
                {
                    PipeType.Async => await ((dynamic)pipeAndType.Pipe).ExecuteAsync(useItem, token),
                    PipeType.Sync => ((dynamic)pipeAndType.Pipe).Execute(useItem),
                    _ => useItem
                };
            }
            return (TOut)PostProcessItem.Invoke(useItem);
        }

        private record PipeAndType(IPipe Pipe, PipeType Type);
        private enum PipeType { Unknown, Async, Sync }
    }

}

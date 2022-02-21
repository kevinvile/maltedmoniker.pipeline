using maltedmoniker.pipeline.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline.Pipelines
{
    public abstract class BasePipeline<TIn, TOut>
    {
        protected Func<TIn, TIn> _preProcessItem = (TIn o) => o;// PassThrough;
        protected Func<TOut, TOut> _postProcessItem = (TOut o) => o;
        protected Func<TIn, TIn> PreProcessItem => _preProcessItem;
        protected Func<TOut, TOut> PostProcessItem => _postProcessItem;

        protected MethodInfo _postProcessItemMethod = default!;
        private readonly IPipeline<(TIn, Exception), TOut>? _exceptionPipeline;

        protected BasePipeline(IPipeline<(TIn, Exception), TOut>? exceptionPipeline)
        {
            UpdateMethodInfo();
            _exceptionPipeline = exceptionPipeline;
        }

        public void ChangeItemPreProcessor(Func<TIn, TIn> preProcessItem)
        {
            _preProcessItem = preProcessItem;
        }

        public void ChangeItemPostProcessor(Func<TOut, TOut> postProcessItem)
        {
            _postProcessItem = postProcessItem;
            UpdateMethodInfo();
        }

        private void UpdateMethodInfo()
        {
            _postProcessItemMethod = PostProcessItem.GetType().GetMethod("Invoke") ?? throw new Exception("Unable to get the post process item method!");
        }

        protected static T PassThrough<T>(T item)
            => item;

        protected async Task<TOut> HandleException(TIn item, Exception ex)
        {
            List<(TIn, Exception)> errors = new List<(TIn, Exception)>();
            errors.Add((item, ex));
            if (_exceptionPipeline is not null)
            {
                await foreach (var o in _exceptionPipeline.Process(errors))
                {
                    return o;
                }
            }
            
            return default(TOut?);
        }
    }

    public class Pipeline<T> : BasePipeline<T, T>, IPipeline<T>
    {
        private readonly List<Func<T, CancellationToken, Task<T>>> _pipes;
        private readonly IPipeline<(T, Exception), T>? _exceptionPipeline;

        public Pipeline(List<IPipe<T>> pipes, IPipeline<(T, Exception), T>? exceptionPipeline=null) 
            : base(exceptionPipeline)
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

        private async Task<T?> Process(T item, CancellationToken token)
        {
            try
            {
                var useItem = PreProcessItem.Invoke(item);
                foreach (var step in _pipes)
                {
                    useItem = await step.Invoke(useItem, token);
                }
                return PostProcessItem.Invoke(useItem);
            }
            catch(Exception ex)
            {
                if (_exceptionPipeline is null) throw;
                return await HandleException(item, ex);
            }
        }
    }

    public class Pipeline<TIn, TOut> : BasePipeline<TIn, TOut>, IPipeline<TIn, TOut>
    {
        private readonly List<PipeAndType> _pipeAndTypes;
        private static readonly Type _asyncStepDefinition = typeof(IAsyncPipe<,>);
        private static readonly Type _syncStepDefinition = typeof(ISyncPipe<,>);
        private readonly IPipeline<(TIn, Exception), TOut>? _exceptionPipeline;

        public Pipeline(List<IPipe> pipes, IPipeline<(TIn, Exception), TOut>? exceptionPipeline = null) 
            : base(exceptionPipeline)
        {
            _exceptionPipeline = exceptionPipeline;

            _pipeAndTypes = pipes
                .Select(pipe =>
                {
                    var type = pipe.GetType();
                    var typeInterface = type
                        .GetInterfaces()
                        .FirstOrDefault(t => t.IsGenericType && (t.Name.Contains("IAsyncPipe") || t.Name.Contains("ISyncPipe")));

                    if (typeInterface is null || !typeInterface.IsGenericType) return null;

                    var genericDefinition = typeInterface.GetGenericTypeDefinition();
                    var stepType = genericDefinition switch
                    {
                        var s when s == _asyncStepDefinition => PipeType.Async,
                        var s when s == _syncStepDefinition => PipeType.Sync,
                        _ => PipeType.Unknown
                    };

                    var genericArgs = typeInterface.GetGenericArguments();
                    if (genericArgs.Length != 2) throw new Exception("Invalid pipe!");

                    var tIn = genericArgs[0];
                    var tOut = genericArgs[1];

                    var methodInfo = stepType switch
                    {
                        PipeType.Async => type.GetMethod(nameof(IAsyncPipe<TIn, TOut>.ExecuteAsync)),
                        PipeType.Sync => type.GetMethod(nameof(ISyncPipe<TIn, TOut>.Execute)),
                        _ => type.GetMethod("Execute")
                    } ?? throw new Exception("Can't get pipe's execute method!");

                    return new PipeAndType(pipe, tIn, tOut, methodInfo, stepType);
                })
                .Where(s => s is not null && s.Type != PipeType.Unknown)
                .Select(s => s!)
                .ToList();
            
        }

        public async virtual IAsyncEnumerable<TOut> Process(IEnumerable<TIn> items, [EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var item in items)
            {
                TOut? result = await Process(item, token);
                if (result is null) continue;

                yield return result;
            }
        }

        protected async Task<TOut?> Process(TIn item, CancellationToken token)
        {
            try
            {
                var preProcessed = PreProcessItem.Invoke(item);
                if (preProcessed is null) return default;

                var useItem = (dynamic)preProcessed;
                foreach (var pipeAndType in _pipeAndTypes)
                {
                
                        var method = pipeAndType.Method;
                        useItem = pipeAndType.Type switch
                        {
                            PipeType.Async => await method.InvokeAsync(pipeAndType.Pipe, (object)useItem, token),
                            PipeType.Sync => method.Invoke(pipeAndType.Pipe, BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { useItem }, null),
                            _ => useItem
                        };
                
                }

                return (TOut)_postProcessItemMethod.Invoke(PostProcessItem, new object[] { useItem });
            }
            catch (Exception ex)
            {
                if (_exceptionPipeline is null) throw;
                return await HandleException(item, ex);
            }
        }

        private record PipeAndType(IPipe Pipe, Type In, Type Out, MethodInfo Method, PipeType Type);
        private enum PipeType { Unknown, Async, Sync }
    }

}

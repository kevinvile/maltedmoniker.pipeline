using System.Threading;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline
{
    public interface IPipe { }
    public interface IPipe<T> : IPipe { }

    public interface ISyncPipe<T> : IPipe<T>
    {
        T Execute(T item, PipelineContext? context);
    }

    public interface IAsyncPipe<T> : IPipe<T>
    {
        Task<T> ExecuteAsync(T item, PipelineContext? context, CancellationToken token = default);
    }

    public interface IPipe<TIn, TOut> : IPipe { }

    public interface ISyncPipe<TIn, TOut> : IPipe<TIn, TOut>
    {
        TOut Execute(TIn item, PipelineContext? context);
    }

    public interface IAsyncPipe<TIn, TOut> : IPipe<TIn, TOut>
    {
        Task<TOut> ExecuteAsync(TIn item, PipelineContext? context, CancellationToken token = default);
    }

}

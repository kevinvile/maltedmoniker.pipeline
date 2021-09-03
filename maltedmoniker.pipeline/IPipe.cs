using System.Threading;
using System.Threading.Tasks;

namespace maltedmoniker.pipeline
{
    public interface IPipe { }
    public interface IPipe<T> : IPipe { }

    public interface ISyncPipe<T> : IPipe<T>
    {
        T Execute(T item);
    }

    public interface IAsyncPipe<T> : IPipe<T>
    {
        Task<T> ExecuteAsync(T item, CancellationToken token = default);
    }

    public interface IPipe<TIn, TOut> : IPipe { }

    public interface ISyncPipe<TIn, TOut> : IPipe<TIn, TOut>
    {
        TOut Execute(TIn item);
    }

    public interface IAsyncPipe<TIn, TOut> : IPipe<TIn, TOut>
    {
        Task<TOut> ExecuteAsync(TIn item, CancellationToken token = default);
    }

}

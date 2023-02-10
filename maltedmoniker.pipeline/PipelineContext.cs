using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace maltedmoniker.pipeline
{
    public sealed class PipelineContext : IDisposable
    {
        private Dictionary<string, object> _data;
        private bool _disposedValue;

        public int ItemsProcessed { get; private set; }
        public PipelineItemProcessContext? CurrentItemContext { get; private set; }

        internal PipelineContext()
        {
            _data = new Dictionary<string, object>();
        }

        internal void Clear()
        {
            ItemsProcessed = 0;
            _data = new Dictionary<string, object>();
        }

        internal void StartItem(object? item)
        {
            ItemsProcessed += 1;
            if (CurrentItemContext is not null) CurrentItemContext.Dispose();

            CurrentItemContext = new PipelineItemProcessContext(item);
        }

        internal void StartPipe(Type typeIn, Type typeOut, object? item, IPipe? pipe=null)
        {
            CurrentItemContext?.PushPipe(pipe, typeIn, typeOut, item);
        }

        internal void EndPipe(object? itemOut)
        {
            CurrentItemContext?.SetItemOut(itemOut);
        }

        internal void EndItem(object? endItem)
        {
            var context = CurrentItemContext;
            if (context is not null)
            {
                context.EndingItem = endItem;
            }
        }

        public void StoreValue(string key, object value)
        {
            _data[key] = value;
        }

        public object? GetValue(string key)
        {
            if (!_data.ContainsKey(key)) return null;

            return _data[key];
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if(CurrentItemContext is not null) CurrentItemContext.Dispose();
                    _data.Clear();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class PipelineItemProcessContext : IDisposable
    {
        public int PipesProcessedCount => _pipesProcessed.Count;
        public object? StartingItem { get; }
        public object? EndingItem { get; internal set; }

        private readonly ConcurrentStack<PipeInfo> _pipesProcessed;
        private bool _disposedValue;

        public PipelineItemProcessContext(object? startingItem)
        {
            StartingItem = startingItem;
            _pipesProcessed = new ConcurrentStack<PipeInfo>();
        }

        public PipeInfo? CurrentPipe()
        {
            _pipesProcessed.TryPeek(out PipeInfo? pipe);
            return pipe;
        }
        internal void PushPipe(IPipe? pipe, Type typeIn, Type typeOut, object? inItem)
        {
            var pipeInfo = new PipeInfo(pipe, typeIn, typeOut, inItem);
            _pipesProcessed.Push(pipeInfo);
        }

        internal void SetItemOut(object? item)
        {
            var pipe = CurrentPipe();
            if(pipe is not null)
            {
                pipe.ItemOut = item;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _pipesProcessed.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class PipeInfo
    {
        public IPipe? Pipe { get; }
        public Type TypeIn { get; }
        public Type TypeOut { get; }
        public object? ItemIn { get; }
        public object? ItemOut { get; internal set; }

        public PipeInfo(IPipe? pipe, Type typeIn, Type typeOut, object? itemIn)
        {
            Pipe = pipe;
            TypeIn = typeIn;
            TypeOut = typeOut;
            ItemIn = itemIn;
        }
    }
}

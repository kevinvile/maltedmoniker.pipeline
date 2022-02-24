using System;
using System.Collections.Generic;
using System.Linq;

namespace maltedmoniker.pipeline
{
    public sealed class PipelineContext
    {
        public int ItemsProcessed => _processContexts.Count;
        public PipelineItemProcessContext CurrentItemContext => _processContexts.Last();
        private List<PipelineItemProcessContext> _processContexts;
        private Dictionary<string, object> _data;

        internal PipelineContext()
        {
            _data = new Dictionary<string, object>();
            _processContexts = new List<PipelineItemProcessContext>();
        }

        internal void Clear()
        {
            _data = new Dictionary<string, object>();
            _processContexts = new List<PipelineItemProcessContext>();
        }

        internal void StartItem(object? item)
        {
            var newItemContext = new PipelineItemProcessContext(item);
            _processContexts.Add(newItemContext);
        }

        internal void StartPipe(Type typeIn, Type typeOut, object? item, IPipe? pipe=null)
        {
            CurrentItemContext.PushPipe(pipe, typeIn, typeOut, item);
        }

        internal void EndPipe(object? itemOut)
        {
            CurrentItemContext.SetItemOut(itemOut);
        }

        internal void EndItem(object? endItem)
        {
            CurrentItemContext.EndingItem = endItem;
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
    }

    public sealed class PipelineItemProcessContext
    {
        public int PipesProcessedCount => _pipesProcessed.Count;
        public object? StartingItem { get; }
        public object? EndingItem { get; internal set; }

        private readonly Stack<PipeInfo> _pipesProcessed;

        public PipelineItemProcessContext(object? startingItem)
        {
            StartingItem = startingItem;
            _pipesProcessed = new Stack<PipeInfo>();
        }

        public PipeInfo CurrentPipe() => _pipesProcessed.Peek();
        internal void PushPipe(IPipe? pipe, Type typeIn, Type typeOut, object? inItem)
        {
            var pipeInfo = new PipeInfo(pipe, typeIn, typeOut, inItem);
            _pipesProcessed.Push(pipeInfo);
        }

        //TODO (kevin): Somehow CurrentPipe() returned null in TestConsole
        internal void SetItemOut(object? item) => CurrentPipe().ItemOut = item;
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

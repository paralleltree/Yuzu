using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Events;

namespace Yuzu.UI.Operations
{
    internal abstract class EventOperationBase<T> : IOperation where T : EventBase
    {
        public abstract string Description { get; }
        protected T Value { get; }

        public EventOperationBase(T value)
        {
            Value = value;
        }

        public abstract void Undo();
        public abstract void Redo();
    }

    internal abstract class EventCollectionOperationBase<T> : EventOperationBase<T> where T : EventBase
    {
        protected List<T> Collection { get; }

        public EventCollectionOperationBase(T value, List<T> collection) : base(value)
        {
            Collection = collection;
        }
    }

    internal class AddEventOperation<T> : EventCollectionOperationBase<T> where T : EventBase
    {
        public override string Description => "イベント挿入";

        public AddEventOperation(T value, List<T> collection) : base(value, collection)
        {
        }

        public override void Redo()
        {
            Collection.Add(Value);
        }

        public override void Undo()
        {
            Collection.Remove(Value);
        }
    }

    internal class RemoveEventOperation<T> : EventCollectionOperationBase<T> where T : EventBase
    {
        public override string Description => "イベント挿入";

        public RemoveEventOperation(T value, List<T> collection) : base(value, collection)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Value);
        }

        public override void Undo()
        {
            Collection.Add(Value);
        }
    }
}

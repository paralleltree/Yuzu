using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Track;

namespace Yuzu.UI.Operations
{
    internal abstract class FieldObjectOperationBase<T> : IOperation where T : FieldObject
    {
        public abstract string Description { get; }
        protected T FieldObject { get; }

        protected FieldObjectOperationBase(T obj)
        {
            FieldObject = obj;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class MoveFieldObjectOperation : FieldObjectOperationBase<FieldObject>
    {
        public override string Description => "オブジェクトの移動";
        protected FieldPoint BeforePosition { get; }
        protected FieldPoint AfterPosition { get; }

        public MoveFieldObjectOperation(FieldObject obj, int beforeTick, int beforeOffset, int afterTick, int afterOffset)
            : base(obj)
        {
            BeforePosition = new FieldPoint() { Tick = beforeTick, LaneOffset = beforeOffset };
            AfterPosition = new FieldPoint() { Tick = afterTick, LaneOffset = afterOffset };
        }

        public override void Redo()
        {
            FieldObject.Position.Tick = AfterPosition.Tick;
            FieldObject.Position.LaneOffset = AfterPosition.LaneOffset;
        }

        public override void Undo()
        {
            FieldObject.Position.Tick = BeforePosition.Tick;
            FieldObject.Position.LaneOffset = BeforePosition.LaneOffset;
        }
    }

    internal class AddFieldObjectOperation<T> : FieldObjectOperationBase<T> where T : FieldObject
    {
        public override string Description => "オブジェクトの追加";
        protected List<T> Collection { get; }

        public AddFieldObjectOperation(T obj, List<T> collection) : base(obj)
        {
            Collection = collection;
        }

        public override void Redo()
        {
            Collection.Add(FieldObject);
        }

        public override void Undo()
        {
            Collection.Remove(FieldObject);
        }
    }
}

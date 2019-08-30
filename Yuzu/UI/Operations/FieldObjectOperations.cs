using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Track;

namespace Yuzu.UI.Operations
{
    internal abstract class FieldObjectOperationBase : IOperation
    {
        public abstract string Description { get; }
        protected FieldObject FieldObject { get; }

        protected FieldObjectOperationBase(FieldObject obj)
        {
            FieldObject = obj;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class MoveFieldObject : FieldObjectOperationBase
    {
        public override string Description => "オブジェクトの移動";
        protected FieldPoint BeforePosition { get; }
        protected FieldPoint AfterPosition { get; }

        public MoveFieldObject(FieldObject obj, int beforeTick, int beforeOffset, int afterTick, int afterOffset)
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
}

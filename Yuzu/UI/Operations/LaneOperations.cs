using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Track;
using Yuzu.UI.Data.Track;

namespace Yuzu.UI.Operations
{
    internal abstract class LaneStepOperationBase : IOperation
    {
        public abstract string Description { get; }
        protected FieldPoint FieldPoint { get; }

        protected LaneStepOperationBase(FieldPoint point)
        {
            FieldPoint = point;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class MoveLaneStepOperation : LaneStepOperationBase
    {
        public override string Description => "レーン移動";
        protected FieldPoint Before { get; }
        protected FieldPoint After { get; }

        public MoveLaneStepOperation(FieldPoint point, int beforeTick, int beforeOffset, int afterTick, int afterOffset) : base(point)
        {
            Before = new FieldPoint() { Tick = beforeTick, LaneOffset = beforeOffset };
            After = new FieldPoint() { Tick = afterTick, LaneOffset = afterOffset };
        }

        public override void Redo()
        {
            FieldPoint.Tick = After.Tick;
            FieldPoint.LaneOffset = After.LaneOffset;
        }

        public override void Undo()
        {
            FieldPoint.Tick = Before.Tick;
            FieldPoint.LaneOffset = Before.LaneOffset;
        }
    }

    internal abstract class SideLaneGuideOperationBase : IOperation
    {
        public abstract string Description { get; }
        protected Data.Track.SideLane SideLane { get; }

        protected SideLaneGuideOperationBase(Data.Track.SideLane lane)
        {
            SideLane = lane;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class ChangeSideLaneGuideRangeOperation : SideLaneGuideOperationBase
    {
        public override string Description => "ガイドライン範囲の変更";
        protected TickRange Before { get; }
        protected TickRange After { get; }

        public ChangeSideLaneGuideRangeOperation(Data.Track.SideLane lane, int beforeStart, int beforeDuration, int afterStart, int afterDuration)
            : base(lane)
        {
            Before = new TickRange() { StartTick = beforeStart, Duration = beforeDuration };
            After = new TickRange() { StartTick = afterStart, Duration = afterDuration };
        }

        public override void Redo()
        {
            SideLane.ValidRange.StartTick = After.StartTick;
            SideLane.ValidRange.Duration = After.Duration;
        }

        public override void Undo()
        {
            SideLane.ValidRange.StartTick = Before.StartTick;
            SideLane.ValidRange.Duration = Before.Duration;
        }
    }
}

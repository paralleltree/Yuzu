using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Collections;
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

    internal class InsertLaneStepOperation : LaneStepOperationBase
    {
        public override string Description => "レーン制御点の追加";
        protected AVLTree<FieldPoint> Collection { get; }

        public InsertLaneStepOperation(FieldPoint fp, AVLTree<FieldPoint> collection) : base(fp)
        {
            Collection = collection;
        }

        public override void Redo()
        {
            Collection.Add(FieldPoint);
        }

        public override void Undo()
        {
            Collection.Remove(FieldPoint);
        }
    }

    internal class RemoveLaneStepOperation : LaneStepOperationBase
    {
        public override string Description => "レーン制御点の削除";
        protected AVLTree<FieldPoint> Collection { get; }

        public RemoveLaneStepOperation(FieldPoint fp, AVLTree<FieldPoint> collection) : base(fp)
        {
            Collection = collection;
        }

        public override void Redo()
        {
            Collection.Remove(FieldPoint);
        }

        public override void Undo()
        {
            Collection.Add(FieldPoint);
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

    internal class InsertSideLaneOperation : SideLaneGuideOperationBase
    {
        public override string Description => "サイドレーンの追加";
        protected AVLTree<Data.Track.SideLane> Collection { get; }

        public InsertSideLaneOperation(Data.Track.SideLane lane, AVLTree<Data.Track.SideLane> collection) : base(lane)
        {
            Collection = collection;
        }

        public override void Redo()
        {
            Collection.Add(SideLane);
        }

        public override void Undo()
        {
            Collection.Remove(SideLane);
        }
    }

    internal class RemoveSideLaneOperation : SideLaneGuideOperationBase
    {
        public override string Description => "サイドレーンの削除";
        protected AVLTree<Data.Track.SideLane> Collection { get; }

        public RemoveSideLaneOperation(Data.Track.SideLane lane, AVLTree<Data.Track.SideLane> collection) : base(lane)
        {
            Collection = collection;
        }

        public override void Redo()
        {
            Collection.Remove(SideLane);
        }

        public override void Undo()
        {
            Collection.Add(SideLane);
        }
    }

    internal abstract class SurfaceLaneCollectionOperationBase : IOperation
    {
        public abstract string Description { get; }
        protected Data.Track.SurfaceLane SurfaceLane { get; }
        protected List<Data.Track.SurfaceLane> Collection { get; }

        protected SurfaceLaneCollectionOperationBase(Data.Track.SurfaceLane lane, List<Data.Track.SurfaceLane> collection)
        {
            SurfaceLane = lane;
            Collection = collection;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class AddSurfaceLaneOperation : SurfaceLaneCollectionOperationBase
    {
        public override string Description => "レーンの追加";

        public AddSurfaceLaneOperation(Data.Track.SurfaceLane lane, List<Data.Track.SurfaceLane> collection)
            : base(lane, collection)
        {
        }

        public override void Redo()
        {
            Collection.Add(SurfaceLane);
        }

        public override void Undo()
        {
            Collection.Remove(SurfaceLane);
        }
    }

    internal class RemoveSurfaceLaneOperation : SurfaceLaneCollectionOperationBase
    {
        public override string Description => "レーンの削除";

        public RemoveSurfaceLaneOperation(Data.Track.SurfaceLane lane, List<Data.Track.SurfaceLane> collection)
            : base(lane, collection)
        {
        }

        public override void Redo()
        {
            Collection.Remove(SurfaceLane);
        }

        public override void Undo()
        {
            Collection.Add(SurfaceLane);
        }
    }
}

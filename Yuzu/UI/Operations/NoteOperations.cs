﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core.Track;

namespace Yuzu.UI.Operations
{
    internal abstract class NoteOperationBase : IOperation
    {
        public abstract string Description { get; }
        protected Note Note { get; }

        protected NoteOperationBase(Note note)
        {
            Note = note;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    internal class MoveNoteOperation : NoteOperationBase
    {
        public override string Description => "ノートの移動";
        protected TickRange Before { get; }
        protected TickRange After { get; }

        public MoveNoteOperation(Note note, int beforeStart, int beforeDuration, int afterStart, int afterDuration)
            : base(note)
        {
            Before = new TickRange() { StartTick = beforeStart, Duration = beforeDuration };
            After = new TickRange() { StartTick = afterStart, Duration = afterDuration };
        }

        public override void Redo()
        {
            Note.TickRange.StartTick = After.StartTick;
            Note.TickRange.Duration = After.Duration;
        }

        public override void Undo()
        {
            Note.TickRange.StartTick = Before.StartTick;
            Note.TickRange.Duration = Before.Duration;
        }
    }
}

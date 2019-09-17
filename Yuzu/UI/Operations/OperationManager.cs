using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.UI.Operations
{
    internal class OperationManager
    {
        protected Stack<IOperation> UndoStack { get; } = new Stack<IOperation>();
        protected Stack<IOperation> RedoStack { get; } = new Stack<IOperation>();
        protected IOperation LastCommittedOperation { get; set; }

        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;
        public bool IsChanged => LastCommittedOperation != (UndoStack.Count > 0 ? UndoStack.Peek() : null);

        public event EventHandler OperationHistoryChanged;
        public event EventHandler ChangesCommitted;

        public void Push(IOperation op)
        {
            UndoStack.Push(op);
            RedoStack.Clear();
            OperationHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ExecuteAndPush(IOperation op)
        {
            op.Redo();
            Push(op);
        }

        public void Undo()
        {
            IOperation op = UndoStack.Pop();
            op.Undo();
            RedoStack.Push(op);
            OperationHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            IOperation op = RedoStack.Pop();
            op.Redo();
            UndoStack.Push(op);
            OperationHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            LastCommittedOperation = null;
            OperationHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CommitChanges()
        {
            LastCommittedOperation = UndoStack.Count > 0 ? UndoStack.Peek() : null;
            ChangesCommitted?.Invoke(this, EventArgs.Empty);
        }
    }
}

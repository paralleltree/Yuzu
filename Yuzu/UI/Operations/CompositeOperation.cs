using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.UI.Operations
{
    internal class CompositeOperation : IOperation
    {
        public string Description { get; }
        protected ICollection<IOperation> Operations { get; }

        public CompositeOperation(string description, params IOperation[] operations)
        {
            Description = description;
            Operations = operations;
        }

        public void Redo()
        {
            foreach (var op in Operations)
                op.Redo();
        }

        public void Undo()
        {
            foreach (var op in Operations.Reverse())
                op.Undo();
        }
    }
}

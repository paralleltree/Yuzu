using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.UI.Operations
{
    /// <summary>
    /// ユーザの操作を表すインターフェースです。
    /// </summary>
    internal interface IOperation
    {
        /// <summary>
        /// この操作の説明を取得します。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 操作を元に戻します。
        /// </summary>
        void Undo();

        /// <summary>
        /// 操作をやり直します。
        /// </summary>
        void Redo();
    }
}

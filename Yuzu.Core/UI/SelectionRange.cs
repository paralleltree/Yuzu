using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.UI
{
    /// <summary>
    /// 譜面における選択範囲を表します。
    /// </summary>
    public struct SelectionRange
    {
        /// <summary>
        /// 選択を開始したTickを設定します。
        /// </summary>
        public int StartTick { get; set; }

        /// <summary>
        /// <see cref="StartTick"/>とのオフセットを表すTickを設定します。
        /// この値が負であるとき、<see cref="StartTick"/>よりも前の範囲が選択されていることを表します。
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 選択されたレーンの左端のインデックスを設定します。
        /// </summary>
        public int StartLaneIndex { get; set; }

        /// <summary>
        /// <see cref="StartLaneIndex"/>を起点に選択されているレーン数を設定します。
        /// </summary>
        public int SelectedLanesCount { get; set; }
    }
}

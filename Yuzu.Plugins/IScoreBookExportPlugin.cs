using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yuzu.Core;

namespace Yuzu.Plugins
{
    /// <summary>
    /// <see cref="ScoreBook"/>のエクスポートを行うプラグインを表します。
    /// </summary>
    public interface IScoreBookExportPlugin : IPlugin
    {
        /// <summary>
        /// 拡張子のフィルタ文字列を取得します。
        /// </summary>
        string Filter { get; }

        /// <summary>
        /// データのエクスポートを行います。
        /// </summary>
        /// <param name="args"></param>
        void Export(IScoreBookExportPluginArgs args);
    }

    public interface IScoreBookExportPluginArgs
    {
        string OutputPath { get; }
        ScoreBook GetScoreBook();
    }
}

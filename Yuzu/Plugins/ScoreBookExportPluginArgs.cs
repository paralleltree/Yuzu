using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzu.Core;

namespace Yuzu.Plugins
{
    public class ScoreBookExportPluginArgs : IScoreBookExportPluginArgs
    {
        private Func<ScoreBook> getScoreBookFunc;

        public string OutputPath { get; internal set; }

        public ScoreBookExportPluginArgs(Func<ScoreBook> getScoreBookFunc)
        {
            this.getScoreBookFunc = getScoreBookFunc;
        }

        public ScoreBook GetScoreBook() => getScoreBookFunc();
    }
}

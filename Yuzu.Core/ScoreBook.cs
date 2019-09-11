using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yuzu.Core
{
    /// <summary>
    /// 譜面ファイルを表します。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class ScoreBook
    {
        internal static Version CurrentVersion => typeof(ScoreBook).Assembly.GetName().Version;
        internal readonly static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        [Newtonsoft.Json.JsonProperty]
        private Version version = CurrentVersion;
        [Newtonsoft.Json.JsonProperty]
        private string title = "";
        [Newtonsoft.Json.JsonProperty]
        private string artistName = "";
        [Newtonsoft.Json.JsonProperty]
        private string notesDesignerName = "";
        [Newtonsoft.Json.JsonProperty]
        private Score score = new Score();

        public string Path { get; set; }

        /// <summary>
        /// このファイルのバージョンを取得します。
        /// </summary>
        public Version Version
        {
            get => version;
            set => version = value;
        }

        /// <summary>
        /// 楽曲のタイトルを設定します。
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        /// 楽曲のアーティストを設定します。
        /// </summary>
        public string ArtistName
        {
            get => artistName;
            set => artistName = value;
        }

        /// <summary>
        /// ノーツデザイナーを設定します。
        /// </summary>
        public string NotesDesignerName
        {
            get => notesDesignerName;
            set => notesDesignerName = value;
        }

        /// <summary>
        /// 譜面のデータを設定します。
        /// </summary>
        public Score Score
        {
            get => score;
            set => score = value;
        }


        public void Save(string path)
        {
            Path = path;
            Save();
        }

        /// <summary>
        /// データを<see cref="Path"/>で指定された場所へ書き込みます。
        /// </summary>
        public void Save()
        {
            string data = JsonConvert.SerializeObject(this, SerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var stream = new MemoryStream(bytes))
            using (var file = new FileStream(Path, FileMode.Create))
            using (var gz = new GZipStream(file, CompressionMode.Compress))
            {
                stream.CopyTo(gz);
            }
        }

        /// <summary>
        /// 指定のファイルから<see cref="ScoreBook"/>のインスタンスを生成します。
        /// 古いバージョンのファイルは現在のバージョン用に変換されます。
        /// </summary>
        /// <param name="path">読み込むファイルへのパス</param>
        /// <returns>読み込まれた<see cref="ScoreBook"/></returns>
        public static ScoreBook LoadFile(string path)
        {
            if (!IsCompatible(path)) throw new ArgumentException("The file is incompatible.");
            string data = GetDecompressedData(path);
            var doc = JObject.Parse(data);

            var res = doc.ToObject<ScoreBook>(JsonSerializer.Create(SerializerSettings));
            res.Path = path;
            return res;
        }

        /// <summary>
        /// 指定のファイルのバージョンが現在のバージョンと互換性があるかどうか判定します。
        /// </summary>
        /// <param name="path">判定するファイルへのパス</param>
        /// <returns>互換性があればtrue, 互換性がなければfalse</returns>
        public static bool IsCompatible(string path)
        {
            return GetFileVersion(path).Major <= CurrentVersion.Major;
        }

        /// <summary>
        /// 指定のファイルを読み込むためにバージョンアップが必要かどうか判定します。
        /// </summary>
        /// <param name="path">判定するファイルへのパス</param>
        /// <returns>バージョンアップが必要であればtrue, 必要なければfalse</returns>
        public static bool IsUpgradeNeeded(string path)
        {
            return GetFileVersion(path).Major != CurrentVersion.Major;
        }

        private static Version GetFileVersion(string path)
        {
            var doc = JObject.Parse(GetDecompressedData(path));
            return doc["version"].ToObject<Version>();
        }

        private static string GetDecompressedData(string path)
        {
            using (var file = new FileStream(path, FileMode.Open))
            using (var gz = new GZipStream(file, CompressionMode.Decompress))
            using (var stream = new MemoryStream())
            {
                gz.CopyTo(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public ScoreBook Clone()
        {
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(this, SerializerSettings);
            var book = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoreBook>(serialized);
            book.Path = Path;
            return book;
        }
    }
}

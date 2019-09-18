using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using Yuzu.Media;

namespace Yuzu.Configuration
{
    internal sealed class SoundSettings : SettingsBase
    {
        public static SoundSettings Default { get; } = (SoundSettings)Synchronized(new SoundSettings());

        private SoundSettings()
        {
            if (ClapSource == null)
                ClapSource = new SoundSource("guide.mp3", 0.036);
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public SoundSource ClapSource
        {
            get => (SoundSource)this["ClapSource"];
            set => this["ClapSource"] = value;
        }

        // ref: https://stackoverflow.com/a/12807699
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")] // empty dictionary
        public Dictionary<string, SoundSource> ScoreSounds
        {
            get => (Dictionary<string, SoundSource>)this["ScoreSounds"];
            set => this["ScoreSounds"] = value;
        }
    }
}

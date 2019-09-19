using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Yuzu.Configuration
{
    internal sealed class ApplicationSettings : SettingsBase
    {
        public static ApplicationSettings Default { get; } = (ApplicationSettings)Synchronized(new ApplicationSettings());

        private ApplicationSettings()
        {
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool PreviewOnlyNotes
        {
            get => (bool)this["PreviewOnlyNotes"];
            set => this["PreviewOnlyNotes"] = value;
        }
    }
}

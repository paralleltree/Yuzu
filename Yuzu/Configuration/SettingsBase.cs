using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Yuzu.Configuration
{
    internal interface IUpgradable
    {
        bool HasUpgraded { get; }
    }

    internal abstract class SettingsBase : ApplicationSettingsBase, IUpgradable
    {
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool HasUpgraded
        {
            get => (bool)this["HasUpgraded"];
            private set => this["HasUpgraded"] = value;
        }

        public override void Upgrade()
        {
            base.Upgrade();
            HasUpgraded = true;
            Save();
        }
    }
}

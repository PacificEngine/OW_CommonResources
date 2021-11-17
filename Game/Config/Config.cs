using OWML.Common;
using System;

namespace PacificEngine.OW_CommonResources.Config
{
    public static class ConfigHelper
    {
        public static T getConfigOrDefault<T>(IModConfig config, string id, T defaultValue)
        {
            try
            {
                var sValue = config.GetSettingsValue<T>(id);
                if (sValue == null)
                {
                    config.SetSettingsValue(id, defaultValue);
                    return defaultValue;
                }
                if (sValue is string && ((string)(object)sValue).Length < 1)
                {
                    config.SetSettingsValue(id, defaultValue);
                    return defaultValue;
                }
                return sValue;
            }
            catch (Exception)
            {
                config.SetSettingsValue(id, defaultValue);
                return defaultValue;
            };
        }
    }
}

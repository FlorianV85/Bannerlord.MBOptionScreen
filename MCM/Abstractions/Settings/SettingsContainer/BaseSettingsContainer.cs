﻿using MCM.Abstractions.Settings.Definitions;
using MCM.Abstractions.Settings.Formats;
using MCM.Utils;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Engine;

using Path = System.IO.Path;

namespace MCM.Abstractions.Settings.SettingsContainer
{
    public abstract class BaseSettingsContainer<TSettings> : ISettingsContainer where TSettings : BaseSettings
    {
        protected virtual string RootFolder { get; } = Path.Combine(Utilities.GetConfigsPath(), "ModSettings");
        protected Dictionary<string, ISettingsFormat> AvailableSettingsFormats { get; } = new Dictionary<string, ISettingsFormat>();
        protected Dictionary<string, TSettings> LoadedSettings { get; } = new Dictionary<string, TSettings>();

        public List<SettingsDefinition> CreateModSettingsDefinitions => LoadedSettings.Keys
            .Select(id => new SettingsDefinition(id))
            .OrderByDescending(a => a.DisplayName)
            .ToList();

        protected BaseSettingsContainer()
        {
            foreach (var format in DI.GetImplementations<ISettingsFormat, SettingFormatWrapper>(ApplicationVersionUtils.GameVersion()))
            foreach (var extension in format.Extensions)
            {
                AvailableSettingsFormats[extension] = format;
            }

            AvailableSettingsFormats.Add("memory", new MemorySettingsFormat());
        }

        protected virtual void RegisterSettings(TSettings tSettings)
        {
            if (tSettings == null || LoadedSettings.ContainsKey(tSettings.Id))
                return;

            LoadedSettings.Add(tSettings.Id, tSettings);

            var path = Path.Combine(RootFolder, tSettings.FolderName, tSettings.SubFolder ?? "", $"{tSettings.Id}.{tSettings.Format}");
            if (AvailableSettingsFormats.ContainsKey(tSettings.Format))
                AvailableSettingsFormats[tSettings.Format].Load(tSettings, path);
            else
                AvailableSettingsFormats["memory"].Load(tSettings, path);
        }

        public BaseSettings? GetSettings(string id) => LoadedSettings.TryGetValue(id, out var result) ? result : null;
        public virtual bool SaveSettings(BaseSettings settings)
        {
            if (!(settings is TSettings tSettings) || !LoadedSettings.ContainsKey(tSettings.Id))
                return false;

            var path = Path.Combine(RootFolder, tSettings.FolderName, tSettings.SubFolder ?? "", $"{tSettings.Id}.{tSettings.Format}");
            if (AvailableSettingsFormats.ContainsKey(tSettings.Format))
                AvailableSettingsFormats[tSettings.Format].Save(tSettings, path);
            else
                AvailableSettingsFormats["memory"].Save(tSettings, path);

            return true;
        }

        public virtual bool OverrideSettings(BaseSettings settings)
        {
            if (!(settings is TSettings tSettings) || !LoadedSettings.ContainsKey(tSettings.Id))
                return false;

            SettingsUtils.OverrideSettings(LoadedSettings[tSettings.Id], tSettings);
            return true;
        }
        public virtual bool ResetSettings(BaseSettings settings)
        {
            if (!(settings is TSettings tSettings) || !LoadedSettings.ContainsKey(tSettings.Id))
                return false;

            SettingsUtils.ResetSettings(LoadedSettings[tSettings.Id]);
            return true;
        }
    }
}
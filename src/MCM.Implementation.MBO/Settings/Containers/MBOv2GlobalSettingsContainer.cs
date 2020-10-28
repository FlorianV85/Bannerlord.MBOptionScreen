﻿extern alias v2;
extern alias v4;

using Bannerlord.ButterLib.Common.Extensions;

using MCM.Implementation.MBO.Settings.Base;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using v4::MCM.Abstractions.Settings.Base.Global;
using v4::MCM.Abstractions.Settings.Containers.Global;
using v4::MCM.Abstractions.Settings.Formats;

namespace MCM.Implementation.MBO.Settings.Containers
{
    internal sealed class MBOv2GlobalSettingsContainer : BaseGlobalSettingsContainer
    {
        private static List<GlobalSettings>? Settings { get; set; }

        public MBOv2GlobalSettingsContainer()
        {
            if (Settings == null)
            {
                Settings = new List<GlobalSettings>();

                var allTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    // ignore v1 and v2 classes
                    .Where(a => !Path.GetFileNameWithoutExtension(a.Location).StartsWith("MBOptionScreen"))
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                var mbOptionScreenSettings = allTypes
                    .Where(t => typeof(v2::MBOptionScreen.Settings.SettingsBase).IsAssignableFrom(t))
                    .Select(obj => new MBOv2GlobalSettingsWrapper(Activator.CreateInstance(obj)));
                Settings.AddRange(mbOptionScreenSettings);
            }

            foreach (var setting in Settings)
                RegisterSettings(setting);
        }

        protected override void RegisterSettings(GlobalSettings settings)
        {
            if (settings == null || LoadedSettings.ContainsKey(settings.Id))
                return;

            LoadedSettings.Add(settings.Id, settings);

            var directoryPath = Path.Combine(RootFolder, settings.FolderName, settings.SubFolder);
            var serviceProvider = v4::MCM.MCMSubModule.Instance?.GetServiceProvider() ?? v4::MCM.MCMSubModule.Instance?.GetTempServiceProvider();
            var settingsFormats = serviceProvider.GetRequiredService<IEnumerable<ISettingsFormat>>() ?? Enumerable.Empty<ISettingsFormat>();
            var settingsFormat = settingsFormats.FirstOrDefault(x => x.FormatTypes.Any(y => y == settings.FormatType));
            settingsFormat?.Load(settings, directoryPath, settings.Id);
        }
    }
}
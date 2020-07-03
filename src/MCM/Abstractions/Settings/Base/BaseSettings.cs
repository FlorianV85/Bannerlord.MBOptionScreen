﻿using HarmonyLib;

using MCM.Abstractions.Settings.Models;
using MCM.Abstractions.Settings.Properties;
using MCM.Extensions;
using MCM.Utils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using TaleWorlds.Localization;

namespace MCM.Abstractions.Settings.Base
{
    public abstract class BaseSettings : INotifyPropertyChanged
    {
        public const string SaveTriggered = "SAVE_TRIGGERED";
        /// <inheritdoc/>
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Unique identifier used to save to file. Make sure this is unique to your mod.
        /// </summary>
        public abstract string Id { get; }
        /// <summary>
        /// The display name of the setting in the settings menu.
        /// </summary>
        public abstract string DisplayName { get; }
        public virtual string FolderName { get; } = string.Empty;
        /// <summary>
        /// If you want this settings file stored inside a subfolder, set this to the name of the subfolder.
        /// </summary>
        public virtual string SubFolder => string.Empty;
        public virtual string Format => "json";
        public virtual int UIVersion => 1;
        protected virtual char SubGroupDelimiter => '/';
        protected virtual ISettingsPropertyDiscoverer? Discoverer { get; } = null;

        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual BaseSettings CreateNew()
        {
            var type = GetType();
            var constructor = AccessTools.Constructor(type, Type.EmptyTypes);
            return constructor != null
                ? (BaseSettings) constructor.Invoke(null)
                : (BaseSettings) FormatterServices.GetUninitializedObject(type);
        }
        protected virtual BaseSettings CopyAsNew()
        {
            var newSettings = CreateNew();
            SettingsUtils.OverrideSettings(newSettings, this);
            return newSettings;
        }
        public virtual IDictionary<string, Func<BaseSettings>> GetAvailablePresets() => new Dictionary<string, Func<BaseSettings>>()
        {
            { new TextObject("{=BaseSettings_Default}Default").ToString(), CreateNew }
        };

        public virtual List<SettingsPropertyGroupDefinition> GetSettingPropertyGroups() => GetUnsortedSettingPropertyGroups()
            .SortDefault()
            .ToList();

        protected virtual IEnumerable<SettingsPropertyGroupDefinition> GetUnsortedSettingPropertyGroups() =>
            SettingsUtils.GetSettingsPropertyGroups(SubGroupDelimiter, Discoverer?.GetProperties(this) ?? Array.Empty<ISettingsPropertyDefinition>());
    }
}
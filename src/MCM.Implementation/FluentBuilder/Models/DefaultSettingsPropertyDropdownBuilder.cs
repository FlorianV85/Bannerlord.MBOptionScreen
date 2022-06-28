﻿using MCM.Abstractions;
using MCM.Abstractions.FluentBuilder.Models;
using MCM.Abstractions.Wrapper;
using MCM.Common;

using System.Collections.Generic;

namespace MCM.Implementation.FluentBuilder.Models
{
    internal sealed class DefaultSettingsPropertyDropdownBuilder :
        BaseDefaultSettingsPropertyBuilder<ISettingsPropertyDropdownBuilder>,
        ISettingsPropertyDropdownBuilder,
        IPropertyDefinitionDropdown
    {
        /// <inheritdoc/>
        public int SelectedIndex { get; }

        internal DefaultSettingsPropertyDropdownBuilder(string id, string name, int selectedIndex, IRef @ref)
            : base(id, name, @ref)
        {
            SettingsPropertyBuilder = this;
            SelectedIndex = selectedIndex;
        }

        /// <inheritdoc/>
        public override IEnumerable<IPropertyDefinitionBase> GetDefinitions() => new IPropertyDefinitionBase[]
        {
            new PropertyDefinitionDropdownWrapper(this),
            new PropertyDefinitionWithIdWrapper(this),
        };
    }
}
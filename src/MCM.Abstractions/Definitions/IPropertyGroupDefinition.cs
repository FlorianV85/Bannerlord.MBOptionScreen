﻿namespace MCM.Abstractions
{
    public interface IPropertyGroupDefinition
    {
        /// <summary>
        /// The name of the settings group. Includes SubGroup notation if present.
        /// </summary>
        string GroupName { get; }

        int GroupOrder { get; }
    }
}
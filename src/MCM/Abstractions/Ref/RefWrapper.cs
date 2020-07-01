﻿using HarmonyLib;

using System;
using System.ComponentModel;
using System.Reflection;

namespace MCM.Abstractions.Ref
{
    /// <summary>
    /// Wrapper around any type that implements <see cref="IRef"/>.
    /// We don't use casting because it might not be safe.
    /// </summary>
    public class RefWrapper : IRef, IWrapper
    {
        /// <inheritdoc/>
        public object Object { get; }
        private PropertyInfo? TypeProperty { get; }
        private PropertyInfo? ValueProperty { get; }
        /// <inheritdoc/>
        public bool IsCorrect { get; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { if (Object is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged += value; }
            remove { if (Object is INotifyPropertyChanged notifyPropertyChanged) notifyPropertyChanged.PropertyChanged -= value; }
        }
        /// <inheritdoc/>
        public Type Type => (Type) TypeProperty!.GetValue(Object);
        /// <inheritdoc/>
        public object Value { get => ValueProperty!.GetValue(Object); set => ValueProperty?.SetValue(Object, value); }

        public RefWrapper(object @object)
        {
            Object = @object;
            var type = @object.GetType();

            TypeProperty = AccessTools.Property(type, nameof(Type));
            ValueProperty = AccessTools.Property(type, nameof(Value));

            IsCorrect = TypeProperty != null && ValueProperty != null;
        }
    }
}
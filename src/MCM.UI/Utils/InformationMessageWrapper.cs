﻿namespace MCM.UI.Utils
{
    public sealed record InformationMessageWrapper(object Object)
    {
        public static InformationMessageWrapper Create(object @object) => new(@object);
    }
}
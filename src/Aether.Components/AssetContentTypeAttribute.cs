﻿using System;

namespace tainicom.Aether.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetContentTypeAttribute: Attribute
    {
        public String AssetContentTypeFullName { get; private set; }

        public AssetContentTypeAttribute(Type assetContentType)
        {
            AssetContentTypeFullName = assetContentType.FullName;
        }

        public AssetContentTypeAttribute(String assetContentTypeFullName)
        {
            AssetContentTypeFullName = assetContentTypeFullName;
        }

    }
}

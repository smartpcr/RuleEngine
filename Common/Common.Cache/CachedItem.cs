// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachedItem.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;

    public class CachedItem<T> where T : class, new()
    {
        public CachedItem()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        public CachedItem(T value) : this()
        {
            Value = value;
        }

        public DateTimeOffset CreatedOn { get; set; }
        public T Value { get; set; }
    }
}
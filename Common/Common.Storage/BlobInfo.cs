// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Microsoft Corporation">
// //   Copyright (c) 2017 Microsoft Corporation.  All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    using System;

    public class BlobInfo
    {
        public string Name { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public bool IsLeased { get; set; }
        public TimeSpan TimeToLive { get; set; }
        public long Size { get; set; }
    }
}
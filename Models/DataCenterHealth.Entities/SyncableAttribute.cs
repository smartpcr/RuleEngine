// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncable.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities
{
    using System;
    using System.Linq;
    using Common.Auth;
    using Common.DocDb;
    using Common.Kusto;
    using Common.Storage;
    using DataCenterHealth.Models.Sync;

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SyncableAttribute : Attribute
    {
        public DataStorageType SourceType { get; set; }

        protected SyncableAttribute(DataStorageType sourceType)
        {
            SourceType = sourceType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CosmosReaderAttribute : SyncableAttribute
    {
        public CosmosReaderSettings ReaderSettings { get; set; }

        public CosmosReaderAttribute(
            string account,
            string db,
            string collection,
            string authKeySecret,
            string countByField)
            : base(DataStorageType.CosmosTable)
        {
            ReaderSettings = new CosmosReaderSettings
            {
                DocDb = new DocDbSettings()
                {
                    Account = account,
                    Db = db,
                    Collection = collection,
                    AuthKeySecret = authKeySecret
                },
                CountBy = countByField
            };
        }
    }

    public class BlobReaderAttribute : SyncableAttribute
    {
        public BlobReaderSettings ReaderSettings { get; set; }
        public Type ParserType { get; set; }

        public BlobReaderAttribute(
            Type parserType,
            string account,
            string container,
            string connStrSecret)
            : base(DataStorageType.BlobStorage)
        {
            ParserType = parserType;
            ReaderSettings=new BlobReaderSettings()
            {
                Blob = new BlobStorageSettings()
                {
                    Account = account,
                    Container = container,
                    ConnectionStringSecretName = connStrSecret
                }
            };
        }
    }

    public class KustoReaderAttribute : SyncableAttribute
    {
        public KustoReaderSettings ReaderSettings { get; set; }

        public KustoReaderAttribute(
            string cluster,
            string db,
            string table,
            string[] orderByFields)
            : base(DataStorageType.Kusto)
        {
            ReaderSettings=new KustoReaderSettings()
            {
                Kusto = new KustoSettings()
                {
                    ClusterName = cluster,
                    DbName = db,
                    AuthMode = AuthMode.SPN,
                    RegionName = null,
                },
                Query = table,
                OrderByFields = orderByFields.ToList(),
                ThrottlingSize = 500000
            };
        }
    }

    public class CosmosWriterAttribute : Attribute
    {
        public CosmosWriterSettings WriterSettings { get; set; }

        public CosmosWriterAttribute(
            string account,
            string db,
            string collection,
            string authKeySecret,
            string countByField,
            string uniqueField)
        {
            WriterSettings=new CosmosWriterSettings()
            {
                DocDb = new DocDbSettings()
                {
                    Account = account,
                    Db = db,
                    Collection = collection,
                    AuthKeySecret = authKeySecret
                },
                CountBy = countByField,
                UniqueField = uniqueField
            };
        }
    }
}

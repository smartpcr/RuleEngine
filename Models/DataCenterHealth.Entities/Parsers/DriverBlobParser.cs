// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DriverBlobParser.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Common.Storage;
    using Common.Telemetry;
    using DataCenterHealth.Entities.DataType;
    using Devices;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DriverBlobParser : IBlobParser, IBlobParserFactory
    {
        private readonly ILogger<DriverBlobParser> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly IBlobClient client;
        private readonly string localFolder;

        public DriverBlobParser()
        {
        }

        public DriverBlobParser(IBlobClient client, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.client = client;
            logger = loggerFactory.CreateLogger<DriverBlobParser>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();

            var assemblyFileName = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrEmpty(assemblyFileName))
            {
                throw new InvalidOperationException("invalid assembly file location");
            }
            var binFolder = Path.GetDirectoryName(assemblyFileName);
            if (string.IsNullOrEmpty(binFolder) || !Directory.Exists(binFolder))
            {
                throw new InvalidOperationException("invalid bin folder");
            }
            localFolder = Path.Combine(binFolder, "driver_config");
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }
        }

        public Type InputType => typeof(ZenonDriverConfigRoot);
        public Type OutputType => typeof(ZenonDriverConfig);

        public Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancel)
        {
            var containers = new List<string>() {client.CurrentContainerName};
            return Task.FromResult(containers.AsEnumerable());
        }

        public async Task<IEnumerable<string>> ListBlobNamesAsync(string containerName, DateTime? timeFilter, CancellationToken cancel)
        {
            var containerClient = containerName == client.CurrentContainerName
                ? client
                : client.SwitchContainer(containerName);
            return await containerClient.ListBlobNamesAsync(timeFilter, cancel);
        }

        public async Task<IEnumerable<object>> ParseBlobAsync(string containerName, string blobName, CancellationToken cancel)
        {
            var output = new List<ZenonDriverConfig>();
            try
            {
                var containerClient = containerName == client.CurrentContainerName
                    ? client
                    : client.SwitchContainer(containerName);

                logger.LogInformation($"evaluating blob {blobName}...");
                var blobFile = await containerClient.DownloadAsync(null, blobName, localFolder, cancel);
                FileStream fs = new FileStream(blobFile, FileMode.Open);

                var serializer = new XmlSerializer(typeof(ZenonDriverConfigRoot));
                serializer.UnknownNode += (s, e) => OnUnknownNode(blobName, s, e);
                serializer.UnknownAttribute += (s, e) => OnUnknownAttribute(blobName, s, e);
                var root = (ZenonDriverConfigRoot) serializer.Deserialize(fs);
                if (root.DriverType?.Name == "MODBUS_ENERGY")
                {
                    var configFileName = blobName;
                    if (configFileName.EndsWith(".xml"))
                    {
                        configFileName = configFileName.Substring(0, configFileName.Length - ".xml".Length);
                    }
                    if (configFileName.EndsWith("_config"))
                    {
                        configFileName = configFileName.Substring(0, configFileName.Length - "_config".Length);
                    }

                    var config = new ZenonDriverConfig()
                    {
                        ConfigFileName = configFileName,
                        PriorityTimesNormal = root.DriverType.General.PriorityTimesNormal,
                        PriorityTimesHigh = root.DriverType.General.PriorityTimesHigh,
                        PriorityTimesHigher = root.DriverType.General.PriorityTimesHigher,
                        PriorityTimesHighest = root.DriverType.General.PriorityTimesHighest,
                        CommunicationTimeout = root.DriverType.Settings.CommunicationTimeout,
                        CommunicationRetries = root.DriverType.Settings.CommunicationRetries,
                        ReConnectTimeout = root.DriverType.Settings.ReConnectTimeout
                    };
                    output.Add(config);
                }

                return output;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to parse blob {blobName}, error: {ex.Message}");
                return new List<object>();
            }
        }

        public string TypeName => GetType().Name;

        public IBlobParser Create(IBlobClient blobClient, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            return new DriverBlobParser(blobClient, serviceProvider, loggerFactory);
        }

        private void OnUnknownAttribute(string blobName, object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            logger.LogWarning($"Unknown attribute: {attr.Name} = {attr.Value}");
            appTelemetry.RecordMetric(
                $"{GetType().Name}-unknownAttribute",
                1,
                ("blobName", blobName),
                ("attrName", attr.Name));
        }

        private void OnUnknownNode(string blobName, object sender, XmlNodeEventArgs e)
        {
            logger.LogWarning($"Unknown Node: {e.Name} \t {e.Text}");
            appTelemetry.RecordMetric(
                $"{GetType().Name}-unknownNode",
                1,
                ("blobName", blobName),
                ("nodeName", e.Name));
        }
    }
}
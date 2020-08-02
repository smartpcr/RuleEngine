// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigBlobParser.cs" company="Microsoft Corporation">
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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DataPointConfigBlobParser : IBlobParser, IBlobParserFactory
    {
        private readonly Microsoft.Extensions.Logging.ILogger<DataPointConfigBlobParser> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly IBlobClient client;
        private readonly string localFolder;

        public DataPointConfigBlobParser()
        {
        }

        public DataPointConfigBlobParser(IBlobClient client, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.client = client;
            logger = loggerFactory.CreateLogger<DataPointConfigBlobParser>();
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
            localFolder = Path.Combine(binFolder, "datapoint_config");
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

        public Task<IEnumerable<string>> ListBlobNamesAsync(string containerName, DateTime? timeFilter, CancellationToken cancel)
        {
            var blobNames = new List<string>() {"Configuration.xml"};
            return Task.FromResult(blobNames.AsEnumerable());
        }

        public async Task<IEnumerable<object>> ParseBlobAsync(string containerName, string blobName, CancellationToken cancel)
        {
            var output = new List<ZenonDataPointConfig>();
            try
            {
                var containerClient = containerName == client.CurrentContainerName
                    ? client
                    : client.SwitchContainer(containerName);

                logger.LogInformation($"evaluating blob {blobName}...");
                var blobFile = await containerClient.DownloadAsync(null, blobName, localFolder, cancel);
                FileStream fs = new FileStream(blobFile, FileMode.Open);

                var serializer = new XmlSerializer(typeof(ReactionMatrix));
                serializer.UnknownNode += (s, e) => OnUnknownNode(blobName, s, e);
                serializer.UnknownAttribute += (s, e) => OnUnknownAttribute(blobName, s, e);
                var root = (ReactionMatrix) serializer.Deserialize(fs);
                var approvedDataPoints = root.ApprovedDataPoints?.DataPoints;
                if (approvedDataPoints != null && approvedDataPoints.Length > 0)
                {
                    foreach (var approvedDataPoint in approvedDataPoints)
                    {
                        var attributes =
                            approvedDataPoint.Attributes?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                        if (attributes != null && attributes.Length > 0)
                        {
                            foreach (var datapoint in attributes)
                            {
                                var dpStr = datapoint.Trim(new[] {'.'}).Trim();
                                var pair = dpStr.Split(new[] {'.'}, 2, StringSplitOptions.RemoveEmptyEntries);
                                if (pair.Length == 2)
                                {
                                    var channelType = pair[0];
                                    var channel = pair[1];
                                    var config = new ZenonDataPointConfig()
                                    {
                                        Type = approvedDataPoint.Type,
                                        DataPoint = dpStr,
                                        ChannelType = channelType,
                                        Channel = channel,
                                        Version = approvedDataPoint.Version
                                    };
                                    output.Add(config);
                                }
                                else
                                {
                                    logger.LogWarning($"Invalid datapoint: {datapoint} in type: {approvedDataPoint.Type}");
                                }
                            }
                        }
                    }
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
            return new DataPointConfigBlobParser(blobClient, serviceProvider, loggerFactory);
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
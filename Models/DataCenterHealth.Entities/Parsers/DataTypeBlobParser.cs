namespace DataCenterHealth.Entities.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Common.Storage;
    using Devices;
    using Microsoft.Extensions.Logging;

    public class DataTypeBlobParser : IBlobParser, IBlobParserFactory
    {
        private readonly ILogger<DataTypeBlobParser> logger;
        private readonly IBlobClient client;

        private readonly string localFolder;

        public DataTypeBlobParser()
        {
        }

        public DataTypeBlobParser(IBlobClient client, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.client = client;
            logger = loggerFactory.CreateLogger<DataTypeBlobParser>();
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
            localFolder = Path.Combine(binFolder, "data_type");
            if (!Directory.Exists(localFolder))
            {
                Directory.CreateDirectory(localFolder);
            }
        }

        public Type InputType => typeof(Subject);
        public Type OutputType => typeof(ZenonDataType);

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
            var output = new List<ZenonDataType>();
            try
            {
                var containerClient = containerName == client.CurrentContainerName
                    ? client
                    : client.SwitchContainer(containerName);

                logger.LogInformation($"evaluating blob {blobName}...");
                var blobFile = await containerClient.DownloadAsync(null, blobName, localFolder, cancel);

                XmlDocument xDoc = new XmlDocument();
                string content = await File.ReadAllTextAsync(blobFile, Encoding.Unicode, cancel);
                xDoc.LoadXml(content);

                XmlNode dtNode = xDoc.SelectSingleNode("//Type/Name");
                string dtName = dtNode.InnerText;
                XmlNode ctNode = dtNode.ParentNode;
                foreach (XmlNode child in ctNode.ChildNodes)
                {
                    XmlNode cTypeNode = child.SelectSingleNode(".//TypeID");
                    if (cTypeNode != null)
                    {
                        string cTypeId = cTypeNode.InnerText;
                        string cTypeName = cTypeNode.ParentNode.SelectSingleNode(".//Name").InnerText;

                        // Get Channels
                        XmlNode cNode = xDoc.SelectSingleNode("//Type[@TypeID='" + cTypeId + "']");
                        foreach (XmlNode cNodeChild in cNode.ChildNodes)
                        {
                            if (cNodeChild.Name.StartsWith("Items_"))
                            {
                                string cName = cNodeChild.SelectSingleNode(".//Name").InnerText;
                                string cOffset = cNodeChild.SelectSingleNode(".//Offset").InnerText;
                                string cDataTypeId = cNodeChild.SelectSingleNode(".//ID_DataTyp").InnerText;

                                XmlNode priNode = xDoc.SelectSingleNode("//Type[@TypeID='" + cDataTypeId + "']");

                                string priDataType = priNode.SelectSingleNode(".//Name").InnerText;
                                var priority = priNode.SelectSingleNode(".//UpdatePriority")?.InnerText;
                                string digits = priNode.SelectSingleNode(".//Digits")?.InnerText;
                                string dp = $"{cTypeName}.{cName}";

                                output.Add(new ZenonDataType()
                                {
                                    DataTypeFileName = dtName,
                                    ChannelTypeName = cTypeName,
                                    ChannelTypeId = int.Parse(cTypeId),
                                    ChannelName = cName,
                                    ChannelId = int.Parse(cTypeId),
                                    Offset = int.Parse(cOffset),
                                    Primitive = priDataType,
                                    UpdatePriority = priority == null ? default : (byte)int.Parse(priority),
                                    Digits = digits == null ? default : (byte)int.Parse(digits),
                                    DataPoint = dp,
                                    FileDataPoint = $"{dtName}.{dp}",
                                });
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
            return new DataTypeBlobParser(blobClient, serviceProvider, loggerFactory);
        }
    }
}
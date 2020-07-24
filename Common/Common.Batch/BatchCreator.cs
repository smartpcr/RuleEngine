// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Batcher.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Storage;
using Common.Config;
using Common.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Common.Batch
{
    public class BatchCreator<TField> : IBatchCreator<TField> where TField : IComparable
    {
        private readonly BatchSetting<TField> _batchSetting;
        private readonly ILogger<BatchCreator<TField>> _logger;
        private readonly IAppTelemetry _appTelemetry;
        private readonly IBlobClient _blobClient;
        private readonly IRangeHelper<TField> _rangeHelper;
        private readonly Func<Batch<TField>, string> _getBlobName;

        public BatchCreator(IConfiguration configuration, ILoggerFactory loggerFactory, IAppTelemetry appTelemetry,
            IBlobClient blobClient, IRangeHelper<TField> rangeHelper)
        {
            _batchSetting = configuration.GetConfiguredSettings<BatchSetting<TField>>();
            _logger = loggerFactory.CreateLogger<BatchCreator<TField>>();
            _appTelemetry = appTelemetry;
            _blobClient = blobClient;
            _rangeHelper = rangeHelper;
            _getBlobName = t => $"{_batchSetting.Name}-{t.Sequence}";
        }

        public async Task GenerateBatchesIfNotExist()
        {
            _logger.LogInformation(
                $"Generating batches... total: {_batchSetting.Total}, batch size: {_batchSetting.BatchSize}");
            var totalBatches = (int) (_batchSetting.Total / _batchSetting.BatchSize);
            var spans = _rangeHelper.Split(_batchSetting.Start, _batchSetting.End, totalBatches);
            var batchesFound = await _blobClient.CountAsync<Batch<TField>>(_batchSetting.Name, t => true, CancellationToken.None);
            if (batchesFound == spans.Count)
            {
                _logger.LogInformation($"batches already exist, count: {batchesFound}");
                return;
            }

            await _blobClient.DeleteBlobs(_batchSetting.Name, CancellationToken.None);
            int seq = 0;
            var batches = spans.Select(s => new Batch<TField>(s.start, s.end, ++seq)).ToList();
            await _blobClient.UploadBatchAsync(_batchSetting.Name, _getBlobName, batches, new CancellationToken());
            _logger.LogInformation($"total of {totalBatches} batches are generated");
        }

        public async Task<int> GetTotalProcessed()
        {
            _logger.LogInformation($"counting batches already processed...");
            var count = await _blobClient.CountAsync<Batch<TField>>(_batchSetting.Name, t => t.FinishTime != null,
                new CancellationToken());
            _logger.LogInformation($"total of {count} batches are processed");
            return count;
        }

        public async Task<int> GetTotalInProgress()
        {
            _logger.LogInformation($"counting batches in progress...");
            var count = await _blobClient.CountAsync<Batch<TField>>(_batchSetting.Name,
                t => t.FinishTime == null && t.StartTime.HasValue, new CancellationToken());
            _logger.LogInformation($"total of {count} batches in progress");
            return count;
        }

        public async Task<int> GetTotalInQueue()
        {
            _logger.LogInformation($"counting batches in queue...");
            var count = await _blobClient.CountAsync<Batch<TField>>(_batchSetting.Name, t => t.StartTime == null,
                new CancellationToken());
            _logger.LogInformation($"total of {count} batches in queue");
            return count;
        }

        public async Task<IEnumerable<Batch<TField>>> Pickup(string consumer, int count = 1)
        {
            _logger.LogInformation($"picking up batch from payload...");
            var items = await _blobClient.TryAcquireLease<Batch<TField>>(_batchSetting.Name, count,
                t => t.Consumer = consumer, _batchSetting.LeaseTimeout);
            _logger.LogInformation($"lease acquired? {items != null}, total: {items?.Count}");
            items = items?.Select(t =>
            {
                t.StartTime ??= DateTime.UtcNow;
                return t;
            }).ToList();
            return items;
        }

        public async Task Fail(Batch<TField> batch)
        {
            batch.FinishTime ??= DateTime.UtcNow;
            batch.Failed = true;
            _logger.LogInformation(
                $"batch failed, seq: {batch.Sequence}, consumer: {batch.Consumer}, error: {batch.Error}...");
            await _blobClient.UploadAsync(_batchSetting.Name, _getBlobName(batch), JsonConvert.SerializeObject(batch),
                new CancellationToken());
        }

        public async Task Succeed(Batch<TField> batch)
        {
            batch.FinishTime ??= DateTime.UtcNow;
            batch.Failed = false;
            _logger.LogInformation(
                $"batch succeed, seq: {batch.Sequence}, consumer: {batch.Consumer}, span: {batch.FinishTime - batch.StartTime}...");
            await _blobClient.UploadAsync(_batchSetting.Name, _getBlobName(batch), JsonConvert.SerializeObject(batch),
                new CancellationToken());
        }
    }
}
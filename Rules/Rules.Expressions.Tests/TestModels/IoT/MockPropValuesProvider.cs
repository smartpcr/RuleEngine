// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockPropValuesProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Builders;
    using Models.IoT;

    public class MockPropValuesProvider : IPropertyValuesProvider
    {
        public async Task<IEnumerable<string>> GetAllowedValues(Type owner, PropertyInfo prop)
        {
            if (owner == typeof(Device))
            {
                switch (prop?.Name)
                {
                    case "DeviceType":
                        return GetDeviceTypes();
                    case "DeviceState":
                        return GetDeviceStates();
                    case "Hierarchy":
                        return GetDeviceHierarchies();
                }
            }

            if (owner == typeof(ReadingStats))
            {
                switch (prop?.Name)
                {
                    case "ChannelType":
                        return await GetAllowedChannelTypes();
                    case "Channel":
                        return await GetAllowedChannels();
                    case "DataPoint":
                        return await GetAllowedDataPoints();
                }
            }

            if (owner == typeof(LastReading))
            {
                switch (prop?.Name)
                {
                    case "ChannelType":
                        return await GetAllowedChannelTypes();
                    case "Channel":
                        return await GetAllowedChannels();
                    case "DataPoint":
                        return await GetAllowedDataPoints();
                }
            }

            if (owner == typeof(DataPoint))
            {
                switch (prop?.Name)
                {
                    case "ChannelType":
                        return await GetAllowedChannelTypes();
                    case "Channel":
                        return await GetAllowedChannels();
                    case "DataPoint":
                        return await GetAllowedDataPoints();
                }
            }

            return new List<string>();
        }
        
         #region allowed values
        private List<string> GetDeviceTypes()
        {
            return new List<string>()
            {
                "AHU",
                "ATS",
                "BMS",
                "Breaker",
                "Busbar",
                "Busway",
                "Condenser",
                "DOAS",
                "DistributionPanel",
                "End",
                "Feed",
                "Filter",
                "FuelFill",
                "FuelPolisher",
                "GenBreaker",
                "Generator",
                "HRG",
                "Heat",
                "Heater",
                "Humidifier",
                "LoadBank",
                "PDU",
                "Panel",
                "ParallelPanel",
                "PowerMeter",
                "Pump",
                "RPP",
                "STS",
                "SurgeProtectiveDevice",
                "Switch",
                "TieBreaker",
                "Transformer",
                "UPS",
                "VFD",
                "Zenon"
            };
        }

        private List<string> GetDeviceStates()
        {
            return new List<string>()
            {
                "NormallyClosed",
                "NotApplicable",
                "NormallyOpen",
                "Spare",
                "StandBy",
                "Future",
                "Missing",
                "Source1",
                "Source2"
            };
        }

        private List<string> GetDeviceHierarchies()
        {
            return new List<string>()
            {
                "ATS",
                "BUSBAR",
                "GEN",
                "LVS-Colo",
                "LVS-SubStation",
                "MSB",
                "MVS-SubStation",
                "MechanicalDistribution",
                "Misc-Others",
                "PDU",
                "PDUInput",
                "PDUOutput",
                "PDUR",
                "PDURInput",
                "PDUROutput",
                "RPP",
                "STS",
                "UDS",
                "UPS",
                "UTS-Campus",
                "UTS-Facility",
                "Unknown"
            };
        }

        private Task<List<string>> GetAllowedDataPoints()
        {
            var allowedDataPoints = new List<string>()
            {
                "Amps.1a",
                "Amps.1b",
                "Amps.1c",
                "Volb.Vab",
                "Volb.Vbc",
                "Volb.Vca",
                "Energe.kWh",
                "Pwr.kW tot",
                "Freq.Freq",
                "Argus.Unmonitored"
            };
            return Task.FromResult(allowedDataPoints);
            // var datapoints = await cache.GetOrUpdateAsync(
            //     $"list-{nameof(ZenonDataPointConfig)}",
            //     async () => await dataPointConfigRepo.GetLastModificationTime(null, default),
            //     async () =>
            //     {
            //         var dpConfigs = await dataPointConfigRepo.GetAll();
            //         return dpConfigs.ToList();
            //     }, default);
            // logger.LogInformation($"total of {datapoints.Count} data points retrieved");
            // return datapoints.Select(dp => dp.DataPoint).ToList();
        }

        private Task<List<string>> GetAllowedChannelTypes()
        {
            var allowedChannelTypes = new List<string>()
            {
                "Amps",
                "Volt",
                "Energy",
                "Pwr",
                "Freq",
                "Argus"
            };
            return Task.FromResult(allowedChannelTypes);

            // var datapoints = await cache.GetOrUpdateAsync(
            //     $"list-{nameof(ZenonDataPointConfig)}",
            //     async () => await dataPointConfigRepo.GetLastModificationTime(null, default),
            //     async () =>
            //     {
            //         var dpConfigs = await dataPointConfigRepo.GetAll();
            //         return dpConfigs.ToList();
            //     }, default);
            // logger.LogInformation($"total of {datapoints.Count} data points retrieved");
            // return datapoints.Select(dp => dp.ChannelType).Distinct().ToList();
        }

        private Task<List<string>> GetAllowedChannels()
        {
            var allowedChannels = new List<string>()
            {
                "kWh",
                "kW tot",
                "Freq",
                "Unmonitored"
            };
            return Task.FromResult(allowedChannels);

            // var datapoints = await cache.GetOrUpdateAsync(
            //     $"list-{nameof(ZenonDataPointConfig)}",
            //     async () => await dataPointConfigRepo.GetLastModificationTime(null, default),
            //     async () =>
            //     {
            //         var dpConfigs = await dataPointConfigRepo.GetAll();
            //         return dpConfigs.ToList();
            //     }, default);
            // logger.LogInformation($"total of {datapoints.Count} data points retrieved");
            // return datapoints.Select(dp => dp.Channel).Distinct().ToList();
        }
        #endregion
    }
}
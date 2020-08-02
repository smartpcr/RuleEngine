// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Microsoft Corporation">
// //   Copyright (c) 2017 Microsoft Corporation.  All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common.Config;
    using DataCenterHealth.Models.Devices;

    public class MockPropValuesProvider : IPropertyValuesProvider
    {
        public Task<IEnumerable<string>> GetAllowedValues(Type owner, PropertyInfo prop)
        {
            var allowedValues = new List<string>();
            if (owner == typeof(PowerDevice))
            {
                switch (prop.Name)
                {
                    case "DeviceType":
                        allowedValues = GetDeviceTypes();
                        break;
                    case "Hierarchy":
                        allowedValues =  GetDeviceHierarchies();
                        break;
                    case "DeviceState":
                        allowedValues =  GetDeviceStates();
                        break;
                }
            }

            if (owner == typeof(ZenonEventStats))
            {
                switch (prop?.Name)
                {
                    case "ChannelType":
                        allowedValues = GetAllowedChannelTypes();
                        break;
                    case "Channel":
                        allowedValues = GetAllowedChannels();
                        break;
                    case "DataPoint":
                        allowedValues = GetAllowedDataPoints();
                        break;
                }
            }

            if (owner == typeof(ZenonLastReading))
            {
                switch (prop?.Name)
                {
                    case "DataPoint":
                        allowedValues = GetAllowedDataPoints();
                        break;
                }
            }

            return Task.FromResult(allowedValues.AsEnumerable());
        }

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

        private List<string> GetAllowedDataPoints()
        {
            return new List<string>()
            {
                "Amps.1a",
                "Amps.1b",
                "Amps.1c",
                "Volb.Vab",
                "Volb.Vbc",
                "Volb.Vca",
                "Energe.kWh",
                "Pwr.kW tot",
                "Freq.Freq"
            };
        }

        private List<string> GetAllowedChannelTypes()
        {
            var allowedChannelTypes = new List<string>()
            {
                "Amps",
                "Volt",
                "Energy",
                "Pwr",
                "Freq"
            };
            return allowedChannelTypes;
        }

        private List<string> GetAllowedChannels()
        {
            var allowedChannels = new List<string>()
            {
                "kWh",
                "kW tot",
                "Freq"
            };
            return allowedChannels;
        }
    }
}
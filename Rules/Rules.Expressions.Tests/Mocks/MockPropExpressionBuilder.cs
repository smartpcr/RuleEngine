// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockPropExpressionBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common.Config;
    using DataCenterHealth.Models.Devices;

    public class MockPropExpressionBuilder : IPropertyExpression
    {
        public List<Type> SupportedTypes => new List<Type>()
        {
            typeof(PowerDevice),
            typeof(DataPoint),
            typeof(ZenonEventStats),
            typeof(ZenonLastReading),
            typeof(DeviceAssociation)
        };

        public List<MethodInfo> GetMacroMethods(Type owner)
        {
            return owner.GetExtensionMethods().ToList();
        }

        public bool CanQuery(Type owner, PropertyInfo prop)
        {
            if (owner == typeof(PowerDevice))
            {
                switch (prop.Name)
                {
                    case "DeviceName":
                    case "DeviceState":
                    case "DeviceType":
                    case "Hierarchy":
                    case "OnboardingMode":
                    case "DcName":
                    case "ColoName":
                    case "IsMonitorable":
                    case "PrimaryParentDevice":
                    case "SecondaryParentDevice":
                    case "MaintenanceParentDevice":
                    case "RedundantDevice":
                    case "AllParents":
                    case "RootDevice":
                    case "Children":
                    case "ReadingStats":
                    case "DataType":
                    case "LastReadings":
                    case "SiblingDevices":
                    case "DriverName":
                    case "ConnectionName":
                    case "DataPoints":
                    case "DirectUpstreamDeviceList":
                    case "DirectDownstreamDeviceList":
                    case "Amperage":
                    case "Voltage":
                    case "RatedCapacity":
                    case "DeratedCapacity":
                    case "MaxItCapacity":
                    case "ItCapacity":
                    case "AmpRating":
                    case "VoltageRating":
                    case "KwRating":
                    case "KvaRating":
                    case "PowerFactor":
                    case "DeRatingFactor":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DataPoint))
            {
                switch (prop.Name)
                {
                    case "ChannelType":
                    case "Channel":
                    case "PollInterval":
                    case "FilterdOutInPG":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DeviceAssociation))
            {
                return prop.Name == "AssociationType";
            }

            if (owner == typeof(ZenonEventStats))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "Count":
                    case "Avg":
                    case "Max":
                    case "Min":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(ZenonLastReading))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "EventTime":
                    case "Value":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        public bool CanSelect(Type owner, PropertyInfo prop)
        {
            if (owner == typeof(PowerDevice))
            {
                switch (prop.Name)
                {
                    case "DeviceType":
                    case "DeviceName":
                    case "DeviceState":
                    case "Hierarchy":
                    case "OnboardingMode":
                    case "DcName":
                    case "ColoName":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DataPoint))
            {
                switch (prop.Name)
                {
                    case "ChannelType":
                    case "Channel":
                    case "PollInterval":
                    case "FilterdOutInPG":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DeviceAssociation))
            {
                return prop.Name == "AssociationType";
            }

            if (owner == typeof(ZenonEventStats))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "Count":
                    case "Avg":
                    case "Max":
                    case "Min":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(ZenonLastReading))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "EventTime":
                    case "Value":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        public bool CanCompare(Type owner, PropertyInfo prop)
        {
            if (owner == typeof(PowerDevice))
            {
                switch (prop.Name)
                {
                    case "DeviceName":
                    case "DeviceState":
                    case "Hierarchy":
                    case "OnboardingMode":
                    case "DcName":
                    case "ColoName":
                    case "Amperage":
                    case "Voltage":
                    case "IsMonitorable":
                    case "DataType":
                    case "SiblingDevices":
                    case "DriverName":
                    case "ConnectionName":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DataPoint))
            {
                switch (prop.Name)
                {
                    case "ChannelType":
                    case "Channel":
                    case "PollInterval":
                    case "FilterdOutInPG":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(DeviceAssociation))
            {
                return prop.Name == "AssociationType";
            }

            if (owner == typeof(ZenonEventStats))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "Count":
                    case "Avg":
                    case "Max":
                    case "Min":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(ZenonLastReading))
            {
                switch (prop.Name)
                {
                    case "DataPoint":
                    case "EventTime":
                    case "Value":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        public bool CanSort(Type owner, PropertyInfo prop)
        {
            if (owner == typeof(ZenonEventStats))
            {
                switch (prop.Name)
                {
                    case "Count":
                    case "Avg":
                    case "Max":
                    case "Min":
                        return true;
                    default:
                        return false;
                }
            }

            if (owner == typeof(ZenonLastReading))
            {
                switch (prop.Name)
                {
                    case "EventTime":
                    case "Value":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}
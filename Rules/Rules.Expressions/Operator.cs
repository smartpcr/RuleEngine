// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Operator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    public enum Operator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Contains,
        NotContains,
        ContainsAll,
        NotContainsAll,
        StartsWith,
        NotStartsWith,
        In,
        NotIn,
        AllIn,
        NotAllIn,
        AnyIn,
        NotAnyIn,
        IsNull,
        NotIsNull,
        IsEmpty,
        NotIsEmpty,
        DiffWithinPct,
        AllInRangePct,
        ChannelNameEquals,
        ChannelNameNotEquals,
        ChannelNameContains,
        ChannelNameNotContains,
        ChannelNameStartsWith,
        ChannelNameNotStartsWith,
        QualityEquals,
        QualityNotEquals,
        DataPointValueGreaterThanRatingPct,
        IsStale,
        CheckStaleness,
        StaledAmpsChannels,
        StaledS1AmpsChannels,
        AllAmpsChannelsAreStale,
        AllS1AmpsChannelsAreStale,
        StaledVoltChannels,
        StaledS1VoltChannels,
        AllVoltChannelsAreStale,
        AllS1VoltChannelsAreStale,
        MaxChannelVoltGreaterThanRating,
        MaxS1ChannelVoltGreaterThanRating
    }
}
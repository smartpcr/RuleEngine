// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistableMessage.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    public class PersistableMessage
    {
        public bool Persist { get; }

        public PersistableMessage() : this(true) { }

        public PersistableMessage(bool persist)
        {
            Persist = persist;
        }
    }
}
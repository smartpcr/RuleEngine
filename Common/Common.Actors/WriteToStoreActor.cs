// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WriteToStoreActor.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    using Akka.Actor;

    public class WriteToStoreActor : ReceiveActor
    {
        private readonly string id;

        public WriteToStoreActor(string id)
        {
            this.id = id;
            Receive<WriteMessagesToStore>(HandleWriteMessagesToStore);
        }

        private void HandleWriteMessagesToStore(WriteMessagesToStore message)
        {
            // TODO: write to azure blob
        }

        public static Props CreateProps(string id)
        {
            return Props.Create<WriteToStoreActor>(id);
        }
    }
}
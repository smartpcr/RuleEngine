// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistenceActor.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Akka.Actor;
    using Akka.Persistence;

    public class PersistenceActor : ReceivePersistentActor
    {
        private readonly string id;
        private readonly IActorRef blobStoreWriterActor;
        private PersistenceState state = new PersistenceState();
        public override string PersistenceId => $"message-persistence-{id}";

        public PersistenceActor(string id)
        {
            this.id = id;

            Command<PersistableMessage>(HandlePersistableMessageCommand);
            Command<RequestLastPersistedItems>(HandleRequestLastPersistedItemsCommand);
            Command<TakeHourlySnapshotMessage>(_ => HandleTakeHourlySnapshotMessageCommand());
            Command<WrittenToStoreMessage>(HandleWrittenToStoreMessageCommand);

            Recover<PersistableMessage>(HandlePersistableMessage);
            Recover<SnapshotOffer>(HandleSnapshotOffer);
            Recover<WrittenToStoreMessage>(HandleWrittenToStoreMessage);

            blobStoreWriterActor = Context.ActorOf(WriteToStoreActor.CreateProps(id), $"persistence-{id}");
        }

        protected override void PreStart()
        {
            var seconds = new Random().Next(3600);
            var initialDelay = new TimeSpan(0, 0, 0, seconds);
            var interval = new TimeSpan(0, 1, 0, 0);
            Context.System.Scheduler.ScheduleTellRepeatedly(
                initialDelay,
                interval,
                Self,
                new TakeHourlySnapshotMessage(),
                Self);
        }

        private void HandlePersistableMessageCommand(PersistableMessage message)
        {
            Persist(message, HandlePersistableMessage);
        }

        private void HandlePersistableMessage(PersistableMessage message)
        {
            state.Add(message);
        }

        private void HandleRequestLastPersistedItemsCommand(RequestLastPersistedItems message)
        {
            var lastReadings = state.GetLastPersistedItems(message.NumberOfReadings);
            var response = new ReturnLastPersistedMessages(lastReadings);
            Sender.Tell(response);
        }

        private void HandleTakeHourlySnapshotMessageCommand()
        {
            SaveSnapshot(state);
            blobStoreWriterActor.Tell(new WriteMessagesToStore(state.GetUnsavedItems()));
        }

        private void HandleWrittenToStoreMessageCommand(WrittenToStoreMessage message)
        {
            Persist(message, HandleWrittenToStoreMessage);
        }

        private void HandleWrittenToStoreMessage(WrittenToStoreMessage message)
        {
            state.SetSaveUntil(message.WrittenToDate);
            state.Truncate();
        }

        private void HandleSnapshotOffer(SnapshotOffer offer)
        {
            if (offer.Snapshot is PersistenceState persistenceState)
            {
                state = persistenceState;
            }
        }

        public static Props CreateProps(string id)
        {
            return Props.Create<PersistenceActor>(id);
        }
    }

    class PersistenceState
    {
        public List<PersistenceStateItem> Items { get; } = new List<PersistenceStateItem>();

        public void Add(PersistableMessage message)
        {
            Items.Add(new PersistenceStateItem(message));
        }

        public PersistableMessage[] GetUnsavedItems()
        {
            return Items.Where(i => !i.Saved).Select(i => i.Message).ToArray();
        }

        public void SetSaveUntil(DateTime until)
        {
            foreach (var item in Items.Where(i => i.Timestamp <= until))
            {
                item.Saved = true;
            }
        }

        public PersistableMessage[] GetLastPersistedItems(int numberOfReadings)
        {
            var numberOfReturnedReadings = Math.Min(numberOfReadings, Items.Count);

            if (numberOfReturnedReadings == 0)
                return Array.Empty<PersistableMessage>();

            return Items
                .OrderByDescending(r => r.Timestamp)
                .Take(numberOfReturnedReadings)
                .Select(i => i.Message)
                .ToArray();
        }

        public void Truncate()
        {
            if (Items.Any())
            {
                var bottomDate = Items.Last().Timestamp.AddHours(-12);
                Items.RemoveAll(i => i.Timestamp < bottomDate && i.Saved);
            }
        }
    }

    class PersistenceStateItem
    {
        public DateTime Timestamp { get; set; }
        public PersistableMessage Message { get; set; }
        public bool Saved { get; set; }

        public PersistenceStateItem(PersistableMessage message)
        {
            Message = message;
            Timestamp = DateTime.UtcNow;
            Saved = false;
        }
    }

    class TakeHourlySnapshotMessage {}

    class RequestLastPersistedItems
    {
        public int NumberOfReadings { get; }

        public RequestLastPersistedItems(int numberOfReadings)
        {
            NumberOfReadings = numberOfReadings;
        }
    }

    class WrittenToStoreMessage
    {
        public DateTime WrittenToDate { get; }

        public WrittenToStoreMessage(DateTime writtenToDate)
        {
            WrittenToDate = writtenToDate;
        }
    }

    class ReturnLastPersistedMessages
    {
        public ImmutableArray<PersistableMessage> Messages { get; }

        public ReturnLastPersistedMessages(params PersistableMessage[] messages)
        {
            Messages = ImmutableArray.Create(messages);
        }
    }

    class WriteMessagesToStore
    {
        public ImmutableList<PersistableMessage> Messages { get; }

        public WriteMessagesToStore(PersistableMessage[] messages)
        {
            Messages = ImmutableList.Create(messages);
        }
    }
}
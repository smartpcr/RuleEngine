// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorBase.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    using System;
    using Akka.Actor;
    using Akka.Event;

    public class ActorBase : ReceiveActor
    {
        private readonly IActorRef persistenceActor;
        protected string Id { get; }
        protected ILoggingAdapter Logger { get; }

        public ActorBase(string id)
        {
            Id = id;
            Logger = Context.GetLogger();
            persistenceActor = Context.ActorOf(PersistenceActor.CreateProps(id), "message-persistence");
        }

        protected override void PreStart()
        {
            Logger.Log(LogLevel.InfoLevel, $"actor {GetType().Name} started");
            // TODO: heartbeat timer
            // TODO: staleness timer
        }

        protected override void PostStop()
        {
            Logger.Log(LogLevel.InfoLevel, $"actor {GetType().Name} stopped");
        }

        protected void OnReceive<T>(Action<T> handler) where T : PersistableMessage
        {
            Logger.Log(LogLevel.InfoLevel, $"receiving message with type {typeof(T).Name}");
            Action<T> handlerWrapper = msg =>
            {
                Logger.Log(LogLevel.InfoLevel, "receiving message: {@msg}", msg);
                handler(msg);
                if (msg.Persist)
                {
                    persistenceActor.Tell(msg);
                }
            };
            Receive(handlerWrapper);
        }
    }
}
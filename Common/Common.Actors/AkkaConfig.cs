// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaConfig.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// json representation of akka configuration HOCON
    /// </summary>
    public class AkkaConfig
    {
        [JsonProperty(PropertyName = "log-config-on-start")]
        public string logconfigonstart { get; set; }
        [JsonProperty(PropertyName = "stdout-loglevel")]
        public string stdoutloglevel { get; set; }
        public string loglevel { get; set; }
        public string[] loggers { get; set; }
        public ActorConfig actor { get; set; }

        public class ActorConfig
        {
            public DebugConfig debug { get; set; }
            public Dictionary<string, string> serializers { get; set; }
            [JsonProperty(PropertyName = "serialization-bindings")]
            public Dictionary<string, string> serializationbindings { get; set; }

            public class DebugConfig
            {
                public string receive { get; set; }
                public string autoreceive { get; set; }
                public string lifecycle { get; set; }
                [JsonProperty(PropertyName = "event-stream")]
                public string eventstream { get; set; }
                public string unhandled { get; set; }
            }
        }
    }
}
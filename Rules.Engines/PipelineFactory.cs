// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Engines
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class PipelineFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<PipelineFactory> logger;
        private readonly PipelineSettings settings;
        private readonly ActivityAssemblySettings assemblySettings;
        private readonly List<Assembly> assemblies = new List<Assembly>();
        private readonly Dictionary<string, IPipelineActivityFactory> activityCreators = 
            new Dictionary<string, IPipelineActivityFactory>();

        public PipelineFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IOptions<PipelineSettings> settings = null)
        {
            this.serviceProvider = serviceProvider;
            logger = loggerFactory.CreateLogger<PipelineFactory>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            assemblySettings = configuration.GetConfiguredSettings<ActivityAssemblySettings>();
            this.settings = settings?.Value ?? configuration.GetConfiguredSettings<PipelineSettings>();

            LoadActivities();
        }

        public IPipeline Create()
        {
            var activities = new List<IDataflowBlock>();
            var contextType = assemblies.SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName == settings.ContextTypeName);
            if (contextType == null)
            {
                throw new InvalidOperationException($"Unable to find context type: {settings.ContextTypeName}");
            }
                
            // producer
            var producerFactory = activityCreators.Values.FirstOrDefault(p =>
                p.ActivityType == PipelineActivityType.Producer &&
                p.OutputType == contextType && 
                p.InputType == null);
            if (producerFactory == null)
            {
                throw new InvalidOperationException($"Uanble to find activity '{PipelineActivityType.Producer}' with context type '{settings.ContextTypeName}'");
            }
            var producer = producerFactory.CreateActivity();
            
            activities.Add(producer);
            
            // batch
            var batchFactory = activityCreators.Values.FirstOrDefault(b =>
                b.ActivityType == PipelineActivityType.Batch &&
                b.InputType == contextType &&
                b.OutputType.IsArrayOf(contextType));
            if (batchFactory == null)
            {
                throw new InvalidOperationException($"Uanble to find activity '{PipelineActivityType.Batch}' with context type '{settings.ContextTypeName}'");
            }
            foreach (var index in Enumerable.Range(1, settings.Transformers.Count))
            {
                var batch = batchFactory.CreateActivity();
                
            }


            return new Pipeline(activities);
        }

        private void LoadActivities()
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (binFolder == null || !Directory.Exists(binFolder))
            {
                throw new ApplicationException("Unable to find bin folder");
            }

            var activityInterfaceName = typeof(IPipelineActivityFactory).FullName;
            if (activityInterfaceName == null)
            {
                throw new ApplicationException("Invalid base activity type");
            }

            var assembly = typeof(IPipelineActivityFactory).Assembly;
            assemblies.Add(assembly);
            
            Action<Assembly> addActivityCreators = a =>
            {
                foreach (var type in a.GetTypes())
                {
                    if (type.GetInterface(activityInterfaceName) != null)
                    {
                        var typeName = type.FullName;
                        if (typeName != null && !activityCreators.ContainsKey(typeName))
                        {
                            var activityCreator = serviceProvider.GetRequiredService(type) as IPipelineActivityFactory;
                            activityCreators.Add(typeName, activityCreator);
                        }
                    }
                }
            };
            addActivityCreators(assembly);         
            foreach (var assemblyFile in assemblySettings.AssemblyFiles)
            {
                assembly = Assembly.LoadFile(assemblyFile);
                assemblies.Add(assembly);
                addActivityCreators(assembly);
            }
        }
    }
}
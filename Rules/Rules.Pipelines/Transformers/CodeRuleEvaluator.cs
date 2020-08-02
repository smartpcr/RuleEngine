//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="PowerDeviceContextEvaluator.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using Pipelines;

    public abstract class CodeRuleEvaluator
        : BasePayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDocDbRepository<CodeRule> codeRuleRepo;
        private readonly JsonSerializerSettings errorSerializerSettings;
        private readonly ILogger<CodeRuleEvaluator> logger;
        private readonly IDocDbRepository<RuleSet> ruleSetRepo;
        private readonly object syncObj = new object();
        private CodeRule codeRule;

        protected CodeRuleEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(
            serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            logger = loggerFactory.CreateLogger<CodeRuleEvaluator>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            ruleSetRepo = repoFactory.CreateRepository<RuleSet>();
            codeRuleRepo = repoFactory.CreateRepository<CodeRule>();
            EnsureCodeRule();

            errorSerializerSettings = new JsonSerializerSettings
            {
                MaxDepth = 3,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            errorSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public abstract ContextErrorCode CodeRuleErrorCode { get; }
        public abstract string RuleName { get; }

        protected override DeviceValidationResult Transform(PowerDevice payload, PipelineExecutionContext context)
        {
            var result = new DeviceValidationResult
            {
                DeviceName = payload.DeviceName,
                ExecutionTime = DateTime.UtcNow,
                Score = 0,
                RunId = context.RunId,
                JobId = context.JobId,
                ValidationRuleId = codeRule.Id
            };

            try
            {
                EvaluateDevice(payload, context);

                // validated device have an empty list, use this to filter those who are out of scope
                var codeRuleEvidences = payload.ContextErrors?.Where(e => e.ErrorCode == CodeRuleErrorCode).ToList();
                if (codeRuleEvidences?.Count > 0)
                {
                    result.Assert = codeRuleEvidences.All(c => c.Passed);
                    result.Score = (decimal) codeRuleEvidences.Average(c => c.Score);

                    if (result.Assert == false)
                    {
                        context.AddTotalFailed(1);
                        result.Error = JsonConvert.SerializeObject(codeRuleEvidences, errorSerializerSettings);
                    }

                    context.AddTotalFiltered(1);
                    context.AddTotalEvaluated(1);
                    context.Scores.Add((payload.DeviceName, GetType().Name, result.Score.Value));
                    if (context.TotalEvaluated % 100 == 0)
                        logger.LogInformation($"total validated: {context.TotalEvaluated} by {GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed on rule {GetType().Name} and device {payload.DeviceName}, {ex.Message}");
                result.Error = ex.Message;
                context.AddTotalFailed(1);
            }

            return result;
        }

        protected abstract void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context);

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }

        private void EnsureCodeRule()
        {
            if (codeRule == null)
            {
                lock (syncObj)
                {
                    if (codeRule == null)
                    {
                        var currentUser = GetCurrentUser();
                        RuleSet codeRuleSet;
                        var ruleSets = ruleSetRepo.Query($"c.name = '{RuleSet.CodeRuleSetName}'").GetAwaiter()
                            .GetResult().ToList();
                        if (!ruleSets.Any())
                        {
                            codeRuleSet = new RuleSet
                            {
                                Name = RuleSet.CodeRuleSetName,
                                Hierarchies = new string[0],
                                Type = RuleType.CodeRule,
                                DataCenters = new string[0],
                                DeviceTypes = new string[0],
                                CreatedBy = currentUser,
                                CreationTime = DateTime.Now,
                                ModificationTime = DateTime.Now,
                                ModifiedBy = currentUser
                            };
                            codeRuleSet = ruleSetRepo.Create(codeRuleSet).GetAwaiter().GetResult();
                        }
                        else
                        {
                            codeRuleSet = ruleSets.First();
                        }

                        var existingCodeRules = codeRuleRepo.Query($"c.ruleSetId = '{codeRuleSet.Id}'").GetAwaiter()
                            .GetResult().ToList();
                        codeRule = existingCodeRules.FirstOrDefault(c => c.ErrorCode == CodeRuleErrorCode);
                        if (codeRule == null)
                        {
                            codeRule = new CodeRule
                            {
                                Name = RuleName ?? CodeRuleErrorCode.ToString(),
                                ErrorCode = CodeRuleErrorCode,
                                RuleSetId = codeRuleSet.Id,
                                Type = RuleType.CodeRule,
                                Weight = 1.0M,
                                ContextType = typeof(PowerDevice).FullName,
                                CreatedBy = currentUser,
                                CreationTime = DateTime.Now,
                                ModificationTime = DateTime.Now,
                                ModifiedBy = currentUser,
                                CoceRuleEvaluatorTypeName = GetType().FullName,
                                Contributors = new List<string>(),
                                Owners = new List<string>(),
                                Description = ""
                            };
                            codeRule = codeRuleRepo.Create(codeRule).GetAwaiter().GetResult();
                        }
                    }
                }
            }
        }

        private string GetCurrentUser()
        {
            var principal = serviceProvider.GetService<IPrincipal>();
            return principal?.Identity.Name ?? Thread.CurrentPrincipal?.Identity?.Name ?? Environment.UserName;
        }
    }
}
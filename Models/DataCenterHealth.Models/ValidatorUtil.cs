// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidatorUtil.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System.Linq;
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;

    public static class ValidationBuilder
    {
        public static IServiceCollection AddValidator(this IServiceCollection services)
        {
            var validatorTypes = typeof(BaseEntity).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(IValidator))).ToList();
            foreach (var validatorType in validatorTypes)
            {
                // TODO: add validator with type name
            }

            return services;
        }
    }
}
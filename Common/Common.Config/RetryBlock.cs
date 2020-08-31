// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryBlock.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Config
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class RetryBlock
    {
        public static async Task RetryOnThrottling(int times, TimeSpan delay, Func<Task> operation, ILogger logger, Predicate<Exception> exceptionFilter = null)
        {
            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    await operation();
                    break; // success
                }
                catch (Exception ex)
                {
                    if ((exceptionFilter?.Invoke(ex) == true || exceptionFilter == null) && attempts < times)
                    {
                        logger?.LogError(ex, ex.Message);
                        await Task.Delay(delay);
                    }
                    else
                    {
                        logger?.LogError(ex, $"failed after {attempts} attempts");
                        throw;
                    }
                }
            } while (true);
        }
    }
}
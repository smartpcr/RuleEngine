// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BddSetup.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using LightBDD.Core.Configuration;
    using LightBDD.Framework.Configuration;
    using LightBDD.Framework.Reporting.Formatters;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BddSetup
    {
        [AssemblyInitialize]
        public static void Setup(TestContext testContext) { LightBddScope.Initialize(OnConfigure); }
        [AssemblyCleanup]
        public static void Cleanup() { LightBddScope.Cleanup(); }
        
        /// <summary>
        /// This method allows to customize LightBDD behavior.
        /// The code below configures LightBDD to produce also a plain text report after all tests are done.
        /// More information on what can be customized can be found on wiki: https://github.com/LightBDD/LightBDD/wiki/LightBDD-Configuration#configurable-lightbdd-features 
        /// </summary>
        private static void OnConfigure(LightBddConfiguration configuration)
        {
            configuration
                .ReportWritersConfiguration()
                .AddFileWriter<XmlReportFormatter>("~\\Reports\\FeaturesReport.xml")
                .AddFileWriter<PlainTextReportFormatter>("~\\Reports\\{TestDateTimeUtc:yyyy-MM-dd-HH_mm_ss}_FeaturesReport.txt");
        }
    }
}
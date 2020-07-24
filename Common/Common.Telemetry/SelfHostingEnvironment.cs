namespace Common.Telemetry
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.FileProviders;

    /// <summary>
    ///     This is a simple wrapper of Microsoft.AspNetCore.Hosting.IHostingEnvironment.
    ///     It is mainly to mitigate the fact there is no class that implements both
    ///     that and Microsoft.Extensions.Hosting.IHostingEnvironment.
    /// </summary>
    public sealed class SelfHostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
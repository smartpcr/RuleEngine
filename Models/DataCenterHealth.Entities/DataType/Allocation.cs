namespace DataCenterHealth.Entities.Location
{
    using System;
    using DataCenterHealth.Models;

    [CosmosReader("power-reference-prod", "power-reference-db", "Allocation", "power-reference-prod-authkey", "Target")]
    [CosmosWriter("xd-dev", "metadata", "allocation", "xd-dev-authkey", "target", "target")]
    [TrackChange(true)]
    public class Allocation : BaseEntity
    {
        public string DcName { get; set; }
        public string Source { get; set; }
        private string target;
        public string Target
        {
            get => target;
            set
            {
                target = value;
                if (!string.IsNullOrEmpty(target))
                {
                    var parts = target.Split(new[] {'.'}, 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        DeviceName = parts[0];
                        ChannelType = parts[1];
                        Channel = parts[2];
                        DataPoint = $"{ChannelType}.{Channel}";
                    }
                }
            }
        }
        public string DeviceName { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public string DataPoint { get; set; }
    }
}
namespace Common.DocDb
{
    using System;

    public static class TypeExtension
    {
        public static DateTime ToDate(this long ts)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(ts);
        }

        public static long ToTs(this DateTime date)
        {
            var seconds = (date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return (long) seconds;
        }
    }
}
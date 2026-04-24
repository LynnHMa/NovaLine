using System;

namespace NovaLine.Script.Utils
{
    public static class TimeStampTool
    {
        private static readonly DateTime UnixEpoch = new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region Timestamp => TimeString
        public static string ToDateTimeString(string timeStamp)
        {
            if (string.IsNullOrEmpty(timeStamp)) return "???";

            return timeStamp.Length switch
            {
                13 when long.TryParse(timeStamp, out long ms) => ToDateTimeString(ms / 1000),
                10 when long.TryParse(timeStamp, out long sec) => ToDateTimeString(sec),
                _ => "???"
            };
        }
        
        public static string ToDateTimeString(long timeStamp)
        {
            try
            {
                DateTime utcTime = UnixEpoch.AddSeconds(timeStamp);
                DateTime localTime = utcTime.ToLocalTime();
                return localTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "???";
            }
        }
        #endregion

        #region TimeString => Timestamp
        public static long GetNowTimeStamp()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }
        
        public static long ToTimeStamp(DateTime dateTime)
        {
            DateTime utcTime = dateTime.ToUniversalTime();
            return (long)(utcTime - UnixEpoch).TotalSeconds;
        }
        #endregion
    }
}
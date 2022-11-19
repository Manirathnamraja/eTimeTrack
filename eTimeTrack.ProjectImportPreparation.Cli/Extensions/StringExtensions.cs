using System;

namespace eTimeTrack.ProjectImportPreparation.Cli.Extensions
{
    static class StringExtensions
    {
        public static string ToStringOrNullString(this int? val)
        {
            if (val == null)
                return "null";

            return val.Value.ToString();
        }

        public static string ToNullableString(this DateTime? val)
        {
            if (val == null)
                return "null";

            return "'" + val.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }

        public static string ToNullableString(this string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return "null";

            return "'" + val.Replace("'", "''") + "'";
        }
    }
}

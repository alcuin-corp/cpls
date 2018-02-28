using System;
using System.IO;

namespace PLS.Utils
{
    public static class DateTimeExt
    {
        public static string PostfixBackup(this DateTime date, string filename)
        {
            var fnwext = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);
            return $"{fnwext}-{date:yyyyMMddHHmmssfff}{ext}";
        }
    }
}
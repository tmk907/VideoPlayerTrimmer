using System;
using System.Collections.Generic;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class LibVlcOptions
    {
        public static string GetSubtitleEncoding(string encoding = Encoding.UTF8)
        {
            return $":subsdec-encoding={encoding}";
        }

        public class Encoding
        {
            public const string UTF8 = "UTF-8";
            public const string Windows1250 = "Windows-1250";
            public const string Windows1252 = "Windows-1252";
            public const string UTF16 = "UTF-16";
            public const string ISO88591 = "ISO-8859-1";
            public const string ISO88592 = "ISO-8859-2";
            public const string CP1250 = "CP-1250";

            public static List<string> All = new List<string>()
            {
                UTF8,
                Windows1250,
                Windows1252,
                UTF16,
                ISO88591,
                ISO88592,
                CP1250
            };
        }
    }
}

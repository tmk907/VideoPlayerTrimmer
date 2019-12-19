using System.Collections.Generic;

namespace VideoPlayerTrimmer.Services
{
    public static class FFmpegCommandsExtensions
    {
        public static IList<string> AddInput(this IList<string> commands, string filePath)
        {
            commands.Add("-i");
            commands.Add(filePath);
            return commands;
        }

        public static IList<string> AddOption(this IList<string> commands, string option, string value = null)
        {
            commands.Add(option);
            if (value != null)
            {
                commands.Add(value);
            }
            return commands;
        }

        public static IList<string> AddOutput(this IList<string> commands, string filePath)
        {
            commands.Add(filePath);
            return commands;
        }
    }
}

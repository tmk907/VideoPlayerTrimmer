using System;

namespace VideoPlayerTrimmer
{
    public class LoggerArgs
    {
        private readonly string[] args;

        public LoggerArgs(params string[] args)
        {
            this.args = args;
        }
        public override string ToString()
        {
            return String.Join(", ", args);
        }
    }
}

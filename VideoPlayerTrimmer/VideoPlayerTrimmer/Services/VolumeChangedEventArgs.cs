using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public class VolumeChangedEventArgs : EventArgs
    {
        public VolumeChangedEventArgs(int volume)
        {
            Volume = volume;
        }

        public int Volume { get; set; }
    }
}

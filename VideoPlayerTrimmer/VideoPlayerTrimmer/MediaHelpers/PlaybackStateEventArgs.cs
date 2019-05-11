using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.MediaHelpers
{
    public class PlaybackStateEventArgs : EventArgs
    {
        public PlaybackStateEventArgs(PlaybackState playbackState)
        {
            PlaybackState = playbackState;
        }

        public PlaybackState PlaybackState { get; }
    }
}

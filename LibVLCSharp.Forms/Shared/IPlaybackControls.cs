using System;
using LibVLCSharp.Shared;

namespace LibVLCSharp.Forms.Shared
{
    public interface IPlaybackControls
    {
        LibVLC LibVLC { get; set; }
        MediaPlayer MediaPlayer { get; set; }
        VideoView VideoView { get; set; }

        void GestureRecognized(object sender, EventArgs e);
    }
}

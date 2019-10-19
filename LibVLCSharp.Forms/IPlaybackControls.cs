using System.Threading.Tasks;
using LibVLCSharp.Forms.Shared;
using LibVLCSharp.Shared;

namespace LibVLCSharp.Forms
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public interface IPlaybackControls
    {
        LibVLC LibVLC { get; set; }
        LibVLCSharp.Shared.MediaPlayer MediaPlayer { get; set; }
        VideoView VideoView { get; set; }
        Task FadeInAsync();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

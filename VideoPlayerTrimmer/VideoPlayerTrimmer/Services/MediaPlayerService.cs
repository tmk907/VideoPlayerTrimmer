using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Services
{
    public class MediaPlayerService
    {
        public MediaPlayerService()
        {
            Task.Run((Action)Initialize);
        }

        private LibVLC LibVLC { get; set; }

        private void Initialize()
        {
            Core.Initialize();
            LibVLC = new LibVLC();
        }

        public MediaPlayer GetMediaPlayer(string filePath)
        {
            var media = new Media(LibVLC, filePath);
            if (Settings.UseHardwareAcceleration)
            {
                var configuration = new MediaConfiguration();
                configuration.EnableHardwareDecoding();
                media.AddOption(configuration);
            }
            var mediaPlayer = new MediaPlayer(media);
            return mediaPlayer;
        }
    }
}

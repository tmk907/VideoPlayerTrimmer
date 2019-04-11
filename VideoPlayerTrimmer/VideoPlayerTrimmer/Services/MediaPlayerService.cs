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

        public MediaPlayer MediaPlayer { get; private set; }

        private void Initialize()
        {
            Core.Initialize();

            LibVLC = new LibVLC();
            MediaPlayer = new MediaPlayer(LibVLC);
        }

        public Media GetMedia(string path)
        {
            return new Media(LibVLC, path, FromType.FromPath);
        }

        public MediaPlayer GetMediaPlayer(string filePath)
        {
            return new MediaPlayer(new Media(LibVLC, filePath));
        }
    }
}

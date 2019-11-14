﻿using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;

namespace VideoPlayerTrimmer.Services
{
    public class MediaPlayerBuilder
    {
        public MediaPlayerBuilder()
        {
        }

        private Lazy<LibVLC> libVLC = new Lazy<LibVLC>(() => { Core.Initialize(); return new LibVLC(); });

        public LibVLC LibVLC
        {
            get
            {
                return libVLC.Value;
            }
        }

        public MediaPlayer GetMediaPlayer(string filePath)
        {
            var media = new Media(LibVLC, filePath);
            if (Settings.UseHardwareAcceleration)
            {
                var configuration = new MediaConfiguration();
                configuration.EnableHardwareDecoding = true;
                media.AddOption(configuration);
            }
            var mediaPlayer = new MediaPlayer(media);
            return mediaPlayer;
        }

        public MediaPlayer GetMediaPlayerForTrimming(string filePath, string outputPath, int startSec, int endSec)
        {
            var lib = new LibVLC($"--start-time={startSec}", $"--stop-time={endSec}");
            var media = new Media(lib, filePath);
            var option = ":sout=#transcode{scodec=none}:std{access=file{overwrite},mux=mp4,dst='" + outputPath + "'}";
            media.AddOption(option);
            var mediaPlayer = new MediaPlayer(media);
            return mediaPlayer;
        }

        public MediaPlayer GetMediaPlayerForTrimming(string filePath, string outputPath, string startSec, string endSec)
        {
            var lib = new LibVLC($"--start-time={startSec}", $"--stop-time={endSec}");
            var media = new Media(lib, filePath);
            var option = ":sout=#transcode{scodec=none}:std{access=file{overwrite},mux=mp4,dst='" + outputPath + "'}";
            media.AddOption(option);
            var mediaPlayer = new MediaPlayer(media);
            return mediaPlayer;
        }

        public MediaPlayer GetMediaPlayer(string filePath, List<string> options)
        {
            var media = new Media(LibVLC, filePath);
            if (Settings.UseHardwareAcceleration)
            {
                var configuration = new MediaConfiguration();
                configuration.EnableHardwareDecoding = true;
                media.AddOption(configuration);
            }
            foreach(var option in options)
            {
                media.AddOption(option);
            }
            var mediaPlayer = new MediaPlayer(media);
            return mediaPlayer;
        }
    }
}
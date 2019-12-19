using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NL.Bravobit.Ffmpeg;
using Plugin.CurrentActivity;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FFmpegConverter))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class FFmpegConverter : IFFmpegConverter
    {
        public event EventHandler<int> ConversionProgressChanged;
        public event EventHandler ConversionStarted;
        public event EventHandler ConversionEnded;

        public Task Convert(List<string>commands, TimeSpan start, TimeSpan end)
        {
            App.DebugLog("FFmpeg command:");
            App.DebugLog(string.Join(" ", commands));

            return Task.Factory.StartNew(() => ExcecuteCommand(commands.ToArray(), start, end), TaskCreationOptions.LongRunning);
        }

        private void ExcecuteCommand(string[] cmd, TimeSpan start, TimeSpan end)
        {
            var ffmpeg = FFmpeg.GetInstance(CrossCurrentActivity.Current.Activity.ApplicationContext);
            if (ffmpeg.IsSupported)
            {
                App.DebugLog("Start conversion");
                ffmpeg.Execute(cmd.ToArray(), new MyExecuteBinaryResponseHandler2(this, start, end));
                App.DebugLog("Stop conversion");
            }
        }

        public void OnStart()
        {
            ConversionStarted?.Invoke(this, new EventArgs());
        }

        public void OnComplete()
        {
            ConversionEnded?.Invoke(this, new EventArgs());
        }

        public void OnProgessChanged(int percent)
        {
            ConversionProgressChanged?.Invoke(this, percent);
        }
    }

    public class MyExecuteBinaryResponseHandler2 : ExecuteBinaryResponseHandler
    {
        private readonly FFmpegConverter converter;
        private readonly TimeSpan _start;
        private readonly TimeSpan _end;
        private readonly TimeSpan _duration;

        public MyExecuteBinaryResponseHandler2(FFmpegConverter converter, TimeSpan start, TimeSpan end)
        {
            this.converter = converter;
            _start = start;
            _end = end;
            _duration = end - start;
        }

        public override void OnStart()
        {
            App.DebugLog("");
            converter.OnStart();
            base.OnStart();
        }

        public override void OnFinish()
        {
            App.DebugLog("");
            converter.OnComplete();
            base.OnFinish();
        }

        public override void OnProgress(string message)
        {
            App.DebugLog(message);
            base.OnProgress(message);
            //frame= 1939 fps= 35 q=22.0 size=   17152kB time=00:05:20.81 bitrate= 438.0kbits/s speed=5.78x
            if (message.StartsWith("frame"))
            {
                int start = message.IndexOf("time=") + "time=".Length;
                int end = message.IndexOf(' ', start);
                string duration = message.Substring(start, end - start);
                TimeSpan position = TimeSpan.Zero;
                TimeSpan.TryParse(duration, out position);
                if (position != TimeSpan.Zero)
                {
                    int progress = (int)(position.TotalSeconds / _duration.TotalSeconds* 100);
                    converter.OnProgessChanged(progress);
                    App.DebugLog("Conversion progress " + progress + "%");
                }
            }
        }

        public override void OnFailure(string message)
        {
            App.DebugLog(message);
            converter.OnComplete();
            base.OnFailure(message);
        }

        public override void OnSuccess(string message)
        {
            App.DebugLog(message);
            converter.OnComplete();
            base.OnSuccess(message);
        }
    }
}
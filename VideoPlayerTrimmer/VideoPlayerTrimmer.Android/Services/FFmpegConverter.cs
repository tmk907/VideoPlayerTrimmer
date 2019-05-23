using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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

        public Task CovertToVideo(FFmpegToVideoConversionOptions options)
        {
            var commands = new List<string>();
            commands.Add("-y");
            commands.Add("-ss");
            commands.Add(options.StartPosition.ToString("g").Replace(',', '.'));
            commands.Add("-i");
            commands.Add(options.InputFilePath);
            commands.Add("-to");
            commands.Add(options.EndPosition.ToString("g").Replace(',', '.'));
            commands.Add("-copyts");
            commands.Add("-c:v");
            commands.Add(options.VideoCodec);
            commands.Add("-preset");
            commands.Add(options.Preset);
            commands.Add("-c:a");
            commands.Add(options.AudioCodec);
            commands.Add("-avoid_negative_ts");
            commands.Add("1");
            commands.Add(options.OutputFilePath);
            return Task.Factory.StartNew(() => ExcecuteCommand(commands.ToArray(), options), TaskCreationOptions.LongRunning);
        }

        public Task ConvertToGif(FFmpegToGifConversionOptions options)
        {
            var commands= new List<string>();
            commands.Add("-y");
            commands.Add("ss");
            commands.Add(options.StartPosition.ToString("g").Replace(',', '.'));
            commands.Add("-t");
            commands.Add(options.Duration.ToString("g").Replace(',', '.'));
            commands.Add("-i");
            commands.Add(options.InputFilePath);
            commands.Add("-filter_complex");
            commands.Add($"\"[0:v] fps=12,scale={options.Width}:-1,split [a][b];[a] palettegen [p];[b][p] paletteuse\"");
            commands.Add(options.OutputFilePath);
            return Task.Factory.StartNew(() => ExcecuteCommand(commands.ToArray(), options));
        }

        private void ExcecuteCommand(string[] cmd, FFmpegConversionOptions options)
        {
            var ffmpeg = FFmpeg.GetInstance(CrossCurrentActivity.Current.Activity.ApplicationContext);
            if (ffmpeg.IsSupported)
            {
                App.DebugLog("Start conversion");
                ffmpeg.Execute(cmd.ToArray(), new MyExecuteBinaryResponseHandler2(this, options));
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
        private Action<int> onProgressAction;
        private readonly FFmpegConverter converter;
        private readonly FFmpegConversionOptions options;

        public MyExecuteBinaryResponseHandler2(FFmpegConverter converter, FFmpegConversionOptions options)
        {
            this.converter = converter;
            this.options = options;
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
                    int progress = (int)((position.TotalSeconds - options.StartPosition.TotalSeconds) / options.Duration.TotalSeconds* 100);
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
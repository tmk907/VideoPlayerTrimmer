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
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

using NL.Bravobit.Ffmpeg;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(FFmpegService2))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class FFmpegService2 : IFFmpegService
    {
        public async Task Test(string path)
        {
            var ffmpeg = FFmpeg.GetInstance(CrossCurrentActivity.Current.Activity.ApplicationContext);
            if (ffmpeg.IsSupported)
            {
                string directory = System.IO.Path.GetDirectoryName(path);
                string outputPath = System.IO.Path.Combine(directory, "outVid.mp4");
                var commands = new List<string>();
                commands.Add("-y");
                commands.Add("-ss");
                commands.Add("0:30");
                commands.Add("-t");
                commands.Add("0:15");
                commands.Add("-noaccurate_seek");
                commands.Add("-i");
                commands.Add(path);
                commands.Add("-codec");
                commands.Add("copy");
                commands.Add("-avoid_negative_ts");
                commands.Add("1");
                commands.Add(outputPath);
                ffmpeg.Execute(commands.ToArray(), new MyExecuteBinaryResponseHandler());
            }
            await Task.CompletedTask;
        }

        public async Task Trim(string[] cmd)
        {
            var ffmpeg = FFmpeg.GetInstance(CrossCurrentActivity.Current.Activity.ApplicationContext);
            if (ffmpeg.IsSupported)
            {
                ffmpeg.Execute(cmd.ToArray(), new MyExecuteBinaryResponseHandler());
            }
            await Task.CompletedTask;
        }
    }

    public class MyExecuteBinaryResponseHandler : ExecuteBinaryResponseHandler
    {
        public override void OnStart()
        {
            base.OnStart();
            System.Diagnostics.Debug.WriteLine("FFMPEG OnStart");
        }

        public override void OnFinish()
        {
            base.OnFinish();
            System.Diagnostics.Debug.WriteLine("FFMPEG OnFinnish");

        }

        public override void OnProgress(string message)
        {
            base.OnProgress(message);
            //frame= 1939 fps= 35 q=22.0 size=   17152kB time=00:05:20.81 bitrate= 438.0kbits/s speed=5.78x
            if (message.StartsWith("frame"))
            {
                int start = message.IndexOf("time=") + "time=".Length;
                int end = message.IndexOf(' ', start);
                string duration = message.Substring(start, end - start);
                TimeSpan position = TimeSpan.Zero;
                TimeSpan.TryParse(duration, out position);
                System.Diagnostics.Debug.WriteLine("FFMPEG OnProgress" + message + " " + duration);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("FFMPEG OnProgress" + message);
            }
        }

        public override void OnFailure(string message)
        {
            base.OnFailure(message);
            System.Diagnostics.Debug.WriteLine("FFMPEG Onfailure" + message);

        }
        public override void OnSuccess(string message)
        {
            base.OnSuccess(message);
            System.Diagnostics.Debug.WriteLine("FFMPEG OnSuccess" + message);
        }
    }
}
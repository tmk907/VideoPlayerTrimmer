using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using VideoPlayerTrimmer.Droid.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Droid.BroadcastReceivers
{
    [BroadcastReceiver(Enabled = false, Exported = true)]
    class VolumeChangeBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            int volume = intent.GetIntExtra("android.media.EXTRA_VOLUME_STREAM_VALUE", 0);
            MessagingCenter.Send<VolumeChangeBroadcastReceiver, int>(this, "VolumeChange", volume);
        }
    }
}
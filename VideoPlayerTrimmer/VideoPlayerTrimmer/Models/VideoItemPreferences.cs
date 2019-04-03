using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class VideoItemPreferences : BindableBase
    {
        private TimeSpan position;
        public TimeSpan Position
        {
            get { return position; }
            set { position = value; }
        }



    }
}

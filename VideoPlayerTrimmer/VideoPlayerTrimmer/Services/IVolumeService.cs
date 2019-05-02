using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public interface IVolumeService
    {
        event EventHandler<VolumeChangedEventArgs> VolumeChanged;
        void SetVolume(int volume);
        int GetMaxVolume();
        int GetVolume();
    }
}

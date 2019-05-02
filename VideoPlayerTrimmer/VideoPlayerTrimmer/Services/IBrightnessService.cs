using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public interface IBrightnessService
    {
        float GetBrightness();

        void SetBrightness(float brightness);

        void RestoreBrightness();
    }
}

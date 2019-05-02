using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public interface IOrientationService
    {
        void RestoreOrientation();

        void ChangeToLandscape();
    }
}

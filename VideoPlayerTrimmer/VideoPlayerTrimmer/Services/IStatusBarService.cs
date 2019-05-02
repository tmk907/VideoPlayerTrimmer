using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public interface IStatusBarService
    {
        bool IsVisible { get; set; }
    }
}

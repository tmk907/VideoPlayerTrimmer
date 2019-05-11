using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseViewModel : BindableBase
    {
        public virtual Task OnAppearingAsync(bool firstTime) => Task.CompletedTask;

        public virtual Task OnDisappearingAsync(bool firstTime) => Task.CompletedTask;

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }
    }
}

using AsyncAwaitBestPractices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Framework
{
    public abstract class BaseViewModel : BindableBase
    {
        protected CancellationTokenSource cts;

        protected bool firstTimeAppeared = true;
        protected bool firstTimeDisappeared = true;

        protected Dictionary<string, string> navigationParameters;

        private bool isLoadingData;
        public bool IsLoadingData
        {
            get => isLoadingData;
            set => SetProperty(ref isLoadingData, value);
        }

        public virtual void OnNavigating(Dictionary<string, string> navigationArgs)
        {
            navigationParameters = navigationArgs;
        }

        public virtual void OnAppearing(bool firstTime)
        {
            firstTimeAppeared = firstTime;
            cts = new CancellationTokenSource();
            InitializeVMAsyncInternal(cts.Token).SafeFireAndForget(true);
        }

        public virtual void OnDisappearing(bool firstTime)
        {
            firstTimeDisappeared = firstTime;
            cts.Cancel();
            UnInitializeVMAsync().SafeFireAndForget(true);
        }

        protected async Task InitializeVMAsyncInternal(CancellationToken token)
        {
            IsLoadingData = true;
            await InitializeVMAsync(token);
            IsLoadingData = false;
        }

        protected virtual Task InitializeVMAsync(CancellationToken token) => Task.CompletedTask;

        protected virtual Task UnInitializeVMAsync() => Task.CompletedTask;

    }
}

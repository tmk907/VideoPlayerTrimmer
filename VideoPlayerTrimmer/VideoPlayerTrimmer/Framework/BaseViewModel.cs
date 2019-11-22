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

        private bool wasResumeCalled = false;
        private bool wasDisappearedCalled = false;

        public BaseViewModel()
        {
            App.OnResumed += App_OnResumed;
            App.OnSuspended += App_OnSuspended;
        }
        
        private void App_OnResumed()
        {
            OnResume();
        }

        private void App_OnSuspended()
        {
            OnSuspend();
        }

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

        public void OnAppearing(bool firstTime)
        {
            App.DebugLog(this.GetType().ToString());

            firstTimeAppeared = firstTime;
            wasDisappearedCalled = false;
            if (wasResumeCalled) return;
            cts = new CancellationTokenSource();
            InitializeVMAsyncInternal(cts.Token).SafeFireAndForget(true);
        }

        public void OnDisappearing(bool firstTime)
        {
            App.DebugLog(this.GetType().ToString());

            wasResumeCalled = false;
            wasDisappearedCalled = true;

            firstTimeDisappeared = firstTime;
            cts.Cancel();
            UnInitializeVMAsync().SafeFireAndForget(true);
        }

        private void OnResume()
        {
            App.DebugLog(this.GetType().ToString());

            wasResumeCalled = true;
            wasDisappearedCalled = false;

            cts = new CancellationTokenSource();
            InitializeVMAsyncInternal(cts.Token).SafeFireAndForget(true);
        }

        private void OnSuspend()
        {
            App.DebugLog(this.GetType().ToString());

            wasResumeCalled = false;
            if (wasDisappearedCalled)
            {
                wasDisappearedCalled = false;
                return;
            }

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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;

namespace BetaSeriesW8
{
    public sealed class MiseAJourDesEpisodesTask : IBackgroundTask
    {
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        ThreadPoolTimer _periodicTimer = null;
        uint _progress = 0;
        IBackgroundTaskInstance _taskInstance = null;

        //
        // The Run method is the entry point of a background task.
        //
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            //
            // Associate a cancellation handler with the background task.
            //
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            //
            _deferral = taskInstance.GetDeferral();
            _taskInstance = taskInstance;

            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromMinutes(2));
        }

        //
        // Handles background task cancellation.
        //
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }

        //
        // Simulate the background task activity.
        //
        private async void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            //if ((_cancelRequested == false) && (_progress < 100))
            //{
            //    _progress += 10;
            //    _taskInstance.Progress = _progress;
            //}
            //else
            //{
            //    _periodicTimer.Cancel();

            //    var settings = ApplicationData.Current.LocalSettings;
            //    var key = _taskInstance.Task.Name;

            //    //
            //    // Write to LocalSettings to indicate that this background task ran.
            //    //
            //    settings.Values[key] = (_progress < 100) ? "Canceled" : "Completed";
            //    Debug.WriteLine("Background " + _taskInstance.Task.Name + ((_progress < 100) ? "Canceled" : "Completed"));

            //    //
            //    // Indicate that the background task has completed.
            //    //

            //}

            await ServicesTiles.MettreAJourLesTilesEpisodes();
            _deferral.Complete();
        }
    }
}

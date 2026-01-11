using System;
using System.Windows.Threading;

namespace AqiChart.Client.Common
{
    public class ThrottleManager
    {
        private DispatcherTimer timer;
        private DateTime lastExecutionTime;
        private bool isThrottling;
        private TimeSpan timeSpan;

        public ThrottleManager(TimeSpan timeSpan, bool isThrottling)
        {
            this.timeSpan = timeSpan;
            this.isThrottling = isThrottling;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lastExecutionTime = DateTime.Now;
            timer.Stop();
        }

        public void Execute(Action action)
        {
            DateTime currentTime = DateTime.Now;

            if (isThrottling)
            {
                if (lastExecutionTime != default && currentTime - lastExecutionTime < timeSpan)
                {
                    timer.Interval = timeSpan - (currentTime - lastExecutionTime);
                    timer.Start();
                    return;
                }
            }
            else // Debouncing
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
            }

            action();
            lastExecutionTime = currentTime;
        }
    }



    //使用方法：

    //节流（Throttling）:
    //ThrottleManager throttleManager = new ThrottleManager(TimeSpan.FromSeconds(1), true);
    //private void Button_Click()
    //{
    //    throttleManager.Execute(() =>
    //    {
    //        // Your code here
    //    });
    //}

    //防抖（Debouncing）:
    //ThrottleManager debounceManager = new ThrottleManager(TimeSpan.FromSeconds(1), false);
    //private void Button_Click()
    //{
    //    debounceManager.Execute(() =>
    //    {
    //        // Your code here
    //    });
    //}
}

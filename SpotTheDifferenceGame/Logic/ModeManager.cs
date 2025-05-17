using System;
using System.Windows.Forms;

namespace SpotTheDifferenceGame.Logic
{
    class ModeManager
    {
        private Timer timer;
        public int TimeLeft { get; private set; }

        public event Action Tick;
        public event Action TimeUp;

        public ModeManager()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
        }

        public void StartTimer(int seconds)
        {
            TimeLeft = seconds;
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        public void Reset()
        {
            StopTimer();
            TimeLeft = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeLeft--;
            Tick?.Invoke();

            if (TimeLeft <= 0)
            {
                StopTimer();
                TimeUp?.Invoke();
            }
        }
    }
}

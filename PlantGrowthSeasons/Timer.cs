using Eco.Core.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantGrowthSeasons
{
    public class TimerTask
    {
        private readonly PeriodicTimer timer;
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private Task? timerTask;

        public string? Identifier { get; private set; }

        public TimerTask(TimeSpan interval) => this.timer = new PeriodicTimer(interval);

        public void Start(Action routine, string id)
        {
            this.Identifier = id;
            this.timerTask = this.DoWorkAsync(routine);
        }

        private async Task DoWorkAsync(Action routine)
        {
            try
            {
                while (true)
                {
                    if (await this.timer.WaitForNextTickAsync(this.cancellationToken.Token))
                        routine();
                    else
                        break;
                }
            }
            catch (OperationCanceledException ex)
            {
                ConsoleLogWriter.Instance.Write("Timer: " + this.Identifier + " has been cancelled.");
            }
        }

        public async Task StopAsync()
        {
            if (this.timerTask == null)
                return;
            this.cancellationToken.Cancel();
            await this.timerTask;
            this.cancellationToken.Dispose();
        }
    }
}

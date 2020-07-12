using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace PingThings.Model
{
    public class PingGroup : INotifyPropertyChanged
    {
        private Timer PingTimer { get; set; }
        private Timer ElapsedTimer { get; set; }
        private int Interval { get; set; } = 0;

        public LiveGraph LiveGraph { get; set; } = new LiveGraph();

        private string _GroupName;
        public string GroupName
        {
            get => _GroupName;
            set
            {
                if (_GroupName != value)
                {
                    _GroupName = value;
                    RaisePropertyChanged("GroupName");
                }
            }
        }

        private DateTime StartDateTime;

        private string _DisplayableInterval;
        public string DisplayableInterval
        {
            get => _DisplayableInterval;
            set
            {
                if (_DisplayableInterval != value)
                {
                    _DisplayableInterval = value;
                    RaisePropertyChanged("DisplayableInterval");
                }
            }
        }

        private string _DisplayableElapsedTime;
        public string DisplayableElapsedTime
        {
            get => _DisplayableElapsedTime;
            set
            {
                if (_DisplayableElapsedTime != value)
                {
                    _DisplayableElapsedTime = value;
                    RaisePropertyChanged("DisplayableElapsedTime");
                }
            }
        }
        public ObservableCollection<PingThing> Pings { get; set; } = new ObservableCollection<PingThing>();

        private void SendPings(object state)
        {
            foreach (PingThing ping in Pings)
            {
                (int, int) NewPingInfo = ping.SendPing();

                LiveGraph.UpdateLiveGraphData(ping.Label, NewPingInfo.Item1, NewPingInfo.Item2, DateTime.Now);
            }
        }

        private void ElapsedUpdated(object state)
        {
            string GetUsableTimeComponent(int TimeComponent, string ComponentName) =>
                (TimeComponent != 0, TimeComponent == 1) switch
                {
                    (false, _) => "None",
                    (true, true) => $"1 {ComponentName}",
                    (true, false) => $"{TimeComponent} {ComponentName}s"
                };

            TimeSpan TimeFromStart = DateTime.Now.Subtract(StartDateTime);

            string MinuteComponent = GetUsableTimeComponent(TimeFromStart.Minutes, "minute");
            string HourComponent = GetUsableTimeComponent(TimeFromStart.Hours, "hour");
            string DayComponent = GetUsableTimeComponent(TimeFromStart.Days, "day");

            string TimeToSet = "Running for: ";

            if (DayComponent != "None")
            {
                TimeToSet += $"{DayComponent} ";
            }
            if (HourComponent != "None")
            {
                TimeToSet += $"{HourComponent} ";
            }
            if (MinuteComponent != "None")
            {
                TimeToSet += $"{MinuteComponent}";
            }

            if (DayComponent == "None" && HourComponent == "None" && MinuteComponent == "None")
            {
                TimeToSet += "Less than a minute";
            }

            DisplayableElapsedTime = TimeToSet;
        }

        public void StartPinging(int interval)
        {
            Interval = interval;

            DisplayableInterval = $"This groups ping interval: {Interval} seconds";

            StartDateTime = DateTime.Now;

            foreach (PingThing ping in Pings)
            {
                LineSeries latencyLine = new LineSeries
                {
                    Title = ping.Label,
                    Values = new ChartValues<LiveLatencyModel>()
                };

                ColumnSeries statusColumns = new ColumnSeries
                {
                    Title = ping.Label,
                    Values = new ChartValues<ObservableValue>()
                    {
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0),
                        new ObservableValue(0)
                    }
                };

                LiveGraph.LatencySeriesCollection.Add(latencyLine);
                LiveGraph.StatusSeriesCollection.Add(statusColumns);
            }

            PingTimer = new Timer(SendPings, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(interval));
            ElapsedTimer = new Timer(ElapsedUpdated, null, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
        }

        public void StopPinging()
        {
            PingTimer.Dispose();
            ElapsedTimer.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

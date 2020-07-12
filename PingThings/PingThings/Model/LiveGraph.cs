using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PingThings.Model
{
    public class LiveGraph : INotifyPropertyChanged
    {
        public ObservableCollection<string> StatusLabels { get; set; } = new ObservableCollection<string>();

        private string _LiveStatusDescription;
        public string LiveStatusDescription
        {
            get => _LiveStatusDescription;
            set
            {
                if (_LiveStatusDescription != value)
                {
                    _LiveStatusDescription = value;
                    RaisePropertyChanged("LiveStatusDescription");
                }
            }
        }

        private double _YAxisStep;
        public double YAxisStep
        {
            get => _YAxisStep;
            set
            {
                if (_YAxisStep != value)
                {
                    _YAxisStep = value;
                    RaisePropertyChanged("YAxisStep");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<DateTime> LiveStatusTimes = new List<DateTime>();

        public SeriesCollection LatencySeriesCollection { get; set; } = new SeriesCollection();

        public SeriesCollection StatusSeriesCollection { get; set; } = new SeriesCollection();

        private bool ProgressionDrop = false;

        public LiveGraph()
        {
            LiveStatusDescription = "Showing the last hour of failed pings";
            YAxisStep = 1;
            SetStatusTimeProgression(6, DateTime.Now);
        }

        private int MultipleInMinutes = 10;
        private void SetStatusTimeProgression(int BarCount, DateTime StartTime)
        {
            LiveStatusTimes.Clear();
            StatusLabels.Clear();

            string GetReadableTimeSpan(int index, DateTime ProgressedTime) =>
                (index == 0) switch
                {
                    true => $"{StartTime.ToString("hh:mm tt")} - {StartTime.AddMinutes(1 * MultipleInMinutes).ToString("hh:mm tt")}",
                    false => $"{StartTime.AddMinutes(index * MultipleInMinutes).ToString("hh:mm tt")} - {StartTime.AddMinutes((index + 1) * MultipleInMinutes).ToString("hh:mm tt")}"
                };

            for (int i = 0; i < BarCount + 1; i++)
            {
                DateTime ProgressedTime = StartTime.AddMinutes(i * MultipleInMinutes);
                LiveStatusTimes.Add(ProgressedTime);
                StatusLabels.Add(GetReadableTimeSpan(i, ProgressedTime));
            }
        }

        private void SwitchToHourProgression()
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (ISeriesView sv in StatusSeriesCollection)
                    {
                        int TotalFirstHour = 0;

                        for (int i = 0; i < sv.Values.Count; i++)
                        {
                            if (sv.Values[i] is ObservableValue ov)
                            {
                                TotalFirstHour += (int)ov.Value;
                                ov.Value = 0;
                            }
                        }

                        if (sv.Values[0] is ObservableValue ovv)
                        {
                            ovv.Value = TotalFirstHour;
                        }
                    }
                });

                MultipleInMinutes = 120;
                SetStatusTimeProgression(6, LiveStatusTimes[0]);

                LiveStatusDescription = "Currently showing last 12 hours of failed pings";
            }
        }

        private int GetLiveStatusTimeIndex(DateTime Time)
        {
            int index = 0;

            if (Time > LiveStatusTimes[LiveStatusTimes.Count - 1])
            {
                if (!ProgressionDrop)
                {
                    SwitchToHourProgression();
                    ProgressionDrop = true;
                }
                else
                {
                    /*drop index 0 data and add new column. Update time info and return latest index.
                     *
                     * - Itirate through StatusSeriesCollection  
                     *      - drop oldest column in series             : ISeriesView.Values.RemoveAt(0)
                     *      - add new column to series                 : ISeriesView.Value.Add(0)
                     * - Update time info
                     *      - Remove oldest Time                       : LiveStatusTimes.RemoveAt(0)
                     *      - Add new time from most recent time value : LiveStatusTimes.Add(LiveStatusTime[LiveStatusTimes.Count-1].AddHours(1))
                     * - return newest index value                     : return LiveStatusTimes.Count-1
                     */

                    foreach (var series in StatusSeriesCollection)
                    {
                        if (series is ISeriesView sv)
                        {
                            if (Application.Current != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    sv.Values.RemoveAt(0);
                                    sv.Values.Add(new ObservableValue(0));
                                });
                            }
                        }
                    }

                    //possible problem with adding more times to LiveStatusTimes than there should be? (got an exception and debug showed 11 entries in LiveStatusTimes. Bad count check?)
                    //might be a race condition...

                    LiveStatusTimes.RemoveAt(0);
                    LiveStatusTimes.Add(LiveStatusTimes[LiveStatusTimes.Count - 1].AddMinutes(MultipleInMinutes));

                    StatusLabels.RemoveAt(0);
                    StatusLabels.Add($"{LiveStatusTimes[0].AddMinutes((LiveStatusTimes.Count - 1) * MultipleInMinutes).ToString("hh:mm tt")} - {LiveStatusTimes[0].AddMinutes((LiveStatusTimes.Count) * MultipleInMinutes).ToString("hh:mm tt")}");

                    return LiveStatusTimes.Count - 1;
                }
            }
            else
            {
                //starting at 1 to ignore the first entry (first entry will always be less than the current time)
                for (int i = 1; i < LiveStatusTimes.Count; i++)
                {
                    if (Time < LiveStatusTimes[i])
                    {
                        return index;
                    }
                    else
                    {
                        index += 1;
                    }
                }
            }

            return index;
        }

        public void UpdateLiveGraphData(string PingLabel, int NewStatusValue, int NewLatencyValue, DateTime Time)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ISeriesView latencyLine = LatencySeriesCollection.Where(x => x.Title == PingLabel).FirstOrDefault();
                    ISeriesView statusColumns = StatusSeriesCollection.Where(x => x.Title == PingLabel).FirstOrDefault();

                    latencyLine.Values.Add(new LiveLatencyModel { Value = NewLatencyValue, DateTime = Time });

                    int StatusIndex = GetLiveStatusTimeIndex(Time);

                    if (StatusIndex <= statusColumns.Values.Count - 1 && statusColumns.Values[StatusIndex] is ObservableValue ov)
                    {
                        if (ov.Value > 9 && YAxisStep != double.NaN)
                        {
                            YAxisStep = double.NaN;
                        }

                        ov.Value += NewStatusValue;
                    }

                    if (latencyLine.Values.Count > 51)
                    {
                        latencyLine.Values.RemoveAt(0);
                    }
                });
            }
        }
    }
}

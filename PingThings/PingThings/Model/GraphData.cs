using System;
using System.Collections.ObjectModel;

namespace PingThings.Model
{
    public class GraphData
    {
        enum CloneType { Status, Latency, TimeStamps }
        public GraphData() { }
        public GraphData(GraphData Original)
        {
            void Clone(object CloneFrom, CloneType cloneType)
            {
                switch (cloneType)
                {
                    case CloneType.Status:
                        {
                            if (CloneFrom is ObservableCollection<int> cf)
                            {
                                foreach (int status in cf)
                                {
                                    StatusValues.Add(status);
                                }
                            }
                            break;
                        }
                    case CloneType.Latency:
                        {
                            if (CloneFrom is ObservableCollection<int> cf)
                            {
                                foreach (int latency in cf)
                                {
                                    LatencyValues.Add(latency);
                                }
                            }
                            break;
                        }
                    case CloneType.TimeStamps:
                        {
                            if (CloneFrom is ObservableCollection<DateTime> cf)
                            {
                                foreach (DateTime dt in cf)
                                {
                                    TimeStamps.Add(new DateTime(dt.Ticks));
                                }
                            }
                            break;
                        }
                }
            }

            GroupName = Original.GroupName;
            Label = Original.Label;
            FromHost = Original.FromHost;
            ToHost = Original.ToHost;
            Clone(Original.TimeStamps, CloneType.TimeStamps);
            Clone(Original.StatusValues, CloneType.Status);
            Clone(Original.LatencyValues, CloneType.Latency);
        }

        public string GroupName { get; set; }
        public string Label { get; set; }
        public string FromHost { get; set; }
        public string ToHost { get; set; }
        public ObservableCollection<DateTime> TimeStamps { get; set; } = new ObservableCollection<DateTime>();
        public ObservableCollection<int> StatusValues { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int> LatencyValues { get; set; } = new ObservableCollection<int>();
    }
}

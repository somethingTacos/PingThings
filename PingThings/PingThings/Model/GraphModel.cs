using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PingThings.Model
{
    public class GraphModel { }

    public class GraphCollection : INotifyPropertyChanged
    {
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();

        private int _GraphColumns;
        public int GraphColumns
        {
            get => _GraphColumns;
            set
            {
                if (_GraphColumns != value)
                {
                    _GraphColumns = value;
                    RaisePropertyChanged("GraphColumns");
                }
            }
        }

        public enum GraphType { Status, Latency }

        private GraphType _DisplayType;
        public GraphType DisplayType
        {
            get => _DisplayType;
            set
            {
                if (_DisplayType != value)
                {
                    _DisplayType = value;
                    RaisePropertyChanged("DisplayType");
                }
            }
        }

        private int _CompressionRateIndex;
        public int CompressionRateIndex
        {
            get => _CompressionRateIndex;
            set
            {
                if (_CompressionRateIndex != value)
                {
                    _CompressionRateIndex = value;
                    RaisePropertyChanged("CompressionRateIndex");
                    OnCompressionRateIndexChanged?.Invoke();
                }
            }
        }
        private int _CompressionValue;
        public int CompressionValue
        {
            get => _CompressionValue;
            set
            {
                if (_CompressionValue != value)
                {
                    _CompressionValue = value;
                    RaisePropertyChanged("CompressionValue");
                }
            }
        }

        public delegate void CompressionRateIndexChanged();

        public event CompressionRateIndexChanged OnCompressionRateIndexChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Graph : INotifyPropertyChanged
    {

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

        private string _YTitle;
        public string YTitle
        {
            get => _YTitle;
            set
            {
                if (_YTitle != value)
                {
                    _YTitle = value;
                    RaisePropertyChanged("YTitle");
                }
            }
        }

        private string _XTitle;
        public string XTitle
        {
            get => _XTitle;
            set
            {
                if (_XTitle != value)
                {
                    _XTitle = value;
                    RaisePropertyChanged("XTitle");
                }
            }
        }

        public List<GraphData> GraphDataList = new List<GraphData>();
        public ObservableCollection<string> Labels { get; set; } = new ObservableCollection<string>();
        public SeriesCollection DataCollection { get; set; } = new SeriesCollection();

        private Func<double, string> _YFormatter;
        public Func<double, string> YFormatter
        {
            get => _YFormatter;
            set
            {
                if (_YFormatter != value)
                {
                    _YFormatter = value;
                    RaisePropertyChanged("YFormatter");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

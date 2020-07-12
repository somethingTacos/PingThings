using PingThings.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace PingThings.Model
{
    public class PingModel { }

    public class Pings : INotifyPropertyChanged
    {
        public event EventHandler SelectedIndexChanged;
        public void OnSelectedIndexChanged()
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private int _LastSelectedIndex;
        public int LastSelectedIndex
        {
            get => _LastSelectedIndex;
            set
            {
                if (_LastSelectedIndex != value)
                {
                    _LastSelectedIndex = value;
                    RaisePropertyChanged("LastSelectedIndex");
                }
            }
        }

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                if (_SelectedIndex != value)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                    OnSelectedIndexChanged();
                }
            }
        }

        public ObservableCollection<PingGroup> Things { get; set; } = new ObservableCollection<PingGroup>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class PingThing : INotifyPropertyChanged
    {
        public string GroupName { get; set; } = String.Empty;
        private Ping pinger { get; set; } = new Ping();
        public string Label { get; set; } = String.Empty;
        public string Host { get; set; } = String.Empty;

        private string _CurrentStatus;
        public string CurrentStatus
        {
            get => _CurrentStatus;
            set
            {
                if (_CurrentStatus != value)
                {
                    _CurrentStatus = value;
                    RaisePropertyChanged("CurrentStatus");
                }
            }
        }

        private int _TotalSent;
        public int TotalSent
        {
            get => _TotalSent;
            private set
            {
                if (_TotalSent != value)
                {
                    _TotalSent = value;
                    RaisePropertyChanged("TotalSent");
                }
            }
        }

        private int _TotalReplies;
        public int TotalReplies
        {
            get => _TotalReplies;
            private set
            {
                if (_TotalReplies != value)
                {
                    _TotalReplies = value;
                    RaisePropertyChanged("TotalReplies");
                }
            }
        }

        private int _TotalFailed;
        public int TotalFailed
        {
            get => _TotalFailed;
            private set
            {
                if (_TotalFailed != value)
                {
                    _TotalFailed = value;
                    RaisePropertyChanged("TotalFailed");
                }
            }
        }

        public (int, int) SendPing()
        {
            try
            {
                TotalSent += 1;
                PingReply reply = pinger.Send(Host, 5000);

                if (reply.Status == IPStatus.Success)
                {
                    TotalReplies += 1;
                    GraphLogger.WriteEntry(GroupName, Label, Host, "0", reply.RoundtripTime.ToString());
                    return (0, (int)reply.RoundtripTime);
                }
                else
                {
                    TotalFailed += 1;
                    GraphLogger.WriteEntry(GroupName, Label, Host, "1", "0");
                    return (1, 0);
                }

            }
            catch (Exception)
            {
                TotalFailed += 1;
                GraphLogger.WriteEntry(GroupName, Label, Host, "1", "0");
                return (1, 0);
            }
        }

        public bool GetHostStatus()
        {
            try
            {
                PingReply reply = pinger.Send(Host, 5000);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

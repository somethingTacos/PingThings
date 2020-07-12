using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PingThings.Model
{
    public class SelectedPingGroupModel { }

    public class SelectedPingGroup : INotifyPropertyChanged
    {
        ObservableCollection<PingThing> Pings { get; set; } = new ObservableCollection<PingThing>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

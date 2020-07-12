using MaterialDesignThemes.Wpf;
using PingThings.Helpers;
using PingThings.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PingThings.CustomControls.LazyDialogs
{
    /// <summary>
    /// Interaction logic for AddPingDialog.xaml
    /// </summary>
    public partial class AddPingDialog : UserControl
    {
        private Pings _pings;

        public SnackbarMessageQueue NotificationQueue { get; set; }
        public MyICommand AddPingEnterKeyCommand { get; set; }
        public AddPingDialog(Pings pings)
        {
            InitializeComponent();
            AddPingEnterKeyCommand = new MyICommand(OnAddPingEnterKeyCommand);
            NotificationQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            _pings = pings;
        }

        public void OnAddPingEnterKeyCommand(object parameter)
        {
            AddPingButton_Click(this, null);
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            int interval = 0;
            string Name = GroupName_Box.Text;

            (bool, string) validateGroupInfo(string possibleInterval, string GroupName) =>
                (String.IsNullOrEmpty(GroupName), int.TryParse(possibleInterval, out interval)) switch
                {
                    (true, _) => (false, "Group Name cannot be empty"),
                    (_, false) => (false, "Interval must be a number in seconds and cannot be empty"),
                    (_, true) => (true, "")
                };

            (bool, string) ValidInfo = validateGroupInfo(GroupInterval_Box.Text, Name);

            if (ValidInfo.Item1)
            {
                if (PingList.Items.Count > 0)
                {
                    PingGroup newGroup = new PingGroup();
                    newGroup.GroupName = Name;

                    foreach (PingThing pt in PingList.Items)
                    {
                        pt.GroupName = Name;
                        newGroup.Pings.Add(pt);
                    }

                    _pings.Things.Add(newGroup);
                    _pings.SelectedIndex = _pings.Things.Count() - 1;
                    newGroup.StartPinging(interval);

                    DialogHost.CloseDialogCommand.Execute(null, this);
                }
                else
                {
                    NotificationQueue.Enqueue("No pings in group. Please add at least 1 ping to add group.");
                }
            }
            else
            {
                NotificationQueue.Enqueue(ValidInfo.Item2);
            }
        }

        private void AddPingButton_Click(object sender, RoutedEventArgs e)
        {
            (bool, string) ValidatePingInfo(string Label, string Host) =>
            (String.IsNullOrEmpty(Label), String.IsNullOrEmpty(Host), Label.Contains(']')) switch
            {
                (_, _, true) => (false, "Label contains reserved character ']'"),
                (_, true, _) => (false, "Host cannot be empty"),
                (true, _, _) => (true, Host),
                (_, _, _) => (true, "")
            };

            (bool, string) ValidInfo = ValidatePingInfo(Label_Box.Text, Host_Box.Text);

            if (ValidInfo.Item1)
            {
                if (Host_Box.Text == ValidInfo.Item2) { Label_Box.Text = Host_Box.Text; }

                PingThing newPing = new PingThing { Label = Label_Box.Text, Host = Host_Box.Text, CurrentStatus = "Checking..." };

                Task.Run(() =>
                {
                    if (newPing.GetHostStatus())
                    {
                        newPing.CurrentStatus = "Responding";
                    }
                    else
                    {
                        newPing.CurrentStatus = "Not Repsonding";
                        NotificationQueue.Enqueue("One or more hosts are not responding. Be sure all information is correct before proceeding.");
                    }
                });

                PingList.Items.Add(newPing);
                Label_Box.Text = "";
                Label_Box.Focus();
                Host_Box.Text = "";
            }
            else
            {
                NotificationQueue.Enqueue(ValidInfo.Item2);
            }
        }

        private void RemovePingButton_Click(object sender, RoutedEventArgs e)
        {


            if (PingList.SelectedItem is PingThing pt)
            {
                if (pt.CurrentStatus == "NOT Responding")
                {
                    int Count = 0;

                    foreach (var item in PingList.Items)
                    {
                        if (item is PingThing thing)
                        {
                            if (thing.CurrentStatus == "NOT Responding")
                            {
                                Count += 1;
                            }
                        }
                    }
                }
            }


            PingList.Items.Remove(PingList.SelectedItem);
        }

        private void GroupName_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            char[] AllowedChars = { '.', '_', ' ', '-' };

            foreach (char c in GroupName_Box.Text)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || AllowedChars.Contains(c))
                {
                    sb.Append(c);
                }
            }

            GroupName_Box.Text = sb.ToString();
            GroupName_Box.CaretIndex = sb.Length;
        }
    }
}

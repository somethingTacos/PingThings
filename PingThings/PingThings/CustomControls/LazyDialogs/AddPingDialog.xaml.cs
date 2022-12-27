using MaterialDesignThemes.Wpf;
using PingThings.Helpers;
using PingThings.Model;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

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

        (bool, string) ValidatePingInfo(string Label, string Host) =>
            (String.IsNullOrEmpty(Label), String.IsNullOrEmpty(Host), Label.Contains(']')) switch
            {
                (_, _, true) => (false, "Label contains reserved character ']'"),
                (_, true, _) => (false, "Host cannot be empty"),
                (true, _, _) => (true, Host),
                (_, _, _) => (true, "")
            };

        private (bool, string) validateGroupInfo(string possibleInterval, string GroupName, out int interval) =>
                (String.IsNullOrEmpty(GroupName), int.TryParse(possibleInterval, out interval),
                _pings.Things.Where(x => x.GroupName == Name).Count() > 0) switch
                {
                    (true, _, _) => (false, "Group Name cannot be empty"),
                    (_, false, _) => (false, "Interval must be a number in seconds and cannot be empty"),
                    (_, _, true) => (false, "Group name already exists"),
                    (_, true, _) => (true, "")
                };

        public void OnAddPingEnterKeyCommand(object parameter)
        {
            AddPingButton_Click(this, null);
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            int interval = 0;
            string Name = GroupName_Box.Text;

            (bool, string) ValidInfo = validateGroupInfo(GroupInterval_Box.Text, Name, out interval);

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

        private void AddPing(string label, string host)
        {
            (bool, string) ValidInfo = ValidatePingInfo(label, host);

            if (ValidInfo.Item1)
            {
                if (host == ValidInfo.Item2) { label = host; }

                PingThing newPing = new PingThing { Label = label, Host = host, CurrentStatus = "Checking..." };

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
            }
            else
            {
                NotificationQueue.Enqueue(ValidInfo.Item2);
            }
        }

        private void AddPingButton_Click(object sender, RoutedEventArgs e)
        {
            AddPing(Label_Box.Text, Host_Box.Text);

            Label_Box.Text = "";
            Label_Box.Focus();
            Host_Box.Text = "";
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

        private string EscapeText(string text)
        {
            StringBuilder sb = new StringBuilder();
            char[] AllowedChars = { '.', '_', ' ', '-' };

            foreach (char c in text)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || AllowedChars.Contains(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private void GroupName_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string escapedText = EscapeText(GroupName_Box.Text);
            GroupName_Box.Text = escapedText;
            GroupName_Box.CaretIndex = escapedText.Length;
        }

        private void Label_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            string escapedText = EscapeText(Label_Box.Text);
            Label_Box.Text = escapedText;
            Label_Box.CaretIndex = escapedText.Length;
        }

        private GroupData LoadData(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<GroupData>(json);
            }
            catch(Exception ex)
            {
                NotificationQueue.Enqueue(ex.Message);
            }

            return null;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    var file = new FileInfo(files[0]);
                    
                    if(file.Exists && file.Extension == ".json")
                    {
                        var groupData = LoadData(File.ReadAllText(file.FullName));

                        if (groupData == null) return;

                        GroupName_Box.Text = EscapeText(groupData.GroupName);

                        foreach(var ping in groupData.Pings)
                        {
                            AddPing(ping.Label, ping.Host);
                        }
                    }
                }
                catch(Exception ex)
                {
                    NotificationQueue.Enqueue(ex.Message);
                }
            }
        }
    }
}

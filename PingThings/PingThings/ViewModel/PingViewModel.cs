using MaterialDesignThemes.Wpf;
using PingThings.CustomControls.LazyDialogs;
using PingThings.Helpers;
using PingThings.Model;
using System;

namespace PingThings.ViewModel
{
    public class PingViewModel
    {
        public Pings pings { get; set; }
        public NavigationViewModel _navigationViewModel { get; set; }
        public NavigationViewModel SelectedPingGroupNavigation { get; set; }
        public MyICommand AddPingCommand { get; set; }
        public MyICommand ShowGraphViewCommand { get; set; }
        public MyICommand ClosePingGroupCommand { get; set; }
        public SnackbarMessageQueue MainQueue { get; set; }

        public PingViewModel(NavigationViewModel navigationViewModel, Pings _pings = null)
        {
            _navigationViewModel = navigationViewModel;
            AddPingCommand = new MyICommand(OnAddPingCommand);
            ShowGraphViewCommand = new MyICommand(OnShowGraphViewCommand);
            ClosePingGroupCommand = new MyICommand(OnClosePingGroupCommand);

            MainQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(10));

            NavigationViewModel spgn = new NavigationViewModel();
            SelectedPingGroupNavigation = spgn;
            SelectedPingGroupNavigation.SelectedViewModel = new NoSelectionViewModel(SelectedPingGroupNavigation);


            if (_pings != null)
            {
                pings = _pings;
            }
            else
            {
                InitPings();
            }

            pings.SelectedIndexChanged += GroupIndexChanged;

            if (pings.LastSelectedIndex != -1)
            {
                pings.SelectedIndex = pings.LastSelectedIndex;
            }
        }

        public void InitPings()
        {
            Pings tempPings = new Pings();
            tempPings.SelectedIndex = -1;
            tempPings.LastSelectedIndex = -1;

            pings = tempPings;
        }

        public void GroupIndexChanged(object sender, EventArgs e)
        {
            if (pings.SelectedIndex > -1 && pings.SelectedIndex <= pings.Things.Count - 1)
            {
                pings.LastSelectedIndex = pings.SelectedIndex;
                SelectedPingGroupNavigation.SelectedViewModel = new SelectedPingGroupViewModel(SelectedPingGroupNavigation, pings.Things[pings.SelectedIndex]);
            }
        }

        public void OnAddPingCommand(object parameter)
        {
            AddPingDialog pd = new AddPingDialog(pings);

            DialogHost.Show(pd); //, ClosingEventHandler);
        }

        //TODO Move data handling stuff from ping dialog to a dialoghost closing event.
        //private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        //{

        //}

        public void OnShowGraphViewCommand(object parameter)
        {
            //HACK this is a dumb fix for an axis re-load issue that I need to hunt down. Something about the axis not having a valid range when it is re-loaded
            //LiveCharts.Helpers.LiveChartsException: 'One axis has an invalid range, it is or it is tends to zero, please ensure your axis has a valid range'

            bool CanChangeView = true;

            foreach(PingGroup group in pings.Things)
            {
                foreach(PingThing thing in group.Pings)
                {
                    if(thing.TotalSent < 2)
                    {
                        CanChangeView = false;
                    }
                }
            }

            if (CanChangeView)
            {
                pings.SelectedIndexChanged -= GroupIndexChanged;
                pings.SelectedIndex = -1;
                _navigationViewModel.SelectedViewModel = new GraphViewModel(_navigationViewModel, pings);
            }
            else
            {
                MainQueue.Enqueue("Sorry, Can't load the graph view until all ping groups have sent at least 2 pings. This is a bug work around.",
                                  "Oh, ok :(", 
                                  () => { });
            }
        }

        public void OnClosePingGroupCommand(object parameter)
        {
            if (parameter is PingGroup pg)
            {
                pg.StopPinging();
                pings.Things.Remove(pg);

                if (pings.Things.Count == 0)
                {
                    SelectedPingGroupNavigation.SelectedViewModel = new NoSelectionViewModel(SelectedPingGroupNavigation);
                }
                else
                {
                    SelectedPingGroupNavigation.SelectedViewModel = new SelectedPingGroupViewModel(SelectedPingGroupNavigation, pings.Things[0]);
                }

            }
        }
    }
}

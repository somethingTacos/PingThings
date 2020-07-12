using PingThings.Model;
using System;

namespace PingThings.ViewModel
{
    public class SelectedPingGroupViewModel
    {
        public Func<double, string> LatencyFormatter { get; set; } = y => $"{y}ms";
        public Func<double, string> StatusFormatter { get; set; } = y => ((int)y).ToString();

        public Func<double, string> DateTimeFormatter { get; set; } = dt => new DateTime((long)dt).ToString("hh:mm:ss tt");

        private NavigationViewModel _navigationViewModel { get; set; }

        public PingGroup CurrentGroup { get; set; }

        public SelectedPingGroupViewModel(NavigationViewModel navigationViewModel, PingGroup group)
        {
            _navigationViewModel = navigationViewModel;
            CurrentGroup = group;
        }
    }
}

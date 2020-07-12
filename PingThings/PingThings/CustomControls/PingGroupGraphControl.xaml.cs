using LiveCharts;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PingThings.CustomControls
{
    /// <summary>
    /// Interaction logic for PingGroupGraphControl.xaml
    /// </summary>
    public partial class PingGroupGraphControl : UserControl
    {
        public PingGroupGraphControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(PingGroupGraphControl), new PropertyMetadata(String.Empty));
        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public static readonly DependencyProperty DataCollectionProperty =
            DependencyProperty.Register("DataCollection", typeof(SeriesCollection), typeof(PingGroupGraphControl), new PropertyMetadata(null));
        public SeriesCollection DataCollection
        {
            get => (SeriesCollection)GetValue(DataCollectionProperty);
            set => SetValue(DataCollectionProperty, value);
        }

        public static readonly DependencyProperty YTitleProperty =
            DependencyProperty.Register("YTitle", typeof(string), typeof(PingGroupGraphControl), new PropertyMetadata(String.Empty));
        public string YTitle
        {
            get => (string)GetValue(YTitleProperty);
            set => SetValue(YTitleProperty, value);
        }

        public static readonly DependencyProperty XTitleProperty =
            DependencyProperty.Register("XTitle", typeof(string), typeof(PingGroupGraphControl), new PropertyMetadata(String.Empty));
        public string XTitle
        {
            get => (string)GetValue(XTitleProperty);
            set => SetValue(XTitleProperty, value);
        }

        public static readonly DependencyProperty LabelsProperty =
            DependencyProperty.Register("Labels", typeof(ObservableCollection<string>), typeof(PingGroupGraphControl), new PropertyMetadata(null));
        public ObservableCollection<string> Labels
        {
            get => (ObservableCollection<string>)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(PingGroupGraphControl), new UIPropertyMetadata(null));

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public static readonly DependencyProperty YFormatterProperty =
            DependencyProperty.Register("YFormatter", typeof(Func<double, string>), typeof(PingGroupGraphControl));

        public Func<double, string> YFormatter
        {
            get => (Func<double, string>)GetValue(YFormatterProperty);
            set => SetValue(YFormatterProperty, value);
        }
    }
}

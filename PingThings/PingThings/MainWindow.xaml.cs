using LiveCharts;
using LiveCharts.Configurations;
using PingThings.Model;
using PingThings.ViewModel;
using System.Windows;

namespace PingThings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //add LiveLatencyModel mapper
            var mapper = Mappers.Xy<LiveLatencyModel>()
                .X(model => model.DateTime.Ticks)
                .Y(model => model.Value);

            Charting.For<LiveLatencyModel>(mapper);

            //set viewmodel navigation
            var viewmodel = new NavigationViewModel();
            viewmodel.SelectedViewModel = new PingViewModel(viewmodel);
            this.DataContext = viewmodel;
        }
    }
}

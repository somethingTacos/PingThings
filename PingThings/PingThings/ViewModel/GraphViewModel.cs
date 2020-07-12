using LiveCharts.Wpf;
using PingThings.Helpers;
using PingThings.Helpers.AsyncCommand;
using PingThings.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PingThings.ViewModel
{
    public class GraphViewModel : IFileDragDropTarget
    {
        public NavigationViewModel _navigationViewModel { get; set; }
        public Pings _pings { get; set; }

        public GraphCollection graphCollection { get; set; }

        public GraphHelper graphHelper { get; set; }
        public MyICommand BackCommand { get; set; }
        public MyICommand CloseGraphCommand { get; set; }
        public MyICommand CompressionValueBoxEnter { get; set; }
        public MyICommand OpenDataFolderCommand { get; set; }
        public AwaitableDelegateCommand ToggleDisplayTypeCommand { get; set; }
        public GraphViewModel(NavigationViewModel navigationViewModel, Pings pings)
        {
            _navigationViewModel = navigationViewModel;
            BackCommand = new MyICommand(OnBackCommand);
            CloseGraphCommand = new MyICommand(OnCloseGraphCommand);
            CompressionValueBoxEnter = new MyICommand(OnCompressionValueBoxEnter);
            ToggleDisplayTypeCommand = new AwaitableDelegateCommand(OnToggleDisplayTypeCommand);
            OpenDataFolderCommand = new MyICommand(OnOpenDataFolderCommand);
            _pings = pings;
            InitGraph();
            graphHelper = new GraphHelper();
        }

        private void InitGraph()
        {
            GraphCollection tempCollection = new GraphCollection();
            tempCollection.CompressionRateIndex = 1;
            tempCollection.CompressionValue = 1;
            tempCollection.GraphColumns = 1;
            graphCollection = tempCollection;
            graphCollection.OnCompressionRateIndexChanged += OnCompressionRateIndexChanged;
        }

        public async void OnCompressionRateIndexChanged()
        {
            if (graphCollection.Graphs.Count > 0)
            {
                foreach (Graph graph in graphCollection.Graphs)
                {
                    Graph TempGraph = await graphHelper.GetCompressedGraphData(graph.GraphDataList, graphCollection.CompressionRateIndex, graphCollection.CompressionValue, graphCollection.DisplayType);
                    SetGraphData(TempGraph, graph);
                }
            }
        }

        public void OnCompressionValueBoxEnter(object parameter)
        {
            OnCompressionRateIndexChanged();
        }

        public void OnBackCommand(object parameter)
        {
            _navigationViewModel.SelectedViewModel = new PingViewModel(_navigationViewModel, _pings);
        }

        private void SetGraphData(Graph NewGraphData, Graph GraphToUpdate)
        {
            GraphToUpdate.Labels.Clear();
            GraphToUpdate.DataCollection.Clear();

            foreach (string label in NewGraphData.Labels)
            {
                GraphToUpdate.Labels.Add(label);
            }

            foreach (LineSeries ls in NewGraphData.DataCollection)
            {
                GraphToUpdate.DataCollection.Add(ls);
            }
        }

        private List<string> GetLogFilePaths(string[] filepaths)
        {
            List<string> LogFiles = new List<string>();

            void CheckFilePath(string filepath)
            {
                if (filepath.EndsWith(".log"))
                {
                    if (File.Exists(filepath))
                    {
                        LogFiles.Add(filepath);
                    }
                }
            }

            foreach (string path in filepaths)
            {
                if (Directory.Exists(path))
                {
                    List<string> DirFiles = Directory.GetFiles(path).ToList();
                    foreach (string DirFile in DirFiles)
                    {
                        CheckFilePath(DirFile);
                    }
                }
                else
                {
                    CheckFilePath(path);
                }
            }

            return LogFiles;
        }

        private async Task LoadFileData(string[] filepaths)
        {
            List<string> LogFilePaths = GetLogFilePaths(filepaths);
            List<GraphData> NewGraphData = new List<GraphData>();

            if (LogFilePaths.Count > 0)
            {

                List<string> UniquePaths = LogFilePaths.Select(x => x.Split("__")[0]).Distinct().ToList();

                foreach (string path in UniquePaths)
                {
                    string[] ContinousLogPaths = LogFilePaths.Where(x => x.StartsWith(path)).ToArray();

                    NewGraphData.Add(await graphHelper.LoadAndParseData(ContinousLogPaths));
                }

                List<string> GroupNames = NewGraphData.Select(x => x.GroupName).Distinct().ToList();

                if(GroupNames.Count > 1)
                {
                    graphCollection.GraphColumns = 2;
                }

                foreach (string Name in GroupNames)
                {
                    Graph NewGraph = new Graph();
                    NewGraph.GraphDataList = NewGraphData.Where(x => x.GroupName == Name).ToList();

                    if (NewGraph.GraphDataList.Count > 0)
                    {
                        NewGraph.GroupName = NewGraph.GraphDataList[0].GroupName;
                        NewGraph.XTitle = "Time";
                        NewGraph.YTitle = graphCollection.DisplayType.ToString();

                        Graph CompressedGraph = await graphHelper.GetCompressedGraphData(NewGraph.GraphDataList, graphCollection.CompressionRateIndex, graphCollection.CompressionValue, graphCollection.DisplayType);
                        SetGraphData(CompressedGraph, NewGraph);
                        graphCollection.Graphs.Add(NewGraph);
                    }
                }
            }
        }

        public async Task OnFileDrop(string[] filepaths)
        {
            await LoadFileData(filepaths);
        }

        public void OnCloseGraphCommand(object parameter)
        {
            if (parameter is Graph graph)
            {
                graphCollection.Graphs.Remove(graph);

                if (graphCollection.Graphs.Count <= 1 && graphCollection.GraphColumns > 1)
                {
                    graphCollection.GraphColumns = 1;
                }
            }
        }

        public async Task OnToggleDisplayTypeCommand()
        {
            switch (graphCollection.DisplayType)
            {
                case GraphCollection.GraphType.Status:
                    {
                        graphCollection.DisplayType = GraphCollection.GraphType.Latency;
                        foreach (Graph graph in graphCollection.Graphs)
                        {
                            graph.YFormatter = x => $"{x.ToString()}ms";
                            graph.YTitle = "Latency";
                            Graph blah = await graphHelper.GetCompressedGraphData(graph.GraphDataList, graphCollection.CompressionRateIndex, graphCollection.CompressionValue, graphCollection.DisplayType);
                            SetGraphData(blah, graph);
                        }
                        break;
                    }
                case GraphCollection.GraphType.Latency:
                    {
                        graphCollection.DisplayType = GraphCollection.GraphType.Status;
                        foreach (Graph graph in graphCollection.Graphs)
                        {
                            graph.YFormatter = x => x.ToString();
                            graph.YTitle = "Status";
                            Graph blah = await graphHelper.GetCompressedGraphData(graph.GraphDataList, graphCollection.CompressionRateIndex, graphCollection.CompressionValue, graphCollection.DisplayType);
                            SetGraphData(blah, graph);
                        }
                        break;
                    }
            }
        }

        public void OnOpenDataFolderCommand(object parameter)
        {
            if (Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\PingThings"))
            {
                ProcessStartInfo startinfo = new ProcessStartInfo();
                startinfo.FileName = "explorer.exe";
                startinfo.Arguments = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\PingThings";

                Process.Start(startinfo);
            }
            else
            {
                MessageBox.Show("The Data directory doesn't exist. This is most likely due to no ping groups being created.", "No Data Folder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}

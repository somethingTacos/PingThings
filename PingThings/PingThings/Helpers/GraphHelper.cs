using LiveCharts;
using LiveCharts.Wpf;
using PingThings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PingThings.Helpers
{
    /// <summary>
    /// A helper class to manager data being loaded into a graph.
    /// </summary>
    public class GraphHelper
    {
        public async Task<GraphData> LoadAndParseData(string[] filepaths)
        {
            GraphData TempGraphData = new GraphData();
            //load data from file, and have the graph logger parse it into a GraphData object.
            await Task.Run(() =>
            {
                bool IsFirst = true;

                try
                {
                    filepaths = filepaths.OrderBy(x => DateTime.Parse(x.Split("__")[1].Replace(".log", ""))).ToArray();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                foreach (string path in filepaths)
                {
                    List<string> fileData = new List<string>();
                    GraphData graphData = new GraphData();

                    using (StreamReader sr = new StreamReader(path))
                    {
                        do
                        {
                            fileData.Add(sr.ReadLine());
                        }
                        while (!sr.EndOfStream);
                    }

                    graphData = GraphLogger.ParseLog(fileData); //<-- this is the reason this task is awaited, even though this only takes about 1ms per 1000 entries. Can't know if someone is going to do lucrative amounts with this. Say 500K entries...
                                                                //    Will probably want to await compression as well, since it's going to be more expensive. Could setup an indeterminate progress bar to show load or compression is happening.
                                                                //    Alternatively, the data could be streamed to the graph, so the UI thread doesn't halt when having to plot a ton of data points in the event the compression value is really low.

                    if (IsFirst)
                    {
                        TempGraphData.GroupName = graphData.GroupName;
                        TempGraphData.Label = graphData.Label;
                        TempGraphData.FromHost = graphData.FromHost;
                        TempGraphData.ToHost = graphData.ToHost;
                        IsFirst = false;
                    }
                    for (int i = 0; i < graphData.StatusValues.Count; i++)
                    {
                        TempGraphData.TimeStamps.Add(graphData.TimeStamps[i]);
                        TempGraphData.StatusValues.Add(graphData.StatusValues[i]);
                        TempGraphData.LatencyValues.Add(graphData.LatencyValues[i]);
                    }

                }

            });

            return TempGraphData;
        }
        private async Task<List<GraphData>> CompressGraphData(List<GraphData> GraphDataList, int CompressionRateIndex, int CompressionValue)
        {
            List<GraphData> CompressedGraphData = new List<GraphData>();

            await Task.Run(() =>
            {
                foreach (GraphData gd in GraphDataList)
                {
                    bool IsCompressing = true; //used to continue while loop until compression is done.
                    long BadEndTimeTicks = 350281194710000000; //containt the discard ticks value in GetEndTime
                    int EntryIndex = 0; //Index where the current compressed data should be stored.

                    GraphData CompressedData = new GraphData(); //compressed form of current graphdata
                    GraphData TempGraphData = new GraphData(gd); //temp copy of GraphData that can have items removed.

                    //Gets the end datetime value. Anything between index 0 and the endtime are compressed to a single value with the index 0 timestamp
                    DateTime GetEndTime(DateTime dt) =>
                        CompressionRateIndex switch
                        {
                            0 => dt.AddMinutes(CompressionValue),
                            1 => dt.AddHours(CompressionValue),
                            2 => dt.AddDays(CompressionValue),
                            3 => dt.AddDays(CompressionValue * 7),
                            4 => dt.AddMonths(CompressionValue),
                            5 => dt.AddYears(CompressionValue),
                            _ => DateTime.Parse("1/1/1111 1:11:11")
                        };

                    //do until this graphdata is compressed
                    do
                    {
                        if (TempGraphData.TimeStamps.Count != 0)
                        {
                            bool PassedEndTime = false; //used to break while loop to find ending time index
                            var EndTime = GetEndTime(TempGraphData.TimeStamps[0]); //gets the end time for value compression
                            List<int> LatencyValueList = new List<int>();

                            //make sure GetEndTime didn't reach the discard. This should never happen, since the CompressionRateIndex is bound to the ComboBox in the GraphView.
                            if (EndTime.Ticks != BadEndTimeTicks)
                            {
                                int index = 0; //used to store the last compressiable index for this value range.
                                               //do until the end time is passed and set the index to the last value within the time span
                                do
                                {
                                    if (index < TempGraphData.TimeStamps.Count)
                                    {
                                        if (TempGraphData.TimeStamps[index].Ticks > EndTime.Ticks)
                                        {
                                            PassedEndTime = true;
                                            index -= 1;
                                        }
                                        else
                                        {
                                            index += 1;
                                        }
                                    }
                                    else
                                    {
                                        PassedEndTime = true;
                                    }
                                }
                                while (!PassedEndTime);

                                //iterate through the index range and compress the values into the CompressedData EntryIndex for the graphdata
                                for (int i = 0; i < index; i++)
                                {
                                    //The first index value should set the non-enumerable values as well; otherwise, just compress the data
                                    if (i == 0)
                                    {
                                        CompressedData.Label = TempGraphData.Label;
                                        CompressedData.ToHost = TempGraphData.ToHost;
                                        CompressedData.FromHost = TempGraphData.FromHost;
                                        CompressedData.TimeStamps.Add(TempGraphData.TimeStamps[0]);
                                        CompressedData.StatusValues.Add(TempGraphData.StatusValues[0]);
                                        CompressedData.LatencyValues.Add(TempGraphData.LatencyValues[0]);
                                    }
                                    else
                                    {
                                        CompressedData.StatusValues[EntryIndex] += TempGraphData.StatusValues[0];
                                        LatencyValueList.Add(TempGraphData.LatencyValues[0]);
                                    }

                                    TempGraphData.TimeStamps.RemoveAt(0);
                                    TempGraphData.StatusValues.RemoveAt(0);
                                    TempGraphData.LatencyValues.RemoveAt(0);
                                }

                                if (LatencyValueList.Count != 0)
                                {
                                    CompressedData.LatencyValues[EntryIndex] = (int)LatencyValueList.Average();
                                }

                                EntryIndex += 1;
                            }
                            else
                            {
                                IsCompressing = false;
                            }
                        }
                        else
                        {
                            IsCompressing = false;
                        }
                    }
                    while (IsCompressing);

                    CompressedGraphData.Add(CompressedData);
                }
            });

            return CompressedGraphData;
        }
        public async Task<Graph> GetCompressedGraphData(List<GraphData> GraphDataList, int CompressionRateIndex, int CompressionValue, GraphCollection.GraphType DataType)
        {
            List<GraphData> CompressedGraphData = await CompressGraphData(GraphDataList, CompressionRateIndex, CompressionValue);
            Graph TempData = new Graph();
            bool LabelsSet = false;

            foreach (GraphData gd in CompressedGraphData)
            {
                if (!LabelsSet)
                {
                    for (int i = 0; i < gd.TimeStamps.Count; i++)
                    {
                        TempData.Labels.Add(gd.TimeStamps[i].ToString("MM/dd/yyyy - hh:mm:ss tt"));
                    }
                    LabelsSet = true;
                }
                LineSeries ls = new LineSeries() { Title = gd.Label };

                switch (DataType)
                {
                    case GraphCollection.GraphType.Status:
                        {
                            ls.Values = new ChartValues<int>(gd.StatusValues);
                            break;
                        }
                    case GraphCollection.GraphType.Latency:
                        {
                            ls.Values = new ChartValues<int>(gd.LatencyValues);
                            break;
                        }
                }

                TempData.DataCollection.Add(ls);
            }

            return TempData;
        }
    }
}

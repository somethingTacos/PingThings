using PingThings.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PingThings.Helpers
{
    public static class GraphLogger
    {
        public static bool WriteEntry(string GroupName, string Label, string Host, string Status, string Latency) //this needs to be updated to account for PingGroups. --Done
        {
            try
            {
                string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PingThings\" + GroupName; //this will probably be a setting of some kind later, but for now this is fine.

                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }

                string LogFile = $"{LogPath}\\{Environment.MachineName}_to_{Label}__{DateTime.Now.ToString("MM-dd-yyyy")}.log";

                string LogEntry = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}]{GroupName}]{Label}]{Environment.MachineName}]{Host}]{Status}]{Latency}";

                using (StreamWriter sw = new StreamWriter(LogFile, true))
                {
                    sw.WriteLine(LogEntry);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


        /* Split data example
                 * 
                 * 0- 03/13/2020 04:01:58 PM : timestamp
                 * 1- Group 1                : GroupName
                 * 2- something              : Label
                 * 3- RDG-JOMELKO            : FromHost
                 * 4- 10.1.1.41              : ToHost
                 * 5- 0                      : Status
                 * 6- 11                     : Latency
                 */
        public static GraphData ParseLog(List<string> fileData)
        {
            GraphData graphData = new GraphData();

            for (int i = 0; i < fileData.Count; i++)
            {
                string[] split = fileData[i].Split(']');

                if (i == 0)
                {
                    graphData.GroupName = split[1];
                    graphData.Label = split[2];
                    graphData.FromHost = split[3];
                    graphData.ToHost = split[4];
                }

                graphData.TimeStamps.Add(DateTime.Parse(split[0]));
                graphData.StatusValues.Add(Convert.ToInt32(split[5]));
                graphData.LatencyValues.Add(Convert.ToInt32(split[6]));
            }

            return graphData;
        }
    }
}

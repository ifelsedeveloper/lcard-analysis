using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LGA.DataSourceLGraph
{
    //Oscilloscope Data File
    //Experiment Time :   10-06-2014 15:12:09
    //Number of frames: 644480

    //Module: E-2010B (5R385464)

    //Number Of Channels : 3
    //Input Rate In kHz: 1000.000000
    //Input Time In Sec: 0.644480
    //Decimation: 1
    //Data Format: Volts
    //Time markers scale: секунды
    //Segments: 1
    //Data as Time Sequence:
    //                    Ch  1      Ch  2      Ch  3  
    //                 Канал 1    Канал 2    Канал 3   


    public class DataManager
    {
        static string[] headers = { "Experiment Time",
                                    "Number of frames",
                                    "Module",
                                    "Input Rate In kHz",
                                    "Input Time In Sec",
                                    "Decimation",
                                    "Data Format",
                                    "Time markers scale",
                                    "Segments",
                                    "Data as Time Sequence"};

        static Color[] colors = {
                                    Colors.Blue, 
                                    Colors.Red, 
                                    Colors.Green, 
                                    Colors.DarkOrange, 
                                    Colors.Chocolate 
                                };

        public DataManager()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
        }

        public LGraphData readData(string path)
        {
            LGraphData record = readHeader(path);
            if (record != null)
            {
                try
                {
                    StreamReader file = new StreamReader(path, System.Text.Encoding.GetEncoding(1251));
                    //read all orther data
                    //skeap headers
                    string line = null;
                    do
                    {
                        line = file.ReadLine();
                    } while (line != null && !line.Contains("Data as Time Sequence"));

                    //find first data line
                    do
                    {
                        line = file.ReadLine();
                    } while (line != null && !line.Contains("0.0"));

                    //read all data

                    int i = 0, j, s, k;
                    record.time = new double[record.KadrsNumber];
                    record.ch = new double[record.NumberOfChannels][];
                    for (i = 0; i < record.NumberOfChannels; i++) record.ch[i] = new double[record.KadrsNumber];

                    string[] spline;
                    string[] spline2 = new string[24];
                    i = 0;
                    do
                    {
                        spline = line.Split(' ');
                        for (k = 0, s = 0; k < spline.Count(); k++)
                            if (spline[k].Count() != 0)
                            {
                                spline2[s] = spline[k];
                                s++;
                            }
                        record.time[i] = Convert.ToDouble(spline2[0]);
                        for (j = 0; j < record.NumberOfChannels; j++)
                            record.ch[j][i] = Convert.ToDouble(spline2[j + 1]);
                        line = file.ReadLine();
                        i++;
                    } while (i < record.KadrsNumber);

                    for (i = 0; i < record.NumberOfChannels; i++)
                    {
                        record.DataChannels.Add(new LGraphDataChannel()
                        {
                            Name = (i+1).ToString(),
                            ChannelColor = colors[i%4],
                            Values = record.ch[i],
                            Times = record.time,
                            Enabled = true,
                            TypeChannel = LGraphDataChannel.ChannelTypes[i%3].Id
                        });
                    }

                    file.Close();
                }
                catch
                {
                    return null;
                }
            }
            return record;
        }

        public void ReadDataAsync(string path, Action<LGraphData> Complete)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
                LGraphData res = this.readData(path);
                Complete(res);
            });
        }

        public LGraphData readHeader(string path)
        {
            try
            {
                LGraphData res = new LGraphData();
                StreamReader file = new StreamReader(path, Encoding.GetEncoding(1251));
                List<string> headerLines = new List<string>();
                int nLine = 0;
                while (nLine < 100)
                {
                    string line = file.ReadLine();
                    if (line == null || line.Contains("Data as Time Sequence"))
                    {
                        break;
                    }
                    headerLines.Add(line);
                    nLine++;
                }
                //parsing headers
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Метоположение", Value = path });

                res.ExperimentTime = findTextValue(headerLines, "Experiment Time");
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Время эксперимента", Value = res.ExperimentTime });

                //validation region Kadrs Number | Number of frames
                int? NumberOfFrames = findIntValue(headerLines, "Number of frames");
                if (NumberOfFrames != null)
                {
                    res.KadrsNumber = (int)NumberOfFrames;
                }
                else
                {
                    NumberOfFrames = findIntValue(headerLines, "Kadrs Number");
                    if (NumberOfFrames == null) 
                        return null;
                    else
                        res.KadrsNumber = (int)NumberOfFrames;
                }
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Число записей", Value = NumberOfFrames.ToString() });
                //Number Of Channels
                int? NumberOfChannels = findIntValue(headerLines, "Number Of Channel");
                if (NumberOfChannels != null)
                    res.NumberOfChannels = (int)NumberOfChannels;
                else
                    return null;
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Число каналов", Value = NumberOfChannels.ToString() });

                res.Module = findTextValue(headerLines, "Module");
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Модуль", Value = res.Module });

                res.InputRateInkHz = findDoubleValue(headerLines, "Input Rate In kHz");
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Частота, КГц", Value = res.InputRateInkHz.ToString() });

                res.TimeMarkersScale = findTextValue(headerLines, "Time markers scale");
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Еденицы измерения времени", Value = res.TimeMarkersScale });
                res.DataFormat = findTextValue(headerLines, "Data Format");

                res.InputTimeInSec = findDoubleValue(headerLines, "Input Time In Sec");
                res.HeaderItems.Add(new LGraphHeaderItem() { Title = "Время записи, с", Value = res.InputTimeInSec.ToString() });

                

                file.Close();
                return res;
            }
            catch
            {
                return null;
            }
        }

        string findTextValue(List<string> data, string key)
        {
            string res = "";
            string strValue = data.FirstOrDefault(str => str.Contains(key));
            if (strValue != null && strValue.Length > 3)
            {
                int posSepartor = strValue.IndexOf(':');
                if (posSepartor > 3)
                    res = strValue.Substring(posSepartor + 2, strValue.Length - posSepartor - 2);
            }
            return res;
        }

        int? findIntValue(List<string> data, string key)
        {
            int? res = null;
            string strValue = findTextValue(data, key);
            if (strValue.Length > 0)
            {
                try
                {
                    res = Convert.ToInt32(strValue.Replace(" ", ""));
                }
                catch
                {
 
                }
            }
            return res;
        }

        double? findDoubleValue(List<string> data, string key)
        {
            double? res = null;
            string strValue = findTextValue(data, key);
            if (strValue.Length > 0)
            {
                try
                {
                    res = Convert.ToDouble(strValue.Replace(" ", ""));
                }
                catch
                {

                }
            }
            return res;
        }
    }
}

using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SignalLabelingApp.Classes
{
    public class PythonMiniseedReader
    {
        public static MiniseedFile ReadMiniseedFile(string miniseedFilePath)
        {
            MiniseedFile miniseedFileResult = new MiniseedFile
            {
                filePath = miniseedFilePath
            };

            EnsurePythonAndObspy();

            // Укажите путь к встроенному Python (вложенный в папку рядом с .exe)
            string pythonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python39");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", Path.Combine(pythonPath, "python39.dll"));
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonPath);
            Environment.SetEnvironmentVariable("PYTHONPATH", Path.Combine(pythonPath, "Lib", "site-packages"));

            // Инициализация Python.NET
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                try
                {
                    // Импорт библиотеки obspy
                    dynamic obspy = Py.Import("obspy");
                    dynamic read = obspy.read;

                    // Чтение Miniseed файла
                    dynamic st = read(miniseedFilePath);

                    // Группируем данные по станциям
                    var stationGroups = new Dictionary<string, StationData>();

                    foreach (var tr in st)
                    {
                        // Извлекаем основные параметры трассы
                        string stationName = tr.stats.station;
                        string channelName = tr.stats.channel;

                        TraceData trace = new TraceData
                        {
                            StationName = stationName,
                            SampleRate = (int)tr.stats.sampling_rate,
                            NumberOfSamples = (int)tr.stats.npts,
                            starttime = ParseUtcDateTime(tr.stats.starttime.ToString()),
                            endtime = ParseUtcDateTime(tr.stats.endtime.ToString()),
                            
                        };

                        trace.data = ConvertPythonListToDotNetList(tr.data.tolist());

                        // Ищем или создаем StationData для текущей станции
                        if (!stationGroups.TryGetValue(stationName, out var stationData))
                        {
                            stationData = new StationData();
                            stationData.StationName = stationName;
                            stationGroups[stationName] = stationData;
                        }

                        // Назначаем канал в StationData
                        if (channelName == "CH1" || channelName == "HHE")
                        {
                            stationData.Channel1 = trace;
                        }
                        else if (channelName == "CH2" || channelName == "HHN")
                        {
                            stationData.Channel2 = trace;
                        }
                        else if (channelName == "CH3" || channelName == "HHZ")
                        {
                            stationData.Channel3 = trace;
                        }
                    }


                    // Синхронизация сигналов
                    foreach (var stationData in stationGroups.Values)
                    {
                        DateTime maxStartTime = new[] {
                            stationData.Channel1?.starttime ?? DateTime.MinValue,
                            stationData.Channel2?.starttime ?? DateTime.MinValue,
                            stationData.Channel3?.starttime ?? DateTime.MinValue
                        }.Max();

                                            DateTime minEndTime = new[] {
                            stationData.Channel1?.endtime ?? DateTime.MaxValue,
                            stationData.Channel2?.endtime ?? DateTime.MaxValue,
                            stationData.Channel3?.endtime ?? DateTime.MaxValue
                        }.Min();

                        if (stationData.Channel1 != null)
                            TrimTraceData(stationData.Channel1, maxStartTime, minEndTime);
                        if (stationData.Channel2 != null)
                            TrimTraceData(stationData.Channel2, maxStartTime, minEndTime);
                        if (stationData.Channel3 != null)
                            TrimTraceData(stationData.Channel3, maxStartTime, minEndTime);
                    }


                    // Добавляем данные станций в MiniseedFile
                    miniseedFileResult.stationDataStructures = new ObservableCollection<StationData>(stationGroups.Values);
                    miniseedFileResult.stationsAmount = stationGroups.Count;
                }
                catch (PythonException ex)
                {
                    Console.WriteLine($"Ошибка Python: {ex.Message}");
                }
            }
            //PythonEngine.Shutdown();
            return miniseedFileResult;
        }

        private static void TrimTraceData(TraceData trace, DateTime maxStartTime, DateTime minEndTime)
        {
            if (trace.starttime < maxStartTime)
            {
                int startIndex = (int)((maxStartTime - trace.starttime).TotalSeconds * trace.SampleRate);
                startIndex = Math.Max(0, startIndex);
                trace.data = trace.data.Skip(startIndex).ToList();
                trace.starttime = maxStartTime;
            }

            if (trace.endtime > minEndTime)
            {
                int endIndex = trace.data.Count - (int)((trace.endtime - minEndTime).TotalSeconds * trace.SampleRate);
                endIndex = Math.Min(trace.data.Count, endIndex);
                trace.data = trace.data.Take(endIndex).ToList();
                trace.endtime = minEndTime;
            }

            trace.NumberOfSamples = trace.data.Count;
        }

        private static List<float> ConvertPythonListToDotNetList(dynamic pythonList)
        {
            List<float> dotNetList = new List<float>();

            foreach (var item in pythonList)
            {
                dotNetList.Add((float)item); // Явное приведение каждого элемента к float
            }

            return dotNetList;
        }

        public static DateTime ParseUtcDateTime(string utcDateTimeString)
        {
            if (string.IsNullOrWhiteSpace(utcDateTimeString))
            {
                throw new ArgumentException("Input string is null or empty.", nameof(utcDateTimeString));
            }

            // Указываем формат строки времени
            string format = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

            // Пытаемся распарсить строку в DateTime с учетом UTC
            if (DateTime.TryParseExact(utcDateTimeString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            throw new FormatException($"Invalid UTC DateTime format: {utcDateTimeString}");
        }

        private static void EnsurePythonAndObspy()
        {
            string pythonDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python39");
            string pythonZipUrl = "https://www.python.org/ftp/python/3.9.13/python-3.9.13-embed-amd64.zip";
            string pythonZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python-3.9.13-embed-amd64.zip");

            // Check if Python directory exists
            if (!Directory.Exists(pythonDir))
            {
                Console.WriteLine("Python 3.9 directory not found. Downloading...");

                // Download Python embed package
                using (var client = new WebClient())
                {
                    client.DownloadFile(pythonZipUrl, pythonZipPath);
                }

                Console.WriteLine("Download complete. Extracting...");

                // Extract Python embed package
                System.IO.Compression.ZipFile.ExtractToDirectory(pythonZipPath, pythonDir);

                // Delete the zip file
                File.Delete(pythonZipPath);

                Console.WriteLine("Python 3.9 extracted successfully.");

                // Edit python39._pth to enable site-packages
                string pythonPthFile = Path.Combine(pythonDir, "python39._pth");
                if (File.Exists(pythonPthFile))
                {
                    Console.WriteLine("Editing python39._pth to enable site-packages...");
                    var lines = File.ReadAllLines(pythonPthFile);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Trim() == "#import site")
                        {
                            lines[i] = "import site";
                            break;
                        }
                    }
                    File.WriteAllLines(pythonPthFile, lines);
                    Console.WriteLine("Edited python39._pth successfully.");
                }
            }

            // Ensure required folders exist
            string sitePackagesDir = Path.Combine(pythonDir, "Lib", "site-packages");
            Directory.CreateDirectory(sitePackagesDir);

            // Set environment variables
            Environment.SetEnvironmentVariable("PYTHONPATH", sitePackagesDir, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", pythonDir + ";" + Path.Combine(pythonDir, "Scripts") + ";" + Environment.GetEnvironmentVariable("PATH"), EnvironmentVariableTarget.Process);

            // Check if pip is installed
            if (!File.Exists(Path.Combine(pythonDir, "Scripts", "pip.exe")))
            {
                Console.WriteLine("pip not found. Installing...");
                string getPipUrl = "https://bootstrap.pypa.io/get-pip.py";
                string getPipPath = Path.Combine(pythonDir, "get-pip.py");

                // Download get-pip.py
                using (var client = new WebClient())
                {
                    client.DownloadFile(getPipUrl, getPipPath);
                }

                // Run get-pip.py
                RunCommand(Path.Combine(pythonDir, "python.exe"), getPipPath);
                File.Delete(getPipPath);

                Console.WriteLine("pip installed successfully.");
            }

            // Validate pip
            Console.WriteLine("Validating pip installation...");
            RunCommand(Path.Combine(pythonDir, "python.exe"), "-m pip --version");
            Console.WriteLine("pip validated successfully.");

            // Install obspy
            Console.WriteLine("Installing obspy...");
            RunCommand(Path.Combine(pythonDir, "python.exe"), "-m pip install obspy");

            Console.WriteLine("obspy installed successfully.");
        }

        private static void RunCommand(string executable, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed: {executable} {arguments}\nError: {error}\nOutput: {output}");
            }

            Console.WriteLine(output);
        }
    }
}




        
    

using System;
using System.Collections.Generic;

namespace TestHarness.Models
{
    public class DataConnectionViewModel
    {
        public int Calls { get; set; }
        public string ElapsedTime { get; set; }
        public string ElapsedTimeMilli { get; set; }
        public string IPAddress { get; set; }
        public int LamCalls { get; set; } 
        public string LamElapsedTime { get; set; }
        public string LamElapsedTimeMilli { get; set; }
        public string TotalCallTime { get; set; }

    }

    public class DB_CS {
        public string username { get; set; }
        public string password { get; set; }
        public string host { get; set; }        
        public string engine { get; set; }
        public string port {get; set;}
        public string dbInstanceIdentifier {get; set;}
    }

    public class testRuns {

        private static List<List<decimal>> _runs = new List<List<decimal>>();
        public static List<List<decimal>> runs { 
            get {
                return _runs;
            }
            set {
                _runs = value;
            } 
        }

        private static List<string> _runType = new List<string>();
        public static List<string> runType { 
            get {
                return _runType;
            }
            set {
                _runType = value;
            } 
        }

        private static List<List<decimal>> _totalCallTime = new List<List<decimal>>();
        public static List<List<decimal>> totalCallTime { 
            get {
                return _totalCallTime;
            }
            set {
                _totalCallTime = value;
            } 
        }

        private static List<decimal> _ave;
        public static List<decimal> Averages { 
            get {
                _ave = new List<decimal>();
                foreach (List<decimal> DList in _runs) {
                    decimal tempAve = 0;
                    decimal[] d  = DList.ToArray();
                    for (int i = 0; i < d.Length; i++) {
                        tempAve += d[i];
                    }
                    if(tempAve > 0 && d.Length >0) {
                        _ave.Add(tempAve/d.Length);
                    } else {
                        _ave.Add(0);
                    }
                }
                return _ave;
            }
        }


        private static List<decimal> _median;
        public static List<decimal> Median { 
            get {
                _median = new List<decimal>();
                foreach (List<decimal> DList in _runs) {
                    decimal tempMedian = 0;
                    decimal[] d  = DList.ToArray();
                    Array.Sort(d);
                    tempMedian = (d.Length % 2 != 0) ? d[d.Length / 2] : (d[(d.Length / 2) - 1] + d[d.Length / 2]) / 2;
                    _median.Add(tempMedian);
                }
                return _median;
            }
        }

        private static List<decimal> _max;
        public static List<decimal> Max { 
            get {
                _max = new List<decimal>();
                foreach (List<decimal> DList in _runs) {
                    decimal tempMax = 0;
                    decimal[] d  = DList.ToArray();
                    for (int i = 0; i < d.Length; i++) {
                        if (tempMax < d[i]) {
                            tempMax = d[i];
                        }
                    }
                    _max.Add(tempMax);
                }
                return _max;
            }
        }

        private static List<decimal> _min;
        public static List<decimal> Min { 
            get {
                _min = new List<decimal>();
                foreach (List<decimal> DList in _runs) {
                    decimal tempMin = 100000000000000;
                    decimal[] d  = DList.ToArray();
                    for (int i = 0; i < d.Length; i++) {
                        if (tempMin > d[i]) {
                            tempMin = d[i];
                        }
                    }
                    _min.Add(tempMin);
                }
                return _min;
            }
        }


        private static List<decimal> _mode;
        public static List<decimal> Mode { 
            get {
                _mode = new List<decimal>();
                foreach (List<decimal> DList in _runs) {
                    _mode.Add(GetMode(DList));
                }
                return _mode;
            }
        }

         private static decimal GetMode(List<decimal> list)
        {
            // Initialize the return value
            decimal mode = -1;
            // Test for a null reference and an empty list
            if (list != null && list.Count > 0)
            {
                // Store the number of occurences for each element
                Dictionary<decimal, int> counts = new Dictionary<decimal, int>();
                // Add one to the count for the occurence of a character
                foreach (decimal element in list)
                {
                    decimal key = Math.Round(element,1);
                    if (counts.ContainsKey(key))
                        counts[key]++;
                    else
                        counts.Add(key, 1);
                }
                // Loop through the counts of each element and find the 
                // element that occurred most often
                int max = 0;
                foreach (KeyValuePair<decimal, int> count in counts)
                {
                    if (count.Value > max)
                    {
                        // Update the mode
                        mode = count.Key;
                        max = count.Value;
                    }
                }
            }
            return mode;
        }

        public static void reset() {
            _totalCallTime = new List<List<decimal>>();
            _runs = new List<List<decimal>>();
            _ave = new List<decimal>();
            _median = new List<decimal>();
            _mode = new List<decimal>();
            _min = new List<decimal>();
            _max = new List<decimal>();
            _runType = new List<string>();
        }

        /*
        SerializeRunResults - this is a partial implementation. I found the built in JsonConvert.SerializeObject met my needs and abandon this approach.
        */
        public static string SerializeRunResults() {
        
            string runValuesString = "{\n";

            foreach (List<decimal> DList in _runs)
            {

                runValuesString += "[";
                
                decimal[] d  = DList.ToArray();
                for (int i = 0; i < d.Length; i++) {
                    runValuesString += d[i];
                    if(i + 1 < d.Length) {
                        runValuesString += ",";
                    } else {
                        runValuesString += "]\n";
                    }
                }
            }

                runValuesString += "\n}";
            Console.WriteLine("CurRun values:" +  runValuesString);
            return runValuesString;
        
        }

    }

}
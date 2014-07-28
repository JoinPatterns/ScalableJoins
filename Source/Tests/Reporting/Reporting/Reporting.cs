using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class Reporting
    {
        public static Dictionary<string, Dictionary<int, long>> data = new Dictionary<string, Dictionary<int, long>>();
        public static List<int> X = new List<int>();

        public static void AddX(int x) {
            if (!X.Contains(x)) X.Add(x);
        }

        public static void ReportData(bool withSpeedup = true)
        {
            {
                Console.WriteLine("Absolute Times");

                var seriesnames = data.Keys;
                Console.Write("{0,-20},", "threads");
                foreach (var x in X)
                {
                    Console.Write("{0,10},", x);
                }
                Console.WriteLine();
                foreach (var name in seriesnames)
                {
                    Console.Write("{0,-20},", name);
                    foreach (var x in X)
                    {
                        Console.Write("{0,10},", (data[name].ContainsKey(x)) ? data[name][x].ToString() : "");
                    }
                    Console.WriteLine();
                }
            }

            if (!withSpeedup) return;

            Console.WriteLine();
            {
                Console.WriteLine("Speedup");

                var seriesnames = data.Keys;
                Console.Write("{0,-20},", "threads");
                foreach (var x in X)
                {
                    Console.Write("{0,10},", x);
                }
                Console.WriteLine();
                foreach (var name in seriesnames)
                {
                    Console.Write("{0,-20},", name);
                    foreach (var x in X)
                    {
                        Console.Write("{0,10:0.00},", (data[name].ContainsKey(x)) ? ((data[name][X[0]] * 1.0) / data[name][x] * 1.0) : 0.0);
                    }
                    Console.WriteLine();
                }
            }

        }
    }
   


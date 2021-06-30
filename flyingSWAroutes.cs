using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace flyingSWAroutes
{
    class flyingSWAroutes
    {
        static void Main(string[] args)
        {
            DateTime StartTime = DateTime.Now;

            MySqlConnection myConnection = new();
            MySqlCommand myCommand = new();

            if (!SQLdbOpen(myConnection, myCommand)) return;

            DoWork(myCommand);

            CloseDBandEndProgram(StartTime, myConnection);
            return;
        }

        private static void DoWork(MySqlCommand myCommand)
        {
            MySqlDataAdapter myAdapter = new();

            string[,] FlightPaths = new string[10000, 31];

            DataTable AllFlightInfo = new();
            myCommand.CommandText = "SELECT * from flying.flightaware where " +
                "DepartureHerbDateTime >= '2021-06-20' and DepartureHerbDateTime <= '2021-06-26' " +
                "order by FlightNumber, DepartureHerbDateTime";
            myAdapter.SelectCommand = myCommand;
            myAdapter.Fill(AllFlightInfo);

            for (int i = 0; i < AllFlightInfo.Rows.Count; i++)
            {
                string FlightNumber = Convert.ToString(AllFlightInfo.Rows[i]["FlightNumber"]);

                if (int.TryParse(FlightNumber[3..], out int FlightNum))
                {
                    string DepartureCity = Convert.ToString(AllFlightInfo.Rows[i]["DepartureCity"]);
                    string ArrivalCity = Convert.ToString(AllFlightInfo.Rows[i]["ArrivalCity"]);
                    int HerbDay = Convert.ToDateTime(AllFlightInfo.Rows[i]["DepartureHerbDateTime"]).Day;

                    if (FlightPaths[FlightNum, HerbDay] == null)
                    {
                        FlightPaths[FlightNum, HerbDay] = DepartureCity + " " + ArrivalCity;
                    }
                    else
                    {
                        if (FlightPaths[FlightNum, HerbDay].EndsWith(DepartureCity))
                        {
                            FlightPaths[FlightNum, HerbDay] += " " + ArrivalCity;
                        }
                        else
                        {
                            FlightPaths[FlightNum, HerbDay] += " ?? " + DepartureCity + " " + ArrivalCity;
                        }
                    }
                }
            }

            StreamWriter sw = new(@"SWAroutes.txt");

            for (int FlightNum = 1; FlightNum < 8000; FlightNum++)
            {
                List<string> OutputDays = new();
                List<string> OutputFlightPath = new();

                for (int d = 1; d < 31; d++)
                {
                    string ThisFlightPath = FlightPaths[FlightNum, d];

                    if (ThisFlightPath != null)
                    {
                        string[] DayOfWeekArray = new string[7];
                        for (int x = 0; x < 7; x++) DayOfWeekArray[x] = "-";
                        DateTime FlightDateTime = new(2021, 6, d);
                        int DayIdx = (int)FlightDateTime.DayOfWeek;
                        DayOfWeekArray[DayIdx] = FlightDateTime.DayOfWeek.ToString().Substring(0, 1);
                        int IdxOf = OutputFlightPath.IndexOf(ThisFlightPath, 0);
                        if (IdxOf == -1)
                        {
                            string DayPattern = DayOfWeekArray[0];
                            for (int x = 1; x < 7; x++) DayPattern += DayOfWeekArray[x];
                            OutputDays.Add(DayPattern);
                            OutputFlightPath.Add(ThisFlightPath);
                        }
                        else
                        {
                            string DayPattern = string.Empty;
                            for (int x = 0; x < 7; x++)
                            {
                                if (x == DayIdx)
                                {
                                    DayPattern += DayOfWeekArray[x];
                                }
                                else
                                {
                                    DayPattern += OutputDays[IdxOf].Substring(x, 1);
                                }
                            }
                            OutputDays[IdxOf] = DayPattern;
                        }
                    }
                }

                for (int j = 0; j < OutputDays.Count; j++)
                {
                    if (!OutputFlightPath[j].Contains("?"))
                    {
                        Console.WriteLine("{0,4} {1} {2}", FlightNum, OutputDays[j], OutputFlightPath[j]);
                        sw.WriteLine("{0,4} {1} {2}", FlightNum, OutputDays[j], OutputFlightPath[j]);
                    }
                }
            }
            sw.Close();
            return;
        }

        private static bool SQLdbOpen(MySqlConnection myConnection, MySqlCommand myCommand)
        {
            DateTime dbOpenStartTime = DateTime.Now;

            /* 
            * Open Connection to SQL DB
            */

            string ConnectionString = "Insert your connection string here."
            try
            {
                myConnection.ConnectionString = ConnectionString;
                myConnection.Open();
                myCommand.Connection = myConnection;
            }
            catch (MySqlException E)
            {
                Console.WriteLine("Open Error: {0}", E.Message);
                Console.WriteLine("Press RETURN to continue or CONTROL-C to abort");
                Console.ReadLine();
                return false;
            }

            DateTime dbOpenEndTime = DateTime.Now;

            Console.WriteLine("The DB Open took {0} seconds.",
                (dbOpenEndTime - dbOpenStartTime).TotalSeconds.ToString("N2"));

            return true;

        }

        private static void CloseDBandEndProgram(DateTime StartTime, MySqlConnection myConnection)
        {
            myConnection.Close();

            DateTime EndTime = DateTime.Now;

            Console.WriteLine("Start Time : {0}", StartTime);
            Console.WriteLine("  End Time : {0}", EndTime);
            Console.WriteLine("This run took {0} minutes.",
                (EndTime - StartTime).TotalMinutes.ToString("N2"));

            Console.WriteLine("Run Completed");
            if (Debugger.IsAttached) Console.ReadLine();
            return;
        }
    }


}

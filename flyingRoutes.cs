using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace flyingRoutes
{
    class FlyingRoutes
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
            
            DataTable AirportCodes = new();
            myCommand.CommandText = "SELECT * from flying.airports " +
                "left join flying.timezones on airports.timezone = timezones.timezonename " +
                "order by ICAOcode";
            myAdapter.SelectCommand = myCommand;
            myAdapter.Fill(AirportCodes);
            var keys = new DataColumn[1];
            keys[0] = AirportCodes.Columns["ICAOcode"];
            AirportCodes.PrimaryKey = keys;

            DateTime SearchStart = new(2021, 6, 24);
            DateTime SearchEnd = new(2021, 6, 30);

            List<string> AirlineCodes = new() { "AAL", "DAL", "SWA", "UAL" };
            foreach (string AirlineCode in AirlineCodes)
            {
                DoAirline(myCommand, AirlineCode, AirportCodes, SearchStart, SearchEnd);
            }
            return;
        }
        private static void DoAirline(MySqlCommand myCommand, 
                                      string AirlineCode, 
                                      DataTable AirportCodes,
                                      DateTime SearchStart,
                                      DateTime SearchEnd)
        {
            MySqlDataAdapter myAdapter = new();
            string[,] FlightPaths = new string[10000, 32];

            DataTable AllFlightInfo = new();
            myCommand.CommandText = "SELECT * from flying.flightaware where " +
                "FlightNumber Like '" + AirlineCode + "%' and " +
                "FlightDate >= '" + SearchStart.ToString("yyyyMMdd") + "' and " +
                "FlightDate <= '" + SearchEnd.ToString("yyyyMMdd") + "' " +
                "order by FlightNumber, FlightDate, FlightTime";
            Console.WriteLine(myCommand.CommandText);
            myAdapter.SelectCommand = myCommand;
            myAdapter.Fill(AllFlightInfo);

            for (int i = 0; i < AllFlightInfo.Rows.Count; i++)
            {
                string FlightNumber = Convert.ToString(AllFlightInfo.Rows[i]["FlightNumber"]);

                if (int.TryParse(FlightNumber[3..], out int FlightNum))
                {
                    string DepartureCity = Convert.ToString(AllFlightInfo.Rows[i]["DepartureCity"]);
                    string ArrivalCity = Convert.ToString(AllFlightInfo.Rows[i]["ArrivalCity"]);

                    object[] FindAirportCode = new object[1];
                    FindAirportCode[0] = DepartureCity.Trim();
                    DataRow ThisAirportCode = AirportCodes.Rows.Find(FindAirportCode);

                    string CityName = string.Empty;

                    string DaylightSavingsTimeOffset = string.Empty;
                    string StandardTimeOffset = string.Empty;
                    string NextChangeDirection = string.Empty;
                    string NextChangeDate = string.Empty;

                    if (ThisAirportCode != null)
                    {
                        CityName = Convert.ToString(ThisAirportCode["Municipality"]);
                        DaylightSavingsTimeOffset = Convert.ToString(ThisAirportCode["DaylightSavingsTimeOffset"]);
                        StandardTimeOffset = Convert.ToString(ThisAirportCode["StandardTimeOffset"]);
                        NextChangeDirection = Convert.ToString(ThisAirportCode["NextChangeDirection"]);
                        NextChangeDate = Convert.ToString(ThisAirportCode["NextChangeDate"]);
                    }
                    else
                    {
                        DataTable CheckForRecord = new();
                        myCommand.CommandText = "SELECT * FROM flying.airports " +
                            "where ICAOcode = ?ICAOcode";
                        myCommand.Parameters.AddWithValue("?ICAOcode", DepartureCity);
                        myAdapter.SelectCommand = myCommand;
                        myAdapter.Fill(CheckForRecord);
                        myCommand.Parameters.Clear();

                        if (CheckForRecord.Rows.Count == 0)
                        {
                            myCommand.CommandText = "INSERT INTO flying.airports (ICAOcode, AirportName) " +
                                "VALUES (?ICAOcode, ?AirportName)";
                            myCommand.Parameters.AddWithValue("?ICAOcode", DepartureCity);
                            myCommand.Parameters.AddWithValue("?AirportName", "<Needs Research>");
                            myCommand.ExecuteNonQuery();
                            myCommand.Parameters.Clear();
                        }
                    }

                    if (StandardTimeOffset == string.Empty) StandardTimeOffset = "+0 hours";
                    if (DaylightSavingsTimeOffset == string.Empty) DaylightSavingsTimeOffset = "+0 hours";

                    string UtcFlightDate = Convert.ToString(AllFlightInfo.Rows[i]["FlightDate"]);
                    int UtcYear = Convert.ToInt16(UtcFlightDate.Substring(0, 4));
                    int UtcMonth = Convert.ToInt16(UtcFlightDate.Substring(4, 2));
                    int UtcDay = Convert.ToInt16(UtcFlightDate.Substring(6, 2));
                    string UtcFlightTime = Convert.ToString(AllFlightInfo.Rows[i]["FlightTime"]);
                    int UtcHour = Convert.ToInt16(UtcFlightTime.Substring(0, 2));
                    int UtcMinute = Convert.ToInt16(UtcFlightTime.Substring(2, 2));
                    DateTime DepartureDateTime = new(UtcYear, UtcMonth, UtcDay, UtcHour, UtcMinute, 0);

                    string OffsetToUse = StandardTimeOffset;
                    if (NextChangeDirection.ToLower() == "standard")
                    {
                        OffsetToUse = DaylightSavingsTimeOffset;
                    }

                    string[] OffsetParts = OffsetToUse.Split(':', ' ');
                    double DeltaHours = Convert.ToDouble(OffsetParts[0]);
                    double.TryParse(OffsetParts[1], out double DeltaMinutes);

                    DepartureDateTime = DepartureDateTime.AddHours(DeltaHours).AddMinutes(DeltaMinutes);

                    int DepartureDay = DepartureDateTime.Day;

                    if (FlightPaths[FlightNum, DepartureDay] == null)
                    {
                        FlightPaths[FlightNum, DepartureDay] = DepartureCity + " " + ArrivalCity;
                    }
                    else
                    {
                        if (FlightPaths[FlightNum, DepartureDay].EndsWith(DepartureCity))
                        {
                            FlightPaths[FlightNum, DepartureDay] += " " + ArrivalCity;
                        }
                        else
                        {
                            FlightPaths[FlightNum, DepartureDay] += " ?? " + DepartureCity + " " + ArrivalCity;
                        }
                    }
                }
            }

            DataTable GetAirlineName = new();
            myCommand.CommandText = "SELECT * FROM flying.airlines " +
                "where ICAOcode = ?ICAOcode";
            myCommand.Parameters.AddWithValue("?ICAOcode", AirlineCode);
            myAdapter.SelectCommand = myCommand;
            myAdapter.Fill(GetAirlineName);
            myCommand.Parameters.Clear();

            string AirlineName = "Unknown";
            if (GetAirlineName.Rows.Count > 0)
            {
                AirlineName = Convert.ToString(GetAirlineName.Rows[0]["AirlineName"]);
            }

            string OutputName = @"C:\users\mark\documents\flying\github\" + AirlineCode + "routes.txt";
            StreamWriter sw = new(OutputName);
            sw.WriteLine("Airline: {0} {1}", AirlineCode, AirlineName);
            sw.WriteLine("Based on flight data from {0} to {1}",
                SearchStart.ToString("MM/dd/yyyy"), 
                SearchEnd.ToString("MM/dd/yyyy"));

            sw.WriteLine("FL #   Days      Route");
            sw.WriteLine("---- ------- ------------------------------------------------------------------");
            for (int FlightNum = 1; FlightNum < 9999; FlightNum++)
            {
                List<string> OutputDays = new();
                List<string> OutputFlightPath = new();

                for (int d = 1; d <= SearchEnd.Day; d++)
                {
                    string ThisFlightPath = FlightPaths[FlightNum, d];

                    if (ThisFlightPath != null)
                    {
                        string[] DayOfWeekArray = new string[7];
                        for (int x = 0; x < 7; x++) DayOfWeekArray[x] = "-";

                        DateTime FlightDateTime = new(SearchStart.Year, SearchStart.Month, d);
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
                        string FancyFlightPath = AddCityNames(OutputFlightPath[j], AirportCodes);
                        //Console.WriteLine("{0,4} {1} {2}", FlightNum, OutputDays[j], FancyFlightPath);
                        sw.WriteLine("{0,4} {1} {2}", FlightNum, OutputDays[j], FancyFlightPath);
                    }
                }
            }
            sw.Close();
            return;
        }

        private static string AddCityNames(string FlightPath, DataTable AirportCodes)
        {
            string FancyFlightPath = string.Empty;
            string[] FlightCities = FlightPath.Split(' ');

            foreach (string FlightCity in FlightCities)
            {
                object[] FindAirportCode = new object[1];
                FindAirportCode[0] = FlightCity.Trim();
                DataRow ThisAirportCode = AirportCodes.Rows.Find(FindAirportCode);

                string CityName = string.Empty;
                if (ThisAirportCode != null)
                {
                    CityName = Convert.ToString(ThisAirportCode["Municipality"]);
                }
                FancyFlightPath = FancyFlightPath += FlightCity + "(" + CityName + ") ";

            }

            return FancyFlightPath.Trim();
        }

        private static bool SQLdbOpen(MySqlConnection myConnection, MySqlCommand myCommand)
        {
            DateTime dbOpenStartTime = DateTime.Now;

            /* 
            * Open Connection to SQL DB
            */

            string ConnectionString = "Insert your connection string here";

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

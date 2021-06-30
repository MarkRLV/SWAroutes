using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace FlightAwareDataDownload
{
    class FlightAwareDataDownload
    {
        static void Main(string[] args)
        {
            DateTime StartTime = DateTime.Now;

            MySqlConnection myConnection = new();
            MySqlCommand myCommand = new();

            if (!SQLdbOpen(myConnection, myCommand)) return;

            for (int i = 0; i < 10000; i++)
            {
                string FlightNumber = "WN" + i.ToString();
                string AltFlightNumber = "SWA" + i.ToString();
                Console.WriteLine("Processing Flight: {0} {1}", FlightNumber, AltFlightNumber);
                GetFlightInfo(myCommand, AltFlightNumber);
            }

            CloseDBandEndProgram(StartTime, myConnection);
            return;
        }

        private static void GetFlightInfo(MySqlCommand myCommand, string FlightNumber)
        {
            List<string> HistoryList = new();

            string url = "https" + "://flightaware.com/live/flight/" + FlightNumber;
            string htmlCode = null;

            using (WebClient client = new())
            {
                try
                {
                    htmlCode = client.DownloadString(url);
                }
                catch (Exception E)
                {
                    Console.WriteLine("URL: {0}", E.Message);
                    Console.WriteLine("Press RETURN to continue.");
                    Console.ReadLine();
                    return;
                }
            }

            bool CollectControl = false;
            string FlightDate = string.Empty;
            string FlightTime = string.Empty;
            string DepartureCity = string.Empty;
            string NextField = string.Empty;

            string[] htmlParts = htmlCode.Split('/');
            foreach (string htmlPart in htmlParts)
            {
                if (htmlPart == FlightNumber)
                {
                    CollectControl = true;
                }

                if (CollectControl)
                {
                    switch (NextField)
                    {
                        case "DepartureCity":
                            DepartureCity = htmlPart;
                            if (DepartureCity.Length > 4)
                            {
                                DepartureCity = DepartureCity.Substring(0, 4);
                            }

                            NextField = "ArrivalCity";
                            break;

                        case "ArrivalCity":
                            string ArrivalCity = htmlPart;
                            if (ArrivalCity.Length > 4)
                            {
                                ArrivalCity = ArrivalCity.Substring(0, 4);
                            }

                            /*
                            Console.WriteLine("FlightDate     : {0}", FlightDate);
                            Console.WriteLine("FlightTime     : {0}", FlightTime);
                            Console.WriteLine("DepartureCity  : {0}", DepartureCity);
                            Console.WriteLine("ArrivalCity    : {0}", ArrivalCity);
                            */

                        string AllInfo = FlightNumber + FlightDate + FlightTime + DepartureCity + ArrivalCity;

                            if (!HistoryList.Contains(AllInfo))
                            {
                                int year = Convert.ToInt16(FlightDate.Substring(0, 4));
                                int month = Convert.ToInt16(FlightDate.Substring(4, 2));
                                int day = Convert.ToInt16(FlightDate.Substring(6, 2));

                                int hour = Convert.ToInt16(FlightTime.Substring(0, 2));
                                int minute = Convert.ToInt16(FlightTime.Substring(2, 2));

                                DateTime HerbDateTime = new DateTime(year, month, day, hour, minute, 0).AddHours(-6);

                                myCommand.CommandText = "INSERT INTO flying.flightaware " +
                                    "(FlightNumber, FlightDate, FlightTime, " +
                                    "DepartureCity, ArrivalCity, DepartureHerbDateTime) VALUES " +
                                    "(?FlightNumber, ?FlightDate, ?FlightTime, " +
                                    "?DepartureCity, ?ArrivalCity, ?DepartureHerbDateTime)";
                                myCommand.Parameters.AddWithValue("?FlightNumber", FlightNumber);
                                myCommand.Parameters.AddWithValue("?FlightDate", FlightDate);
                                myCommand.Parameters.AddWithValue("?FlightTime", FlightTime);
                                myCommand.Parameters.AddWithValue("?DepartureCity", DepartureCity);
                                myCommand.Parameters.AddWithValue("?ArrivalCity", ArrivalCity);
                                myCommand.Parameters.AddWithValue("?DepartureHerbDateTime", HerbDateTime);
                                try
                                {
                                    myCommand.ExecuteNonQuery();
                                    Console.WriteLine("Record Saved {0} {1} {2} {3} {4}",
                                        FlightNumber, FlightDate, FlightTime, DepartureCity, ArrivalCity);
                                }
                                catch (Exception E)
                                {
                                    if (E.Message.StartsWith("Duplicate entry"))
                                    {
                                        Console.WriteLine("Already have {0} {1} {2} {3} {4}",
                                            FlightNumber, FlightDate, FlightTime, DepartureCity, ArrivalCity);
                                    }
                                    else
                                    {
                                        Console.WriteLine(E.Message);
                                        Console.ReadLine();
                                    }
                                }
                                finally
                                {
                                    myCommand.Parameters.Clear();
                                }
                                HistoryList.Add(AllInfo);
                            }
                            
                            CollectControl = false;
                            FlightDate = string.Empty;
                            FlightTime = string.Empty;
                            DepartureCity = string.Empty;
                            ArrivalCity = string.Empty;
                            NextField = string.Empty;
                            break;
                    }

                    if (htmlPart.Length == 8 && long.TryParse(htmlPart, out long TestNum))
                    {
                        FlightDate = htmlPart;
                    }

                    if (htmlPart.Length == 5 && htmlPart.EndsWith("Z"))
                    {
                        FlightTime = htmlPart;
                        NextField = "DepartureCity";
                    }
                }
            }

            return;
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

        private static string RemoveQuotes(string v)
        {
            return v.Replace('"', ' ').Trim();
        }
 
    }
}

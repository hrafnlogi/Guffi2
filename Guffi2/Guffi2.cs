using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.ServiceModel;
using WcfServiceLibrary1;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;

namespace Guffi2
{
    public partial class Guffi2 : ServiceBase
    {


        ServiceHost obj;

        bool sentToday = false;
        string username = "";
        string password = "";
        string from = "";
        string fileName = "";

        string dataSource = "";
        string databaseCatalog = "";        
        string databaseUsername = "";
        string databasePassword = "";

        
        int sendingHour;



        public Guffi2()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog"); 
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";



        }

        public void GetConfigurationUsingSection()
        {
            var applicationSettings = ConfigurationManager.GetSection("ApplicationSettings") as NameValueCollection;
            if (applicationSettings.Count == 0)
            {
                eventLog1.WriteEntry("Application Settings are not defined");
            }
            else
            {
                password = applicationSettings["password"];
                username = applicationSettings["username"];
                username = applicationSettings["from"];
                sendingHour = Convert.ToInt32(applicationSettings["klukkitimidags"]);

                dataSource = applicationSettings["dataSource"];
                databaseCatalog = applicationSettings["databaseCaralog"];
                databaseUsername = applicationSettings["databaseUsername"];
                databasePassword = applicationSettings["databasePassword"];

            }
        }

        protected override void OnStart(string[] args)
        {

            obj = new ServiceHost(typeof(WcfServiceLibrary1.Service1));
            obj.Open();

            eventLog1.WriteEntry("Guffi Hefur verið ræstur");

            // Get config variables
            GetConfigurationUsingSection();
            //call process every 60 seconds
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.sixtySeconds);
            timer.Start();
            

        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Guffi hefur verið settur í dvala!");
            obj.Close();
        }




        private IService1 GetLocalClient(string serviceEndpointName)
        {
            //tengjast hverjum ?
            eventLog1.WriteEntry("2");
            var factory = new ChannelFactory<IService1>(serviceEndpointName);
            eventLog1.WriteEntry("3");
            return factory.CreateChannel();
        }


        public string getSMSText(int daysFromToday)
        {
            DateTime now = DateTime.Now;
            string text;
            //get different sms text based on day
            if(now.DayOfWeek == DayOfWeek.Friday)
            {
                text = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skilabodTextiHelgi.txt");
            }
            else
            {
                text = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skilabodTextiVirkur.txt");
            }

            string SMSText = System.IO.File.ReadAllText(text);


            //var tomorrow = today.AddDays(1);
            //var yesterday = today.AddDays(-1);
            var today = DateTime.Today;
            //Replace the !Dagetning word with the date pickup
            SMSText = SMSText.Replace("!Dagsetning", today.AddDays(daysFromToday).ToString("dd.MM.yyyy"));
            return SMSText;

        }



        public void sixtySeconds(object sender, ElapsedEventArgs args)
        {


            // tengjast hinum gæjanum

            ChannelFactory<IService1> factory = new ChannelFactory<IService1>("guffusendir");
            IService1 channel;
            channel = factory.CreateChannel();
            string dataPackage = channel.GetData(22);
            //eventLog1.WriteEntry("Naðst hefur verið í " + dataPackage);







            //get clock hour right now
            DateTime now = DateTime.Now;
            int klukkutimi = now.Hour;

            //eventLog1.WriteEntry("klukkutimi: " + klukkutimi);
            //check if the clock is send o'clock
            if (klukkutimi == sendingHour)
            {
                
                if (!sentToday)
                {
                    // if havent send today, send it and make the variable true that it has been send
                    string smsTextToLog;
                    //create txt file that logs all of the sending activity;
                    createLogFile();


                    if (now.DayOfWeek == DayOfWeek.Friday || now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        smsTextToLog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skilabodTextiHelgi.txt");
                        smsTextToLog = System.IO.File.ReadAllText(smsTextToLog);
                        writeToLogFile("*****SkilaBoð*****");
                        writeToLogFile(smsTextToLog);
                        writeToLogFile("*****Logs*****");
                    }
                    else
                    {
                        smsTextToLog = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skilabodTextiVirkur.txt");
                        smsTextToLog = System.IO.File.ReadAllText(smsTextToLog);
                        writeToLogFile("*****SkilaBoð*****");
                        writeToLogFile(smsTextToLog);
                        writeToLogFile("*****Logs*****");
                    }    
                    
                    dailySend();


                    sentToday = true;
                }
            }
            else if (klukkutimi == 23)
            {
                if (sentToday)
                {
                    //make it ready for tomorrow
                    sentToday = false;
                }
            }



        }

        public void createLogFile()
        {
            string dagsetning = DateTime.Now.AddDays(1).ToString("d/M/yyyy");
            eventLog1.WriteEntry(dagsetning);
            dagsetning = dagsetning.Replace("/", ".");
            fileName = AppDomain.CurrentDomain.BaseDirectory + "/logs/" + dagsetning + ".txt";      
            
            if (!File.Exists(fileName))
            {

                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine("Logs fyrir " + dagsetning);
                }
            }
        }

        public void writeToLogFile(string text)
        {
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine(text);
            }
        }

        public void dailySend()
        {
            
            //connect to database
            string connetionString = null;
            SqlConnection cnn;
            connetionString = "Data Source=" + dataSource + "; Initial Catalog=" + databaseCatalog + "; Persist Security Info = True; User ID=" + databaseUsername + ";Password=" + databasePassword;
            cnn = new SqlConnection(connetionString);

            try
            {
                cnn.Open();

                DateTime now = DateTime.Now;
                // if its friday
                if (now.DayOfWeek == DayOfWeek.Friday)
                {

                    
                    //Send for saturday                    
                    string saturdayQuery = @"SELECT DELIVERYNAME, ORDERNAME, DELIVERYDATE FROM CECAKEORDERTABLE where DELIVERYDATE = FORMAT(GetDate()+1, 'yyyy-MM-dd');";
                    //define the SqlCommand object
                    SqlCommand saturdaycmd = new SqlCommand(saturdayQuery, cnn);
                    //execute the SQLCommand
                    SqlDataReader saturdaydr = saturdaycmd.ExecuteReader();


                    if (saturdaydr.HasRows)
                    {
                        // read all orders
                        while (saturdaydr.Read())
                        {
                            string phoneNumber = saturdaydr.GetString(0);
                            string clientName = saturdaydr.GetString(1);
                            string pontun = "nafn kunna: " + clientName + " simanumer: " + phoneNumber;

                            eventLog1.WriteEntry(pontun);


                            int n;

                            if (String.IsNullOrEmpty(phoneNumber))
                            {

                                writeToLogFile("Ekki sent þar sem það vantar símanúmer hjá: " + clientName);
                            }
                            else if (!int.TryParse(phoneNumber, out n))
                            {
                                writeToLogFile("Ekki sent vegna þessa að símanumer er ekki eingöngu tölustafir: " + phoneNumber + " kúnni: " + clientName);
                            }
                            else
                            {
                                string smsText = getSMSText(1);
                                sendSMS(smsText, Int32.Parse(phoneNumber));
                                writeToLogFile("Sent á " + clientName + " neð símanúmerið " + phoneNumber);
                                eventLog1.WriteEntry("sms sent á " + Int32.Parse(phoneNumber) + ", " + clientName + " texti:  " + smsText);

                            }



                        }

                        
                    }
                    saturdaydr.Close();

                    //Send for sunday

                    //retrieve the SQL Server instance version
                    string sundayQuery = @"SELECT DELIVERYNAME, ORDERNAME, DELIVERYDATE FROM CECAKEORDERTABLE where DELIVERYDATE = FORMAT(GetDate()+2, 'yyyy-MM-dd');";
                    //define the SqlCommand object
                    SqlCommand sundaycmd = new SqlCommand(sundayQuery, cnn);
                    //execute the SQLCommand
                    SqlDataReader sundaydr = sundaycmd.ExecuteReader();


                    eventLog1.WriteEntry("sunday send1");
                    if (sundaydr.HasRows)
                    {
                        eventLog1.WriteEntry("sunday send2");
                        // read all orders
                        while (sundaydr.Read())
                        {
                            string simanumer = sundaydr.GetString(0);
                            string nafnKunna = sundaydr.GetString(1);
                            string pontun = "nafn kunna: " + nafnKunna + " simanumer: " + simanumer;

                            eventLog1.WriteEntry(pontun);


                            int n;

                            if (String.IsNullOrEmpty(simanumer))
                            {
                                writeToLogFile("Ekki sent þar sem það vantar símanúmer hjá: " + nafnKunna);
                            }
                            else if (!int.TryParse(simanumer, out n))
                            {
                                writeToLogFile("Ekki sent vegna þessa að símanumer er ekki eingöngu tölustafir: " + simanumer + " kúnni: " + nafnKunna);
                            }
                            else
                            {
                                string smsText = getSMSText(2);
                                sendSMS(smsText, Int32.Parse(simanumer));
                                writeToLogFile("Sent á " + nafnKunna + " neð símanúmerið " + simanumer);
                                eventLog1.WriteEntry("sms sent á " + Int32.Parse(simanumer) + ", " + nafnKunna + " texti:  " + smsText);

                            }



                        }
                    }
                    sundaydr.Close();
                }
                else if (now.DayOfWeek == DayOfWeek.Saturday)
                {
                    //do nothing since everything was send on friday for sunday
                }
                else//sunday-thursday
                {
                    //retrieve the SQL Server instance version
                    string query = @"SELECT DELIVERYNAME, ORDERNAME, DELIVERYDATE FROM CECAKEORDERTABLE where DELIVERYDATE = FORMAT(GetDate()+1, 'yyyy-MM-dd');";
                    //define the SqlCommand object
                    SqlCommand cmd = new SqlCommand(query, cnn);
                    //execute the SQLCommand
                    SqlDataReader dr = cmd.ExecuteReader();



                    if (dr.HasRows)
                    {
                        // read all orders
                        while (dr.Read())
                        {
                            string simanumer = dr.GetString(0);
                            string nafnKunna = dr.GetString(1);
                            string pontun = "nafn kunna: " + nafnKunna + " simanumer: " + simanumer;

                            //eventLog1.WriteEntry(pontun);


                            int n;

                            if (String.IsNullOrEmpty(simanumer))
                            {

                                writeToLogFile("Ekki sent þar sem það vantar símanúmer hjá: " + nafnKunna);
                            }
                            else if (!int.TryParse(simanumer, out n))
                            {

                                writeToLogFile("Ekki sent vegna þessa að símanumer er ekki eingöngu tölustafir: " + simanumer + " kúnni: " + nafnKunna);                                
                            }
                            else
                            {
                                string smsText = getSMSText(1);
                                sendSMS(smsText, Int32.Parse(simanumer));
                                writeToLogFile("Sent á " + nafnKunna + " neð símanúmerið " + simanumer);                                
                                eventLog1.WriteEntry("sms sent á " + Int32.Parse(simanumer) + ", " + nafnKunna + " texti:  " + smsText);
                            }



                        }
                    }
                    else
                    {
                        //no orders
                        writeToLogFile("Engar Pantanir í dag :(");
                    }
                    dr.Close();
                }
                

                //if its friday send for sunday aswell
                if (now.DayOfWeek == DayOfWeek.Friday)
                {
                    
                }






                cnn.Close();
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.ToString());

            }
        }


        public void sendSMS(string texti, int numer)
        {
            string smsUrl = "https://vas2.c.is/sendsms?username=" + username + "&password=" + password + "&coding=2&from=" + from + "&to=354" + numer + "&text=" + texti;
           
            var client = new WebClient();
            client.DownloadDataCompleted += OnDownloadCompleted;
            client.DownloadDataAsync(new Uri(smsUrl)); // this makes a GET request



        }

        void OnDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            // when sms gets send out, check e for completion, exceptions, etc.

            bool isCancelled = e.Cancelled;
            if(isCancelled)
            {
                eventLog1.WriteEntry("cancelled");
            }
            else
            {
                eventLog1.WriteEntry("accepted");
                
            }

            

            byte[] data = (byte[])e.Result;
            string textData = System.Text.Encoding.UTF8.GetString(data);
            //eventLog1.WriteEntry(textData);
        }


    }
}
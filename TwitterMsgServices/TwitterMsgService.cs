using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using TweetSharp;

namespace TwitterMsgServices
{
    public partial class TwitterMsgService : ServiceBase
    {

        protected static DataSet ds_twitters;
        protected static List<long> []list_twitterid = { new List<long>(), new List<long>()};

        /* private static void OnTimedEvent(object source, ElapsedEventArgs e)
         {
             Console.WriteLine("Hello World!");
         }
         */
         //message content for sending
        public static string[] message = { @"You Won! 20% Discount on your next vacation.  
Thanks for the follow! 
Enter code: twitterfollower at checkout.
https://www.vacations-abroad.com"
,@"I know you work hard all year.
Tag your summer photos #exploretheworldundaunted to enter our $500 giveaway credit towards a Vacations-abroad.com reservation." };
        public static string[] tblname = { "TwitterFollower", "TwitterUserID"};
        public static void getFollowers( int index=0, string next_cursor="-1")
        {

            // JObject result_ids = CommonProvider.getFollowerIDs( index);
            List<long> result_ids = CommonProvider.getFollowerIDs(index, next_cursor);

            foreach (long row in result_ids)
            {
                if (!list_twitterid[index].Contains(row))
                {

                    //Send welcome message
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@twitterid", row));
                    param.Add(new SqlParameter("@tblname", tblname[index]));
                    CommonProvider.getDataSet("uspAddTwitterFollowerID", param);
                    list_twitterid[index].Add(row);
                    CommonProvider.WriteErrorLog(String.Format("{0}==> New user followed you. ID: {1}", index, row));
                    try
                    {
                        CommonProvider.sendDMMessage(row, message[index], index);
                    }catch(Exception ex)
                    {
                        CommonProvider.WriteErrorLog(String.Format("{0} ==>sending dm message error:  {1} ID:{2} ==>", index, ex.Message, row));
                    }

                }
            }
        }
        public TwitterMsgService()
        {
            InitializeComponent();
        }
        //when Service Starting , fires this function
        protected override void OnStart(string[] args)
        {
            CommonProvider.WriteErrorLog("Twilio service started");

            list_twitterid[0].Clear();
            list_twitterid[1].Clear();

            List<SqlParameter> param = new List<SqlParameter>();
            for(int i=0; i<2; i++)
            {
                param.Clear();
                param.Add(new SqlParameter("@tblname", tblname[i]));
                ds_twitters = CommonProvider.getDataSet("uspGetTwitterFollowerIDs", param);

                foreach (DataRow row in ds_twitters.Tables[0].Rows)
                {
                    try
                    {
                        list_twitterid[i].Add(long.Parse(row[0].ToString()));
                    }
                    catch { }
                }
            }
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 600000; // 600 seconds  
           // timer.Interval = 60000; // 600 seconds  
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            CommonProvider.WriteErrorLog("Twitter service ended");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.  
            for(int i=0;i< 2; i++)
            {
                getFollowers( i, "-1");
            }
            CommonProvider.WriteErrorLog("Twilio service working!!!");
        }

    }
}

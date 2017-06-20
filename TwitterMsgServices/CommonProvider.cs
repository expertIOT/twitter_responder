using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using TweetSharp;

namespace TwitterMsgServices
{
    public class TwitAuthenticateResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
    }

    class CommonProvider
    {
        public static string connString = "xxx";

        public static string []consumer_key = { "xxx", "xxx" };
        public static string []consumer_seckey = { "xx", "xxx" };
        public static string []access_token = { "xxx", "xx" };
        public static string []access_sectoken = { "xxx", "xxx" };
        public static long[] userids = {111, 111 };
        public static void WriteErrorLog(string error, Exception ex = null)
        {
            StreamWriter sw = null;
            if (ex == null) ex = new Exception();
            try
            {
                sw = new StreamWriter("C:\\inetpub\\wwwroot\\log.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + error);
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }
        }
        public static int getScalarValueFromDB(string proc_name, List<SqlParameter> proc_param)
        {
            int ret = 0;
            //  adapter.Fill(customers, "Customers");
            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    string sql = proc_name;

                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    foreach (SqlParameter proc_par in proc_param)
                    {
                        cmd.Parameters.Add(proc_par);
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Int32.TryParse(reader[0].ToString(), out ret);
                        break;
                    }
                    con.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;

        }
        public static DataSet getDataSet(string proc_name, List<SqlParameter> proc_param)
        {

            //  SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);

            DataSet inquiry_set = new DataSet();
            //  adapter.Fill(customers, "Customers");
            try
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        con.Open();
                        string sql = proc_name;

                        SqlCommand cmd = new SqlCommand(sql, con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter proc_par in proc_param)
                        {
                            cmd.Parameters.Add(proc_par);
                        }


                        adapter.SelectCommand = cmd;

                        adapter.Fill(inquiry_set, "Result");

                        con.Close();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return inquiry_set;
        }
        public static List<long> getFollowerIDs(int index=0, string next_cursor="")
        {

            // JObject result_ids = null;
            /*   public static JObject getFollowerIDs(string next_cursor ,int index=0)
             *           try
                       {


                           var oauth_consumer_key = consumer_key;
                           var oauth_consumer_secret = consumer_seckey;
                           //Token URL
                           var oauth_url = "https://api.twitter.com/oauth2/token";
                           var headerFormat = "Basic {0}";
                           var authHeader = string.Format(headerFormat,
                           Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oauth_consumer_key[index]) + ":" +
                           Uri.EscapeDataString((oauth_consumer_secret[index])))
                           ));

                           var postBody = "grant_type=client_credentials";

                           ServicePointManager.Expect100Continue = false;
                           HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url);
                           request.Headers.Add("Authorization", authHeader);
                           request.Method = "POST";
                           request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

                           using (Stream stream = request.GetRequestStream())
                           {
                               byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                               stream.Write(content, 0, content.Length);
                           }

                           request.Headers.Add("Accept-Encoding", "gzip");
                           HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                           Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                           JObject result_token;
                           TwitAuthenticateResponse twitAuthResponse;
                           using (var reader = new StreamReader(responseStream))
                           {
                               // JavaScriptSerializer js = new JavaScriptSerializer();
                               var objText = reader.ReadToEnd();
                               // result_token = JObject.Parse(objText);
                               twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objText);
                           }

                           //Get the follower ids

                           var url = "https://api.twitter.com/1.1/followers/ids.json?cursor=" + next_cursor + "&screen_name=vacationsabroad&skip_status=true&include_user_entities=false";
                           //var url = "https://api.twitter.com/1.1/direct_messages/new.json?text=hello%2C%20tworld.%20welcome%20to%201.1.&screen_name=andrew_lidev";

                           HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(url);
                           var timelineHeaderFormat = "{0} {1}";
                           timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
                           timeLineRequest.Method = "GET";
                           WebResponse timeLineResponse = timeLineRequest.GetResponse();
                           var timeLineJson = string.Empty;



                           using (timeLineResponse)
                           {
                               using (var reader = new StreamReader(timeLineResponse.GetResponseStream()))
                               {
                                   timeLineJson = reader.ReadToEnd();
                                   CommonProvider.WriteErrorLog(timeLineJson);
                                   result_ids = JObject.Parse(timeLineJson);

                               }
                           }
                       }
                       catch(Exception ex)
                       {
                           CommonProvider.WriteErrorLog(index +"===>" + ex.Message + " :Source Message" +ex.Source);
                       }
                       return result_ids;
                    */
            List<long> lstFollowers = new List<long>();
            try
            {
                var service = new TwitterService(consumer_key[index], consumer_seckey[index]);
                service.AuthenticateWith(access_token[index], access_sectoken[index]);

                long t_userid = userids[index];

                try
                {
                    TwitterUser tuSelf = service.GetUserProfile(
                    new GetUserProfileOptions() { IncludeEntities = false, SkipStatus = false });
                    t_userid = tuSelf.Id;
                }catch(Exception ex)
                {
                    WriteErrorLog(String.Format("{0}==>Used instead of userid. Get Profile Error;{1} source:{2} stack:{3}",index, ex.Message, ex.Source, ex.StackTrace));
                }

                //Console.WriteLine(String.Format("{0} {1} {2}", tuSelf.Id, tuSelf.ScreenName, tuSelf.FollowersCount));
                // return;
                // var options = new ListFollowersOptions { ScreenName = tuSelf.ScreenName };
                /*        ListFollowersOptions options = new ListFollowersOptions();
                             //   options.UserId = tuSelf.Id;
                                options.ScreenName = tuSelf.ScreenName;
                                options.IncludeUserEntities = false;
                                options.SkipStatus = true;
                                options.Cursor = -1;

                                List<TwitterUser> lstFollowers = new List<TwitterUser>();
                              TwitterCursorList<TwitterUser> followers = service.ListFollowers(options);
            */

                ListFollowerIdsOfOptions options = new ListFollowerIdsOfOptions();
                options.Cursor = -1;
                options.Count = 3000;
                options.UserId = t_userid;

                // if the API call did not succeed

                while (true)
                {
                    TwitterCursorList<long> followers = service.ListFollowerIdsOf(options);
                    //If the followers exists
                    if (followers == null)
                    {
                        WriteErrorLog(index + "===> there is no followers !! error");
                        break;
                    }
                    else
                    {
                        foreach (long user in followers)
                        {
                            // do something with the user (I'm adding them to a List)
                            lstFollowers.Add(user);
                        }
                    }

                    // if there are more followers
                    if (followers.NextCursor != null &&
                        followers.NextCursor != 0)
                    {
                        // then advance the cursor and load the next page of results
                        options.Cursor = followers.NextCursor;
                        followers = service.ListFollowerIdsOf(options);
                    }
                    // otherwise, we're done!
                    else
                        break;
                } 

            }catch(Exception ex)
            {
                WriteErrorLog(String.Format("{0}===>Error: {1} Sourc: {2}", index, ex.Message, ex.Source));
            }

            return lstFollowers;
        }
        public static void sendDMMessage(long id, string message, int index=0)
        {
            var service = new TwitterService(consumer_key[index], consumer_seckey[index]);
            service.AuthenticateWith(access_token[index], access_sectoken[index]);
            service.SendDirectMessage(new SendDirectMessageOptions() { UserId = id, Text = message });
            // service.ListFollowers(new ListFollowersOptions { Cursor = -1, ScreenName = "vacationsabroad", IncludeUserEntities = false, SkipStatus = true });
        }
    }
}

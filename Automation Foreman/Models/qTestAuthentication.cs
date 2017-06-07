using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Automation_Foreman.Models
{
    public class qTestAuthentication
    {

        // Get OAuthToken from qTest
        public string getQTestOAuthToken()
        {

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();
            string strIntegrationUser = ConfigurationManager.AppSettings["integrationUser"].ToString();
            string strIntegrationPassword = ConfigurationManager.AppSettings["integrationPassword"].ToString();
            string strResponse = "";
            string strOAuthToken = "";

            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            postData.Add(new KeyValuePair<string, string>("username", strIntegrationUser));
            postData.Add(new KeyValuePair<string, string>("password", strIntegrationPassword));

            HttpContent content = new FormUrlEncodedContent(postData);

            HttpResponseMessage responseJSON = new HttpResponseMessage();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", ConfigurationManager.AppSettings["qTestLoginAuth"]);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            try
            {

                responseJSON = client.PostAsync(qTestServer + "oauth/token", content).Result;
                responseJSON.EnsureSuccessStatusCode();
                strResponse = responseJSON.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(strResponse);
                strOAuthToken = data.token_type + " " + data.access_token;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return strOAuthToken;

        }

        public void revokeQTestOAuthToken(string strOAuthToken)
        {

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();

            HttpResponseMessage responseJSON = new HttpResponseMessage();

            // This call requires no body so this creates an empty content object.
            HttpContent content = new StringContent("");

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", strOAuthToken);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            try
            {

                responseJSON = client.PostAsync(qTestServer + "oauth/revoke", content).Result;
                responseJSON.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
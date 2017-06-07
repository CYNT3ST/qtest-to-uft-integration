using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Configuration;
using Automation_Foreman.Models;

namespace Automation_Foreman.Controllers
{
    public class StartTestOnAvailableLabMachineController : ApiController
    {

        private string mStrOAuthToken;

        // POST api/values
        public async Task<HttpResponseMessage> Post(JObject joAutomationParams)
        {
            DateTime oNow = DateTime.Now;

            UFTTestInfo oTestInfo = new UFTTestInfo();

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();

            oTestInfo.invoker = joAutomationParams["invoker"].ToString();
            oTestInfo.scriptPath = @joAutomationParams["scriptPath"].ToString();
            oTestInfo.testURL = joAutomationParams["testURL"].ToString();
            oTestInfo.testUsername = joAutomationParams["testUsername"].ToString();
            oTestInfo.testUserPassword = joAutomationParams["testUserPassword"].ToString();
            oTestInfo.testLabMachine = joAutomationParams["testLabMachine"].ToString();
            oTestInfo.projectID = joAutomationParams["projectID"].ToString();
            oTestInfo.testRunID = joAutomationParams["testRunID"].ToString();

            qTestAuthentication qTestOAuth = new qTestAuthentication();

            // Get a new qTest OAuth Token
            mStrOAuthToken = qTestOAuth.getQTestOAuthToken();

            HttpResponseMessage responseJSON = new HttpResponseMessage();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", mStrOAuthToken);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            try
            {
                client.Timeout = TimeSpan.FromSeconds(14400);  // Default to 4 hours due to long running script executions.
                responseJSON = await client.PostAsJsonAsync("http://" + oTestInfo.testLabMachine + ":81/api/UFTAutomation", oTestInfo);
                responseJSON.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Catch Errors here!
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // Update Run Results only if called from qTest
            if (oTestInfo.invoker == "qTest")
            {

                try
                {

                    string strResponseJSON = await responseJSON.Content.ReadAsStringAsync();

                    HttpResponseMessage responseQTest = new HttpResponseMessage();
                    responseQTest = await client.PostAsync(qTestServer + "api/v3/projects/" + oTestInfo.projectID + "/test-runs/" + oTestInfo.testRunID + "/auto-test-logs", new StringContent(strResponseJSON, Encoding.UTF8, "application/json"));
                    responseQTest.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    // Catch Errors here!
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return responseJSON;

        }

        public class UFTTestInfo
        {
            public string invoker { get; set; }
            public string projectID { get; set; }
            public string scriptPath { get; set; }
            public string testURL { get; set; }
            public string testUsername { get; set; }
            public string testUserPassword { get; set; }
            public string testLabMachine { get; set; }
            public string testRunID { get; set; }
        }

    }
}

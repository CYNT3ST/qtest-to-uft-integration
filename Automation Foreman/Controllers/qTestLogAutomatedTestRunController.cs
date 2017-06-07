using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Automation_Foreman.Models;
using Newtonsoft.Json.Linq;

namespace Automation_Foreman.Controllers
{
    public class qTestLogAutomatedTestRunController : ApiController
    {
        private string mStrOAuthToken;

        // POST api/qtestlogautomatedtestrun
        public async Task<HttpResponseMessage> Post(JObject joAutomatedRunResult)
        {

            DateTime oNow = DateTime.Now;

            HttpResponseMessage responseQTest = new HttpResponseMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            qTestAuthentication qTestOAuth = new qTestAuthentication();

            // Get a new qTest OAuth Token
            mStrOAuthToken = qTestOAuth.getQTestOAuthToken();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", mStrOAuthToken);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            qTestExecutionPost qTestExecution = new qTestExecutionPost();
            qTestExecution.name = joAutomatedRunResult["TestName"].ToString();
            qTestExecution.status = joAutomatedRunResult["ExecutionStatus"].ToString();
            qTestExecution.exe_end_date = oNow.ToString("s") + oNow.ToString("zzz");
            qTestExecution.exe_start_date = oNow.ToString("s") + oNow.ToString("zzz");
            qTestExecution.note = joAutomatedRunResult["ExecutionNote"].ToString();

            string strTestRunURL = joAutomatedRunResult["TestRunURL"].ToString();

            try
            {

                responseQTest = await client.PostAsJsonAsync<qTestExecutionPost>(strTestRunURL, qTestExecution);
                responseQTest.EnsureSuccessStatusCode();

                response.Content = new StringContent("{\"TestRunAdded\":\"Passed\"}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                response.Content = new StringContent("{\"TestRunAdded\":\"Failed\",\"ErrorMessage\":\"" + ex.Message + "\"}");

            }

            // Revoke the qTest OAuth Token to end this current 
            qTestOAuth.revokeQTestOAuthToken(mStrOAuthToken);

            response.StatusCode = HttpStatusCode.OK;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;

        }

    }

    public class qTestExecutionPost
    {
        public string name { get; set; }
        public string status { get; set; }
        public string exe_start_date { get; set; }
        public string exe_end_date { get; set; }
        public string note { get; set; }
    }

}

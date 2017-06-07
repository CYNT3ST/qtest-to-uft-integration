using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Automation_Foreman.Controllers
{
    public class StartTestsFromQTestController : ApiController
    {

        // POST api/values
        public async Task<HttpResponseMessage> Post(JArray joAutomationParams)
        {
            int intTestCnt = 0;
            HttpResponseMessage response = new HttpResponseMessage();
            HttpResponseMessage responseJSON = new HttpResponseMessage();
            HttpClient client = new HttpClient();

            Collection<UFTTestInfo> colTestInfo = new Collection<UFTTestInfo>();
            UFTTestInfo uftTestInfo = null;

            for (int intParamCnt = 0; intParamCnt < joAutomationParams.Count; intParamCnt++)
            {

                switch (joAutomationParams[intParamCnt]["name"].ToString())
                {

                    case "ExecuteTest":

                        if (joAutomationParams[intParamCnt]["value"].ToString() == "on")
                        {

                            intTestCnt++;
                            uftTestInfo = new UFTTestInfo();

                        }
                        else
                        {
                            uftTestInfo = null;
                        }

                        break;

                    case "invoker":

                        if (uftTestInfo != null)
                        {
                            uftTestInfo.invoker = joAutomationParams[intParamCnt]["value"].ToString();
                        }

                        break;

                    case "projectID":

                        if (uftTestInfo != null)
                        {
                            uftTestInfo.projectID = joAutomationParams[intParamCnt]["value"].ToString();
                        }

                        break;

                    case "scriptPath":

                        if (uftTestInfo != null)
                        {
                            uftTestInfo.scriptPath = joAutomationParams[intParamCnt]["value"].ToString();
                        }

                        break;

                    case "testURL":

                        uftTestInfo.testURL = "";

                        break;

                    case "testUsername":

                        uftTestInfo.testUsername = "";

                        break;

                    case "testUserPassword":

                        uftTestInfo.testUserPassword = "";

                        break;

                    case "testLabMachine":

                        if (uftTestInfo != null)
                        {
                            uftTestInfo.testLabMachine = joAutomationParams[intParamCnt]["value"].ToString();
                        }

                        break;

                    case "testRunID":

                        if (uftTestInfo != null)
                        {
                            uftTestInfo.testRunID = joAutomationParams[intParamCnt]["value"].ToString();

                            colTestInfo.Add(uftTestInfo);
                            uftTestInfo = null;

                        }

                        break;

                }

                System.Diagnostics.Debug.WriteLine(joAutomationParams[intParamCnt]["name"].ToString() + " " + joAutomationParams[intParamCnt]["value"].ToString());

            }

            System.Diagnostics.Debug.WriteLine(colTestInfo.Count);
            client.Timeout = TimeSpan.FromSeconds(14400 * colTestInfo.Count); // 4 Hours * the number of tests that will be executed since any one test might take up to 4 hours.  This will be re-visited in the future to try and make the script be only a single scenario.

            for (int intUFTTestCnt = 0; intUFTTestCnt < colTestInfo.Count; intUFTTestCnt++)
            {

                try
                {
                    System.Diagnostics.Debug.WriteLine(this.Url.Link("Default", new { Controller = "api", Action = "StartTestOnAvailableLabMachine" }));
                    // The this.URL.Link allows us to not have to hard code the server name for this service call.
                    responseJSON = await client.PostAsJsonAsync<UFTTestInfo>(this.Url.Link("Default", new { Controller = "api", Action = "StartTestOnAvailableLabMachine" }), colTestInfo[intUFTTestCnt]);
                    responseJSON.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    // Catch Errors here!
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }

            response.StatusCode = responseJSON.StatusCode;//HttpStatusCode.OK;
            response.Content = responseJSON.Content;

            // PREVENT CACHING

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;

        }

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

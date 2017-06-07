using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using QuickTest; //QTObjectModel.dll
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace Automation_Worker.Controllers
{
    public class UFTAutomationController : ApiController
    {

        public Application qtApp;

        public HttpResponseMessage Get()
        {
            InitializeUFTApplication();

            UFTStatus UFTIsBusy;

            if (qtApp != null)
            {
                if (qtApp.Launched == true)
                {
                    if (qtApp.Test != null)
                    {
                        UFTIsBusy = new UFTStatus() { isBusy = qtApp.Test.IsRunning, Username = qtApp.Test.Environment("UserName") };
                    }
                    else
                    {
                        UFTIsBusy = new UFTStatus() { isBusy = false, Username = qtApp.Test.Environment("UserName") };
                    }

                }
                else
                {
                    UFTIsBusy = new UFTStatus() { isBusy = false, Username = "" };
                }

            }
            else
            {
                UFTIsBusy = new UFTStatus() { isBusy = false, Username = "" };
            }

            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent(new JavaScriptSerializer().Serialize(UFTIsBusy));
            return response;

        }

        // POST api/values
        public async Task<HttpResponseMessage> Post(JObject joAutomationParams)
        {

            var response = new HttpResponseMessage();

            try
            {
                InitializeUFTApplication();

                string strScriptPath;
                string strTestName;
                string strLastStatus;
                string strTestURL;
                string strTestUsername;
                string strTestUserPassword;
                object objAssociatedAddins;
                object objAddinError;
                DateTime oNow = DateTime.Now;
                Test qtTest;

                strScriptPath = joAutomationParams["scriptPath"].ToString();
                strTestURL = joAutomationParams["testURL"].ToString();
                strTestUsername = joAutomationParams["testUsername"].ToString();
                strTestUserPassword = joAutomationParams["testUserPassword"].ToString();

                if (strTestURL != "")
                {
                    // Create the variables.xml file that is used by UFT
                    XmlDocument doc = new XmlDocument();
                    XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    doc.AppendChild(docNode);

                    XmlNode environmentNode = doc.CreateElement("Environment");
                    doc.AppendChild(environmentNode);

                    // Create the testURL Variable
                    XmlNode variableNode = doc.CreateElement("Variable");
                    environmentNode.AppendChild(variableNode);

                    XmlNode nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = "testURL";
                    variableNode.AppendChild(nameNode);

                    XmlNode valueNode = doc.CreateElement("Value");
                    XmlNode cdataNode = doc.CreateCDataSection(strTestURL);
                    valueNode.AppendChild(cdataNode);

                    variableNode.AppendChild(valueNode);

                    // Create the testUsername Variable
                    variableNode = doc.CreateElement("Variable");
                    environmentNode.AppendChild(variableNode);

                    nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = "testUsername";
                    variableNode.AppendChild(nameNode);

                    valueNode = doc.CreateElement("Value");
                    cdataNode = doc.CreateCDataSection(strTestUsername);
                    valueNode.AppendChild(cdataNode);

                    variableNode.AppendChild(valueNode);

                    // Create the testUserPassword
                    variableNode = doc.CreateElement("Variable");
                    environmentNode.AppendChild(variableNode);

                    nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = "testUserPassword";
                    variableNode.AppendChild(nameNode);

                    valueNode = doc.CreateElement("Value");
                    cdataNode = doc.CreateCDataSection(strTestUserPassword);
                    valueNode.AppendChild(cdataNode);

                    variableNode.AppendChild(valueNode);

                    if (Directory.Exists(@"C:\automation_temp\environment\") == false)
                    {
                        Directory.CreateDirectory(@"C:\automation_temp\environment\");
                    }

                    doc.Save(@"C:\automation_temp\environment\variables.xml");

                }

                qtApp.UseLicenseOfType(tagUnifiedLicenseType.qtUnifiedFunctionalTesting);

                objAssociatedAddins = qtApp.GetAssociatedAddinsForTest(strScriptPath);
                qtApp.SetActiveAddins(ref objAssociatedAddins, out objAddinError);

                qtApp.Launch(); // Start QuickTest

                qtApp.Visible = true; // Make it visible

                qtApp.Open(strScriptPath);

                qtTest = qtApp.Test;
                System.Diagnostics.Debug.WriteLine(oNow.ToString());
                qtTest.Run(null, true, null);
                System.Diagnostics.Debug.WriteLine(oNow.ToString());
                //qtApp.Test.Run();

                strTestName = qtTest.Name;

                switch (qtTest.LastRunResults.Status.ToLower())
                {
                    case "passed":
                        strLastStatus = "PASS";
                        break;

                    case "failed":
                        strLastStatus = "FAIL";
                        break;

                    default:
                        strLastStatus = "SKIP";
                        break;
                }

                string strLastRunPath = qtTest.LastRunResults.Path;

                var qTestExecution = new qTestExecutionPost() { name = strTestName, status = strLastStatus, exe_end_date = oNow.ToString("s") + oNow.ToString("zzz"), exe_start_date = oNow.ToString("s") + oNow.ToString("zzz"), note = strLastRunPath };

                //qtApp.Test.Close();
                //qtTest.Close();

                qtApp.Quit();

                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(new JavaScriptSerializer().Serialize(qTestExecution));

            }

            catch (Exception ex)
            {

                response.StatusCode = HttpStatusCode.SeeOther;
                response.Content = new StringContent("Error: " + ex.InnerException.Message.ToString());

            }

            return response;

        }

        private void InitializeUFTApplication()
        {
            if (qtApp == null)
            {
                qtApp = new Application();
            }

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

    public class UFTStatus
    {
        public bool isBusy { get; set; }
        public string Username { get; set; }
    }

}

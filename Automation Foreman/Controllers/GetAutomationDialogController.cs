using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Automation_Foreman.Models;
using Newtonsoft.Json.Linq;

namespace Automation_Foreman.Controllers
{
    public class QTestPage
    {
        public string qTestObjectType { get; set; }
        public string qTestProjectID { get; set; }
        public string qTestObjectID { get; set; }
    }

    public class GetAutomationDialogController : ApiController
    {

        private string mStrOAuthToken;

        // GET api/values
        public async Task<HttpResponseMessage> Get([FromUri] QTestPage qTestPage)
        {
            int intCurRow = 0;
            HtmlTableRow trAutomatedTest = null;
            string strProjectID = qTestPage.qTestProjectID;
            string strObjectID = qTestPage.qTestObjectID;
            qTestAuthentication qTestOAuth = new qTestAuthentication();

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();

            // Get a new qTest OAuth Token
            mStrOAuthToken = qTestOAuth.getQTestOAuthToken();

            HttpResponseMessage responseAutomationDialog = new HttpResponseMessage();

            if (mStrOAuthToken != "")
            {

                HttpResponseMessage response = new HttpResponseMessage();
                HttpResponseMessage responseJSON = new HttpResponseMessage();

                HtmlElement htmlRoot = new HtmlElement();
                HtmlHead htmlHead = new HtmlHead();
                HtmlTitle htmlTitle = new HtmlTitle();
                htmlTitle.Text = "Execute Automated Tests";

                htmlHead.Controls.Add(htmlTitle);
                htmlRoot.Controls.Add(htmlHead);

                HtmlTable tblExecutionList = new HtmlTable();
                HtmlTableRow trHeader;
                HtmlTableCell cellHeader;

                StringBuilder sb = new StringBuilder();
                StringWriter tw = new StringWriter(sb);
                HtmlTextWriter hw = new HtmlTextWriter(tw);

                trHeader = new HtmlTableRow();
                cellHeader = new HtmlTableCell();
                cellHeader.InnerText = "Execute";
                cellHeader.Attributes.Add("class", "tableHeader centerText");
                trHeader.Cells.Add(cellHeader);

                cellHeader = new HtmlTableCell();
                cellHeader.InnerText = "Run Order";
                cellHeader.Attributes.Add("class", "tableHeader centerText");
                trHeader.Cells.Add(cellHeader);

                cellHeader = new HtmlTableCell();
                cellHeader.InnerText = "Lab Machine";
                cellHeader.Attributes.Add("class", "tableHeader centerText labMachineCol");
                trHeader.Cells.Add(cellHeader);

                cellHeader = new HtmlTableCell();
                cellHeader.InnerText = "Test Script Name";
                cellHeader.Attributes.Add("class", "tableHeader testScriptNameCol");
                trHeader.Cells.Add(cellHeader);

                cellHeader = new HtmlTableCell();
                cellHeader.InnerText = "Test Script Path";
                cellHeader.Attributes.Add("class", "tableHeader testScriptPathCol");
                trHeader.Cells.Add(cellHeader);

                tblExecutionList.Rows.Add(trHeader);

                HttpResponseMessage responseTestRuns = new HttpResponseMessage();
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", mStrOAuthToken);
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

                try
                {

                    switch (qTestPage.qTestObjectType)
                    {
                        case "2":   // TEST SUITE

                            responseTestRuns = await client.GetAsync(qTestServer + "api/v3/projects/" + strProjectID + "/test-runs?parentId=" + strObjectID + "&parentType=test-suite");
                            responseTestRuns.EnsureSuccessStatusCode();
                            string strTestRuns = await responseTestRuns.Content.ReadAsStringAsync();

                            for (int intTestRun = 0; intTestRun < JArray.Parse(strTestRuns).Count; intTestRun++)
                            {
                                trAutomatedTest = await AddTestRun(strProjectID, (string)JArray.Parse(strTestRuns)[intTestRun]["id"], intTestRun);

                                if (trAutomatedTest != null)
                                {
                                    if (intCurRow % 2 == 1)
                                    {
                                        trAutomatedTest.Attributes.Add("class", "tableAltRow");
                                    }
                                    tblExecutionList.Rows.Add(trAutomatedTest);
                                    intCurRow++;
                                }

                            }

                            break;

                        case "3":   // TEST RUN

                            trAutomatedTest = await AddTestRun(strProjectID, strObjectID, 0);

                            if (trAutomatedTest != null)
                            {
                                tblExecutionList.Rows.Add(trAutomatedTest);
                            }

                            break;

                    }

                    tblExecutionList.RenderControl(hw);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                responseAutomationDialog.Content = new StringContent("<html><head><title>Execute Automated Tests</title><link rel='shortcut icon' href='../../images/execute_automation.ico' type='image/x-icon' /><link rel='stylesheet' type='text/css' href='../../css/base_style.css' /><script type='text/javascript' src='../../scripts/jquery-1.10.2.min.js'></script><script type='text/javascript' src='../../scripts/automation_tools.js'></script></head><body><form id='frmExecuteTests'>" + sb.ToString() + "<input type='button' value='Execute Scripts' onClick='executeTestRuns()' /><div id='status'></div></form></body></html>");

                // Revoke the qTest OAuth Token to end this current session 
                qTestOAuth.revokeQTestOAuthToken(mStrOAuthToken);

            }
            else
            {

                responseAutomationDialog.Content = new StringContent("<html><head><title>Execute Automated Tests</title><link rel='shortcut icon' href='../../images/execute_automation.ico' type='image/x-icon' /><link rel='stylesheet' type='text/css' href='../../css/base_style.css' /><script type='text/javascript' src='../../scripts/jquery-1.10.2.min.js'></script><script type='text/javascript' src='../../scripts/automation_tools.js'></script></head><body><table><tr><td class='tableHeader centerText'>Error</td></tr><tr><td class='tableCell centerText'>qTest getQTestOAuthToken action failed!</td></tr><tr><td class='tableCell centerText'>Please close this dialog and relaunch the test runner.</td></tr></table></body></html>");

            }

            responseAutomationDialog.StatusCode = HttpStatusCode.OK;
            responseAutomationDialog.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return responseAutomationDialog;

        }

        private DropDownList BuildLabDropdown(int intRowNumber)
        {
            DropDownList ddlLabMachines;
            ListItem liLabMachine;

            string[] arrLabMachine = ConfigurationManager.AppSettings["labMachines"].Split(';');

            ddlLabMachines = new DropDownList();
            ddlLabMachines.ID = "testLabMachine";

            for (int intLabCnt = 0; intLabCnt < arrLabMachine.Length; intLabCnt++)
            {

                liLabMachine = new ListItem();
                liLabMachine.Value = arrLabMachine[intLabCnt];
                liLabMachine.Text = arrLabMachine[intLabCnt];
                ddlLabMachines.Items.Add(liLabMachine);

            }

            return ddlLabMachines;

        }

        private async Task<HtmlTableRow> AddTestRun(string strProjectID, string strTestRunID, int intTestRun)
        {
            int intRowNumber;
            bool blnAddAutomatedTest = false;

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();

            HttpResponseMessage responseTestRun = new HttpResponseMessage();
            HttpResponseMessage responseTestCases = new HttpResponseMessage();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", mStrOAuthToken);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            HtmlTableRow trAutomatedTest = null;
            HtmlTableCell cellCheckbox;
            HtmlInputCheckBox chkExecuteTest;
            HtmlTableCell cellRunOrder;
            HtmlTableCell cellDropDownList;
            DropDownList ddlLabMachine;
            HtmlTableCell cellTestName;
            HtmlTableCell cellTestPath;
            HtmlInputHidden inputTestPath;
            HtmlInputHidden inputTestRunID;
            HtmlInputHidden inputInvoker;
            HtmlInputHidden inputProjectID;

            intRowNumber = intTestRun + 1;

            responseTestRun = await client.GetAsync(qTestServer + "api/v3/projects/" + strProjectID + "/test-runs/" + strTestRunID);
            responseTestRun.EnsureSuccessStatusCode();
            string strTestRun = await responseTestRun.Content.ReadAsStringAsync();

            trAutomatedTest = new HtmlTableRow();
            cellCheckbox = new HtmlTableCell();
            chkExecuteTest = new HtmlInputCheckBox();
            chkExecuteTest.ID = "ExecuteTest";
            chkExecuteTest.Checked = true;
            cellCheckbox.Controls.Add(chkExecuteTest);
            cellCheckbox.Attributes.Add("class", "tableCell centerText");

            cellRunOrder = new HtmlTableCell();
            cellRunOrder.InnerText = intRowNumber.ToString();
            cellRunOrder.Attributes.Add("class", "tableCell centerText");

            cellDropDownList = new HtmlTableCell();
            ddlLabMachine = BuildLabDropdown(intRowNumber);
            ddlLabMachine.Attributes.Add("class", "tableCell centerText");
            cellDropDownList.Controls.Add(ddlLabMachine);

            trAutomatedTest.Cells.Add(cellCheckbox);
            trAutomatedTest.Cells.Add(cellRunOrder);
            trAutomatedTest.Cells.Add(cellDropDownList);

            cellTestName = new HtmlTableCell();
            cellTestName.InnerText = JObject.Parse(strTestRun)["name"].ToString();
            cellTestName.Attributes.Add("class", "tableCell");
            trAutomatedTest.Cells.Add(cellTestName);

            for (int intLink = 0; intLink < JArray.Parse(JObject.Parse(strTestRun)["links"].ToString()).Count; intLink++)
            {

                if (JObject.Parse(strTestRun)["links"][intLink]["rel"].ToString() == "test-case")
                {
                    responseTestCases = await client.GetAsync(JObject.Parse(strTestRun)["links"][intLink]["href"].ToString());
                    responseTestCases.EnsureSuccessStatusCode();
                    string strTestCase = await responseTestCases.Content.ReadAsStringAsync();

                    for (int intProperty = 0; intProperty < JArray.Parse(JObject.Parse(strTestCase)["properties"].ToString()).Count; intProperty++)
                    {

                        //switch (JObject.Parse(strTestCase)["properties"][intProperty]["field_id"].ToString())
                        switch (getQTestFieldLabelFromId(JObject.Parse(strTestCase)["properties"][intProperty]["field_id"].ToString(), strProjectID))

                        {
                            case "Automation Content":  //Automation Content (-61)

                                if (JObject.Parse(strTestCase)["properties"][intProperty]["field_value"] != null)
                                {
                                    cellTestPath = new HtmlTableCell();
                                    inputTestPath = new HtmlInputHidden();
                                    inputTestPath.Value = JObject.Parse(strTestCase)["properties"][intProperty]["field_value"].ToString();
                                    inputTestPath.ID = "scriptPath";
                                    cellTestPath.InnerText = JObject.Parse(strTestCase)["properties"][intProperty]["field_value"].ToString();
                                    cellTestPath.Controls.Add(inputTestPath);

                                    inputInvoker = new HtmlInputHidden();
                                    inputInvoker.Value = "qTest";
                                    inputInvoker.ID = "invoker";
                                    cellTestPath.Controls.Add(inputInvoker);

                                    inputProjectID = new HtmlInputHidden();
                                    inputProjectID.Value = strProjectID;
                                    inputProjectID.ID = "projectID";
                                    cellTestPath.Controls.Add(inputProjectID);

                                    inputTestRunID = new HtmlInputHidden();
                                    inputTestRunID.Value = strTestRunID;
                                    inputTestRunID.ID = "testRunID";
                                    cellTestPath.Controls.Add(inputTestRunID);
                                    cellTestPath.Attributes.Add("class", "tableCell");
                                    trAutomatedTest.Cells.Add(cellTestPath);
                                }

                                break;

                            case "Automation": // QTest Automation Checkbox (-60)

                                if (JObject.Parse(strTestCase)["properties"][intProperty]["field_value"].ToString() == "711")  // 711 = "Yes"
                                {
                                    blnAddAutomatedTest = true;
                                }
                                else
                                {
                                    blnAddAutomatedTest = false;
                                }

                                break;

                        }

                    }
                }

            }

            if (blnAddAutomatedTest == false)
            {
                trAutomatedTest = null;
            }

            return trAutomatedTest;

        }

        private string getQTestFieldLabelFromId(string strFieldId, string strProjectId)
        {
            string strFieldLabel = "";
            string strResponse;
            HttpResponseMessage responseJSON = new HttpResponseMessage();

            string qTestServer = ConfigurationManager.AppSettings["qTestServer"].ToString();

            HttpResponseMessage responseTestRun = new HttpResponseMessage();
            HttpResponseMessage responseTestCases = new HttpResponseMessage();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", mStrOAuthToken);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            qTestFields oFields = new qTestFields();

            responseJSON = client.GetAsync(qTestServer + "api/v3/projects/" + strProjectId + "/settings/test-cases/fields").Result;
            responseJSON.EnsureSuccessStatusCode();
            strResponse = responseJSON.Content.ReadAsStringAsync().Result;

            oFields.fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<qTestFieldModel>>(strResponse);

            foreach (qTestFieldModel oField in oFields.fields)
            {
                if (oField.id == strFieldId)
                {
                    strFieldLabel = oField.label;
                }
            }

            return strFieldLabel;
        }

    }

}

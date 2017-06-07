//-----------------------------------------------------------
//| Execute qTest Test Runs                                 |
//-----------------------------------------------------------
//| This chrome extension allows the user to execute a UFT  |
//| automated test script on a test lab machine and return  |
//| the execution response to qTest for reporting purposes  |
//-----------------------------------------------------------
//| Author:         Aaron M. Mann                           |
//| Copyright:      August 27, 2015                         |
//-----------------------------------------------------------
chrome.browserAction.onClicked.addListener(function (tab) {

    chrome.tabs.query({ currentWindow: true, active: true }, function (tabs) {

	const automation_foreman_getautomationdialog = 'http://<yourcompany.com>/api/GetAutomationDialog/';

        // Pulls project id and test suite id from the current url in chrome.
        var strURL = tabs[0].url;
        var strURLParts = strURL.toString().split("/");
        var strProjectID = strURLParts[4];
        var strTestRunParts = strURLParts[strURLParts.length - 1].split("=")
        var strObjectType = strTestRunParts[strTestRunParts.length - 2].replace("&id", "");
        var strObjectID = strTestRunParts[strTestRunParts.length - 1];

        // Sets the height and width of the popup window.
        var height = 768;
        var width = 1024;

        // Gets the launch position for the popup window.
        var left = (screen.width / 2) - (width / 2);
        var top =  (screen.height / 2) - (height / 2);

        // Launch the popup window and open the Automation Dialog window and sets the blnAutomationDialog value to true.
        // NOTE:  The reason for the local storage is to allow a refresh of the current page to fire but only after the popup has been launched.
        chrome.windows.create({ 'url': automation_foreman_getautomationdialog + '?qTestObjectType=' + strObjectType + '&qTestProjectID=' + strProjectID + '&qTestObjectID=' + strObjectID, 'type': 'popup', 'top': top, 'left': left, 'width': width, 'height': height }, function (window) { chrome.storage.local.set({ 'blnAutomationDialog':'true' }); });

    });

});

// Set the blnAutomationDialog local storage variable to false since the dialog wasn't launched.
chrome.tabs.onCreated.addListener(function (tab) {
    chrome.storage.local.set({ 'blnAutomationDialog': 'false' });
});

// Check that the automation dialog was just closed and refresh qTest to show the execution results to the user.
chrome.tabs.onRemoved.addListener(function(tabId, removeInfo) {

    chrome.storage.local.get('blnAutomationDialog', function (result) {

        if (result.blnAutomationDialog == 'true') {
            chrome.tabs.getSelected(null, function (tab) {
                blnAutomationDialog = false;
                var code = 'window.location.reload();';
                chrome.tabs.executeScript(tab.id, { code: code });
            });
        }
    });

});
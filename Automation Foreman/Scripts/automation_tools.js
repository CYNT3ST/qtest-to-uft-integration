function executeTestRuns() {
    // Get the status field that will be used to communicate process status.
    var divStatus = jQuery("#status");
    // Get the FormData and convert to JSON serialization.
    var formData = JSON.stringify(jQuery('#frmExecuteTests').serializeArray());

    // Construct an HTTP request
    var xhr = new XMLHttpRequest();
    divStatus.text("Processing...");
    xhr.open("POST", $(location).attr('protocol') + "//" + $(location).attr('host') + "/api/StartTestsFromQTest", true);

    xhr.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');

    // Send the collected data as JSON
    xhr.send(formData);

    xhr.onloadend = function () {
        // done
        divStatus.text("Test Execution Complete!");
    }

}
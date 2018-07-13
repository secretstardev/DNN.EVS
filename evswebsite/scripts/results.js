var results = function () {
    var endPointURL = '{service url}'; //'http://localhost:81';
    var fileId = '';
    var cookieName = 'evs_fileid';
    var queryStringName = 'fileId';
    
    function load() {
        fileId = getFileId();
        getResults();
    }
    
    function getResults() {
        $.ajax({
            type: "GET",
            async: true,
            url: endPointURL + '/api/Extension?fileid=' + fileId,
            dataType: 'json',
            error: function () {
                alert('ERROR!');
            },
            success: function (results) {
                resultCallBack(results);
            }
        });
    }
    
    function emailResults() {
        $("#submit").attr("disabled", "disabled");
        $("#email").attr("disabled", "disabled");

        var emailAddress = $("#email").val();
        
        //alert("About to send email to: " + emailAddress + " for fileId: " + fileId);
        
        $.ajax({
            type: "POST",
            async: true,
            url: endPointURL + '/api/SendResults/?fileid=' + fileId + '&email=' + emailAddress,
            error: function () {
                alert('There was an issue sending the email, please try again later.');
                
                $("#submit").removeAttr("disabled");
                $("#email").removeAttr("disabled");
            },
            success: function (results) {
                sendEmailCallBack(results);
            }
        });
        
        $("#submit").removeAttr("disabled");
        $("#email").removeAttr("disabled");
    }
    
    function sendEmailCallBack(results) {
        alert('The email has been sent to ' + $("#email").val());
        $("#submit").removeAttr("disabled");
        $("#email").removeAttr("disabled");
        $("#email").val("");
    }
    
    function resultCallBack(results) {
        
        function resultViewModel() {
            this.extension = results;
            this.extensionDetails = results.ExtensionDetails;
            this.extensionErrorMessages = getMessages(results.ExtensionMessages, 1);
            this.extensionWarnMessages = getMessages(results.ExtensionMessages, 2);
            this.extensionInfoMessages = getMessages(results.ExtensionMessages, 3);
            this.extensionSysMessages = getMessages(results.ExtensionMessages, 4);
        }

        ko.applyBindings(new resultViewModel());
        
        $('#errorCount').text(getMessages(results.ExtensionMessages, 1).length);
        $('#warningCount').text(getMessages(results.ExtensionMessages, 2).length);
        $('#infoCount').text(getMessages(results.ExtensionMessages, 3).length);
        $('#sysCount').text(getMessages(results.ExtensionMessages, 4).length);

        var link = "http://evs.dnnsoftware.com/results.htm?fileId=" + results.FileID;

        $('#dlResultsCsv').attr("href", endPointURL + "/api/DownloadResults/?fileId=" + results.FileID + "&type=csv");
        $('#dlResultsXlsx').attr("href", endPointURL + "/api/DownloadResults/?fileId=" + results.FileID + "&type=xlsx");
        $('#dlResultsXml').attr("href", endPointURL + "/api/DownloadResults/?fileId=" + results.FileID + "&type=xml");
        
        $('#fbLink').attr("href", "http://www.facebook.com/share.php?u=" + link);
        $('#liLink').attr("href", "https://www.linkedin.com/cws/share?url=" + link);
        $('#tLink').attr("href", "http://twitter.com/home?status=" + "I just tested my extension using EVS: " + link);

        $('#appBody').show();
        
        var validator = $("#sendResults").validate();

        $("#messages").accordion({
            active: false,
            collapsible: true
        });

        $(".ui-accordion-content").css('height', '');

        if (results.SQLAzureScriptsURI.length > 0) {
            $("#azureOutput").show();
            $("#detailsOuter").css('height', '430px');
        }

        $("#sendResults").submit(function () {
            if (validator.form()) {
                emailResults();
            }
            return false;
        });
    }
    
    function getMessages(messages, messageTypeId) {
        var output = [];

        $.each(messages, function(i, data) {
            if (data.MessageTypeID == messageTypeId) {
                output.push(data);
            }
        });

        return output;
    }
    
    function getFileId() {
        var retval = getParameterByName(queryStringName);
        
        if (retval == '') {
            if ($.cookie(cookieName) != null) {
                retval = $.cookie(cookieName);
            }
        }

        return retval;
    }
    
    function getParameterByName(name) {
        name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regexS = "[\\?&]" + name + "=([^&#]*)";
        var regex = new RegExp(regexS);
        var results = regex.exec(window.location.search);
        if (results == null)
            return "";
        else
            return decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    return {
        load: load
    };
}();

jQuery(function () {
    results.load();
});
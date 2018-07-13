jQuery(function () {
    if (!supportAjaxUploadWithProgress()) {
        $('#appUpload').hide();
        $('#appOldBrowser').show();
    } else {
        if (!checkForOrAddUserCookie()) {
            alert('You do not have a valid user cookie.');
        }
    }
});

function supportAjaxUploadWithProgress() {
    return supportFileAPI() && supportAjaxUploadProgressEvents();
};

function supportFileAPI() {
    var fi = document.createElement('INPUT');
    fi.type = 'file';
    return 'files' in fi;
};

function supportAjaxUploadProgressEvents() {
    var xhr = new XMLHttpRequest();
    return !! (xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
};

function checkForOrAddUserCookie() {
    var endPointURL = '{service url}'; //'http://localhost:81'; 

    if ($.cookie('evs_user') == null) {
        $.ajax({
            type: "GET",
            async: true,
            url: endPointURL + '/api/UserId',
            dataType: 'json',
            error: function() {
                alert('Could not fetch a new user guid.');
                return false;
            },
            success: function(userObject) {
                $.cookie('evs_user', userObject.UserId, { path: '/', expires: 10 });
                return true;
            }
        });
    } else {
        var guid = $.cookie('evs_user');

        return isGuidValid(guid);
    }

    return true;
};

function isGuidValid(value) {
    rGx = new RegExp("\\b(?:[a-fA-F0-9]{8})(?:-[a-fA-F0-9]{4}){3}-(?:[a-fA-F0-9]{12})\\b");
    return rGx.exec(value) != null;
};
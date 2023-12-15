function sendBonusesToUsers() {
    debugger
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);
    var data = {};
    var ListOfUserId = $('#userIds').val();
    if (ListOfUserId != "0") {
        let userDetails = JSON.stringify(data);
        $.ajax({
            type: 'Post',
            url: '/Bonus/PayRegBonuses',
            dataType: 'json',
            data:
            {
                userIds: ListOfUserId,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Bonus/Index';
                    successAlertWithRedirect(result.msg,url);
                    $('#submit_btn').html(defaultBtnValue);
                }
                else {
                    $('#submit_btn').html(defaultBtnValue);
                    $('#submit_btn').attr("disabled", false);
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                $('#submit_btn').html(defaultBtnValue);
                $('#submit_btn').attr("disabled", false);
                errorAlert("Network failure, please try again");
            }
        });
    } else {
        $('#submit_btn').html(defaultBtnValue);
        $('#submit_btn').attr("disabled", false);
        errorAlert("Please fill the form Correctly");
    }

    $('.approve-button, .reject-button').on('click', function () {
        
        var userId = $(this).data('user-id');
        var action = $(this).hasClass('approve-button') ? 'approve' : 'reject';

        $.ajax({
            url: '/Bonus/UpdateUserStatus',
            type: 'POST',
            data: { genLogId: userId, action: action },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Bonus/UserGenLog';
                    successAlertWithRedirect(result.msg, url);
                } else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Error occurred. Please try again.");
            }
        });
    });
}
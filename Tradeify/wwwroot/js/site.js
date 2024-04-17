
function register() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);
    var urlParams = new URLSearchParams(window.location.search);
    var refferrerId = urlParams.get('rf');
    var parentId = urlParams.get('pr');
    var data = {};
    data.FirstName = $('#firstName').val();
    data.LastName = $('#lastName').val();
    data.UserName = $('#userName').val();
    data.Phonenumber = $('#phoneNumber').val();
    data.Email = $('#email').val();
    data.GenderId = $('#genderId').val();
    data.CordinatorId = $('#cordinatorId').val();
    data.PackageId = $('#packageId').val();
    data.Password = $('#password').val();
    data.ConfirmPassword = $('#confirmPassword').val();

    if (data.FirstName != "" && data.LastName != "" && data.UserName != "" && data.Phonenumber != ""
        && data.Email != "" && data.GenderId != 0 && data.Password != "" && data.ConfirmPassword != "") {
        let userDetails = JSON.stringify(data);
        $.ajax({
            type: 'Post',
            url: '/Account/Registration',
            dataType: 'json',
            data:
            {
                userDetails: userDetails,
                refferrerId: refferrerId,
                parentId: parentId,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Account/Login';
                    successAlertWithRedirect(result.msg, url);
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
                errorAlert("Please check and try again. Contact Admin if issue persists..");
            }
        });
    } else {
        $('#submit_btn').html(defaultBtnValue);
        $('#submit_btn').attr("disabled", false);
        errorAlert("Please fill the form Correctly");
    }
}

let packageDetailsToShow = [];

$(window).on("load", function () {
    if (location.pathname.toLowerCase().includes("account/register")) {
        $.ajax({
            type: 'GET',
            url: '/Account/GetPackageDetails',
            success: function (result) {
                if (!result.isError) {
                    packageDetailsToShow = JSON.parse(result.data);
                }
                else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Network failure, please try again", ex);
            }
        });
    }
});

$(document).ready(function () {
    // Attach change event to the package dropdown
    $('#packageId').change(function () {
        var selectedPackageId = $(this).val();
        displaySelectedPackageDetails(selectedPackageId);
    });
});

function displaySelectedPackageDetails(selectedPackageId) {
    var selectedPackageDetailsElement = $('#selectedPackageDetails');
    selectedPackageDetailsElement.empty();
    // Check if a package is selected
    if (selectedPackageId !== "") {
        var selectedPackage = packageDetailsToShow.find(p => p.Id == selectedPackageId);
         
        // Package Details
        
        var app = "<div style=\"border-bottom: 2px solid #dba622; color: white; text-align: left; padding: 5px\">" +
            "<h5 style=\"margin-bottom: 2px; color: white;\">" + "Package Name: " + selectedPackage.Name + "</h5>" +
            "<p>" + "Description: " + selectedPackage.Description + "</p>" + 
            "<p>" + "Package Price: " + selectedPackage.Price + "</p>" + 
            "<p>" + "Package Bonus Amount: " + selectedPackage.BonusAmount + "</p>" + 
            "<p>" + "Package Maximum Generation: " + selectedPackage.MaxGeneration + "</p>" + 
            "</div>";
        $("#selectedPackageDetails").append(app);
    }
}

function login() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);
    var userName = $('#userName').val();
    var password = $('#password').val();
    $.ajax({
        type: 'Post',
        url: '/Account/Login',
        dataType: 'json',
        data:
        {
            userName: userName,
            password: password
        },
        success: function (result) {
            if (!result.isError) {
                var n = 1;
                localStorage.removeItem("on_load_counter");
                localStorage.setItem("on_load_counter", n);
                location.replace(result.dashboard);
                return;
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
            errorAlert("An error occured, please try again.");
        }
    });
}

function OpenAddAdminRoleForm() {
    $("#addRoleModal").modal();
};

function addNewRole() {
    var data = {};
    data.roleUserName = $('#userName').val();
    data.roleSelected = $('#roleSelected').val();

    let roleData = JSON.stringify(data);
    $.ajax({
        type: 'post',
        dataType: 'Json',
        url: '/Admin/AddToRoles',
        data: {
            roleData: roleData ,
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/Permission'
                successAlertWithRedirect(result.msg, url)
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function selectUserRoleToBeRemoved(userName) {
    var roleUserName = userName
    $('#delRole').val(roleUserName);
}

function deleteUserFromAdminRole() {
    var userRole = $('#delRole').val();

    $.ajax({
        type: 'Post',
        url: '/Admin/RemoveFromRoles', // we are calling json method
        data: { userRole: userRole, },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/Permission'
                successAlertWithRedirect(result.msg, url)
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    });
}

function OpenCordinatorForm() {
    $("#cordinatorModal").modal();
};

function addNewCordinator() {
    var cordinatorUserName = $('#cordinatorUserName').val();
    $.ajax({
        type: 'post',
        dataType: 'Json',
        url: '/Admin/AddCordinator',
        data: {
            cordinatorUserName: cordinatorUserName,
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/Cordinator'
                successAlertWithRedirect(result.msg, url)
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    })
}

function selectCordinatorToBeRemoved(id) {
    var sCId = id
    $('#delId').val(sCId);
}

function deleteCordinator() {
    var id = $('#delId').val();

    $.ajax({
        type: 'Post',
        url: '/Admin/DeleteCordinator',
        data: { id: id },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/Cordinator'
                successAlertWithRedirect(result.msg, url)
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert(result.msg);
        }
    });
}

function openSelectedDropDownForEditModal(id) {
    $.ajax({
        type: 'GET',
        url: '/Admin/EditDropDown',
        dataType: 'json',
        data: { id: id},
        success: function (result) {
            if (!result.isError) {
                var resp = result.dropdownDetails;
                $('#dropDownId').val(resp.id);
                $('#name_edit').val(resp.name);
                $('#edit_dropdown').modal("show");
            }
            else {
                errorAlert(result.msg);
            }
        }
    });
}

function UpdateDropDown() {
    var id = $('#dropDownId').val();
    var newDropdownName = $('#name_edit').val();
    if (newDropdownName != "") {
        $.ajax({
            type: 'POST',
            url: '/Admin/SaveEditedDropDownName',
            dataType: 'json',
            data:
            {
                newDropdownName: newDropdownName,
                id : id,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = "/Admin/DropDownList";
                    successAlertWithRedirect(result.msg, url);
                    $('#edit_dropdown').modal("hide");
                }
                else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                "Something went wrong, contact GGC support - " + errorAlert(ex);
            }
        });
    } else {
        errorAlert("Incorrect Details");
    }
}

function selectedDropdownToBeRemoved(id) {
    var seletedDropdownId = id
    $('#deletelId').val(seletedDropdownId);
    $('#delete_dropdown').modal("show");
}

function deleteToDropDown() {
    var id = $('#deletelId').val();
    $.ajax({
        type: 'Post',
        url: '/Admin/DeleteDropDown',
        data: { id: id },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/DropDownList'
                successAlertWithRedirect(result.msg, url)
            }
            else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Error occured try again");
        }
    });
}

function regFeePayment() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);
    
    var data = {};
    data.BankAccountId = $('#bankAccountId').val();
    data.PackageId = $('#packageId').val();
    data.PaidFrom = $('#accountPaidFrom').val();
    data.BankNamePaidFrom = $('#bankNamePaidFrom').val();
    data.AccountNumberPaidFrom = $('#accountNumberPaidFrom').val();
    data.PaymentMethod = $('#paymentMethod').val();

    if (data.PackageId != 0 && data.PaidFrom != "" && data.BankNamePaidFrom != ""
        && data.AccountNumberPaidFrom != "" && data.PaymentMethod != "") {
        let details = JSON.stringify(data);
        $.ajax({
            type: 'Post',
            url: '/User/RegFeePayment',
            dataType: 'json',
            data:
            {
                details: details,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/User/RegistrationSuccessPage';
                    successAlertWithRedirect(result.msg, url);
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
                errorAlert("Please fill the form Correctly");
            }
        });
    } else {
        $('#submit_btn').html(defaultBtnValue);
        $('#submit_btn').attr("disabled", false);
        errorAlert("Please fill the form Correctly");
    }
}

function regCrptoPayment() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);

    var data = {};
    data.BankAccountId = $('#bankAccountId').val();
    data.PackageId = $('#packageId').val();
    data.PaidFrom = $('#accountPaidFrom').val();
    data.BankNamePaidFrom = $('#bankNamePaidFrom').val();
    data.AccountNumberPaidFrom = $('#accountNumberPaidFrom').val();
    data.PaymentMethod = $('#paymentMethod').val();

    if (data.PackageId != 0 && data.PaidFrom != "" && data.BankNamePaidFrom != ""
        && data.AccountNumberPaidFrom != "" && data.PaymentMethod != "") {
        let details = JSON.stringify(data);
        $.ajax({
            type: 'Post',
            url: '/User/RegCryptoPayment',
            dataType: 'json',
            data:
            {
                details: details,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/User/RegistrationSuccessPage';
                    successAlertWithRedirect(result.msg, url);
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
                errorAlert("Please check and try again.");
            }
        });
    } else {
        $('#submit_btn').html(defaultBtnValue);
        $('#submit_btn').attr("disabled", false);
        errorAlert("Please fill the form Correctly");
    }
}

$(document).ready(function () {
    $("#codeId").hide();

    $("#DropDownKeyId").change(function () {
        $.ajax({
            type: 'POST',
            url: '/Admin/GetNameOfDropDownSelected',
            dataType: 'json',
            data: { dropdownId: $("#DropDownKeyId").val() },
            success: function (result) {
                if (result) {
                    $("#codeId").show();
                }
                else {
                    $("#codeId").hide();
                }
            },
            error: function (ex) {
                alert('This Id does not exist in the dropdowns.' + ex);
            }
        });
    });
});

function OpenCreateModalForm() {
    $("#create_dropdown").modal();
};

function createDropdown() {
    var data = {};
    data.dropdownKey = $('#dropDownKeyId').val();
    data.Name = $('#name').val();

    if (data.dropdownKey != "" && data.Name != "") {
        let details = JSON.stringify(data);
        $.ajax({
            type: 'Post',
            url: '/Admin/CreateDropDown',
            dataType: 'json',
            data:
            {
                details: details,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Admin/DropDownList';
                    successAlertWithRedirect(result.msg, url);
                }
                else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Please check and try again. Contact GGC Support if issue persists..");
            }
        });
    } else {
        errorAlert("Please fill the form Correctly");
    }
}

function approveRegPayment(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/ApproveRegFee',
        dataType: 'json',
        data: {
            paymentId: id
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Bonus/UserGenLog';
                successAlertWithDoubleButtons(result.msg, url);
            }
            else {
                 errorAlert(result.msg);
            }
        },
        error: function (ex) {
            "Something went wrong, contact the support - " + errorAlert(ex);
        }
    });
}

function declineRegPayments(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/DeclineRegFee', // we are calling json method
        dataType: 'json',
        data:
        {
            paymentId: id
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/PaymentApproval';
                successAlertWithRedirect(result.msg, url);
                $('#submit_btn').html(defaultBtnValue);
            }
            else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Please, Contact the Support for --- " + ex);
        }
    });
}

function DeactivateUser(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/DeactivateUser', 
        dataType: 'json',
        data:
        {
            userId: id
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/Users';
                successAlertWithRedirect(result.msg, url);
                $('#submit_btn').html(defaultBtnValue);
            }
            else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Please, Contact the Support for --- " + ex);
        }
    });
}

function GetUserDetails(Id, cordinatorId) {
    $.ajax({
        type: 'GET',
        url: '/User/EditUserDetails',
        data: {
            userId: Id,
            cordinatorId: cordinatorId
        },
        success: function (data) {
            $('#userContent').html(data);
            $('#userModal').modal("show");
            $('.select').niceSelect();
        },
    })
}

function userFullDetails(Id) {
    $.ajax({
        type: 'GET',
        url: '/Admin/UserFullDetail',
        data: {
           id: Id
        },
        success: function (data) {
            $('#myModalBody').html(data);
            $('#myModal').modal("show");
        },
        //error: function (ex) {
        //    errorAlert("An error occured, please check and try again. Please contact admin if issue persists..");
        //}
    })
}

function SaveEditedDetails() {
    var defaultBtnValue = $('#submit_Btn').html();
    $('#submit_Btn').html("Please wait...");
    $('#submit_Btn').attr("disabled", true);
    var data = {};
    data.Id = $("#user_Id").val();
    data.FirstName = $("#edited_fName").val();
    data.LastName = $("#edited_lName").val();
    data.Email = $("#edited_email").val();
    data.Phonenumber = $("#edited_phoneNumber").val();
    data.CordinatorId = $("#edited_cordinatorId").val();
    data.PackageId = $("#edited_packageId").val();

    if (data.FirstName != "" && data.LastName != "" && data.Email != "" && data.Phonenumber != "" && data.CordinatorId != "" && data.PackageId != "") {
        let details = JSON.stringify(data);
        $.ajax({
            type: 'POST',
            url: '/User/EditedUserDetails',
            dataType: 'json',
            data:
            {
                details: details,
            },
            success: function (result) {
                if (!result.isError) {
                    location.reload();
                }
                else {
                    $('#submit_Btn').html(defaultBtnValue);
                    $('#submit_Btn').attr("disabled", false);
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                $('#submit_Btn').html(defaultBtnValue);
                $('#submit_Btn').attr("disabled", false);
                errorAlert(result.msg);
            }
        });
    } else {
        $('#submit_Btn').html(defaultBtnValue);
        $('#submit_Btn').attr("disabled", false);
        errorAlert("Invalid, Please fill the form correctly.");
    }
}

function viewDetailModal(id) {
    if(id !=0)
    $.ajax({
        type: 'get',
        dataType: 'json',
        url: '/Admin/GetNewsById',
        data:
        {
            newsId: id,
        },
        success: function (result) {
            if (!result.isError) {
                $("#newsDetails").val(result.data.details);
            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("Network failure, please try again");
        }
    });
        
    
}

function ReferralLink() {
    $.ajax({
        type: 'GET',
        url: '/User/ReferralLink',
        success: function (data) {
            if (!data.isError) {
                var text = data;
                navigator.clipboard.writeText(text)
                successAlert("Referral link copied successfully");
            }
            else {
                location.replace(data.dashboard);
            }
        }
    });
}

function populateUserGen() {
    var selectedGen = $("#selectGen").val();
    var userId = $("#userId").val();

    $.ajax({
        url: '/Binary/GetUserGeneration',
        type: 'GET',
        data:
        {
            gen: selectedGen,
            userId: userId
        },
        success: function (result) {
            if (!result.isError) {
                var data = result.genDetails;
                $("#userListContainer").empty();
                $.each(data, function (i, gen) {

                    var detail = "<div class=\"col-md-1 col-4 pb-1 pl-0\"><div class=\"card \">" +
                        "<div class=\"col-12 d-flex flex-column px-0\">" +
                        "<div class=\"row bg-faded py-2\">" +
                        "<div class=\"col-8 mx-auto\">" +
                        "<img class=\"rounded-circle img-fluid bg-info\" src=\"/images/user.png\">" +
                        "</div>" +
                        "<div class=\"col-12 text-center m-auto\">" +
                        "<a href=\"#\" class=\"p-1 text-dark\" style=\"border-radius:5px;cursor:pointer;background-color:#dba622;\" onclick=\"ChildReferralLink('" + gen.child?.id + "')\">" + gen.child?.userName + "</a>"                        
                        + " </div></div></div></div ></div >";

                    $("#userListContainer").append(detail);
                })

            } else {
                errorAlert(result.msg)
            }
        },
        error: function (error) {
            errorAlert(result.msg);

        }
    });
}

function approveWithdrawals(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/ApproveWithdrawals',
        dataType: 'json',
        data: {
            withdrawalRequestId: id
        },
        success: function (result) {
            if (!result.isError) {
                successAlert("Withdrawal Fee Approved successfully.");
                location.reload("AllWithdrawals");
            }
            else {
                errorAlert("Something went wrong, contact the support - " + result.msg);
            }
        },
        error: function (ex) {
            errorAlert(ex);
        }
    });
}

function rejectWithdrawals(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/RejectWithdrawalRequest',
        dataType: 'Json',
        data:
        {
            withdrawalId: id
        },
        success: function (result) {
            if (!result.isError) {
                successAlert(result.msg);
                location.reload("AllWithdrawals");
            }
            else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Please, Contact the Support for --- " + ex);
        }
    });
}


$(document).ready(function () {
    //Bonus Approval for Multiple Users
    $('.approve-selected, .reject-selected').on('click', function ()
    {
        var selectedUserIds = [];
        var action = $(this).hasClass('approve-selected') ? 'approve' : 'reject';
        $('.user-checkbox:checked').each(function () {
            selectedUserIds.push($(this).data('user-id'));
        });
        $.ajax({
            type: 'POST',
            url: '/Bonus/UpdateUserStatus', 
            data: {
                genLogIds: selectedUserIds,
                action: action
            },
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
    
    //Bonus Approval for Single User
    $('.approve-button, .reject-button').on('click', function () {
        var userId = $(this).data('user-id');
        var action = $(this).hasClass('approve-button') ? 'approve' : 'reject';

        $.ajax({
            url: '/Bonus/UpdateOneUserStatus',
            type: 'POST',
            data: {
                genLogId: userId,
                action: action
            },
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
});

function ChildReferralLink(userId) {
    $.ajax({
        type: 'GET',
        url: '/User/ChildReferralLink',
        dataType: 'Json',
        data:
        {
            userId: userId
        },
        success: function (data) {
            if (!data.isError) {
                var text = data;
                navigator.clipboard.writeText(text)
                successAlert("Referral link copied successfully");
            }
            else {
                errorAlert(data.msg);
            }
        }
    });
}

function viewPackageDetailModal(id) {
    
    if (id != 0)
        $.ajax({
            type: 'get',
            dataType: 'json',
            url: '/Admin/GetPackageDetails',
            data:
            {
                id: id,
            },
            success: function (result) {
                
                if (!result.isError) {
                    $("#packagesDetails").val(result.data.description);
                }
                else {
                    errorAlert(result.msg)
                }
            },
            error: function (ex) {
                errorAlert("Network failure, please try again");
            }
        });


}

function viewUserPackageDetailModal(id) {
 
    if (id != null) {
        $.ajax({
            type: 'get',
            dataType: 'json',
            url: '/User/GetUserPackageDetails',
            data: {
                id: id,
            },
            success: function (result) {

                if (!result.isError) {
                    $("#userPackagesDetails").val(result.data.package.description);
                    
                } else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Network failure, please try again");
            }
        });
    } else {
        errorAlert("Invalid PackageId");
    }
}

function GetProfileDetails(id) {
    $.ajax({
        type: 'GET',
        url: '/User/EditProfileDetails',
        data: {
            userId: id,
        },
        success: function (data) {
            $('#profileContent').html(data);
            $('#profileModal').modal("show");
            $('.select').niceSelect();
        },
    })
}

function SaveEditedProfileDetails() {
    var defaultBtnValue = $('#submit_Btn').html();
    $('#submit_Btn').html("Please wait...");
    $('#submit_Btn').attr("disabled", true);
    var data = {};
    data.Id = $("#profile_Id").val();
    data.FirstName = $("#edited_firstName").val();
    data.LastName = $("#edited_lastName").val();
    data.Email = $("#edited_profileEmail").val();
    data.Phonenumber = $("#edited_phone").val();

    if (data.FirstName != "" && data.LastName != "" && data.Email != "" && data.Phonenumber != "") {
        let details = JSON.stringify(data);
        $.ajax({
            type: 'POST',
            url: '/User/EditedProfileDetails',
            dataType: 'json',
            data:
            {
                details: details,
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Profile';
                    successAlertWithRedirect(result.msg, url);
                }
                else {
                    $('#submit_Btn').html(defaultBtnValue);
                    $('#submit_Btn').attr("disabled", false);
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                $('#submit_Btn').html(defaultBtnValue);
                $('#submit_Btn').attr("disabled", false);
                errorAlert(result.msg);
            }
        });
    } else {
        $('#submit_Btn').html(defaultBtnValue);
        $('#submit_Btn').attr("disabled", false);
        errorAlert("Invalid, Please fill the form correctly.");
    }
}


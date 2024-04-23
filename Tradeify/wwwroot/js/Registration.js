function createpackages() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);

    var data = {
        Name: $('#name').val(),
        Price: $('#price').val(),
        BonusAmount: $('#bonusAmount').val(),
        Description: $('#description').val(),
        GenerationId: $('#generationId').val(),
    };

    if (data.Name != "" && data.Price != 0 && data.Description != "" && data.GenerationId != 0) {
        $.ajax({
            type: 'POST',
            url: '/RegistrationPackage/CreateRegPacks',
            dataType: 'json',
            data:
            {
                CreatePackageData: JSON.stringify(data)
            },
            success: function (result) {
                $('#submit_btn').html(defaultBtnValue);
                if (!result.isError) {
                    var url = '/RegistrationPackage/Index';
                    successAlertWithRedirect(result.msg, url);
                }
                else {
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
    }
    else {
        $('#submit_btn').html(defaultBtnValue);
        $('#submit_btn').attr("disabled", false);
        errorAlert("Please fill the form Correctly");
    }
}

function editPackage(id) {
    $.ajax({
        type: 'GET',
        dataType: 'json',
        url: '/RegistrationPackage/PackageToEdit',
        data: {
            packageId: id
        },
        success: function (result) {
            if (!result.isError) {
                debugger;
                var resp = result.data;
                $('#editId').val(resp.id);
                $('#edit_Name').val(resp.name);
                $('#edit_Price').val(resp.price);
                $('#edit_BonusAmount').val(resp.bonusAmount);
                $('#edit_description').val(resp.description);
                $('#generationId option[value="' + resp.maxGeneration + '"]').attr("selected", "selected");
                //$('#generationId').val(resp.maxGeneration);
                $('#edit_Package').modal('show');
                $('.select').niceSelect('update');

            } else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Network failure, please try again");
        }
    });
}

function PackageToSave() {
    var defaultBtnValue = $('#submit-btn').html();
    $('#submit-btn').html("Please wait...");
    $('#submit-btn').attr("disabled", true);
    debugger;
    var se = $("li.option.selected").attr("data-value");
    var data = {
        Id: $("#editId").val(),
        Name: $("#edit_Name").val(),
        Price: $("#edit_Price").val(),
        BonusAmount: $("#edit_BonusAmount").val(),
        Description: $("#edit_description").val(),
        //GenerationId: $("#generationId").val(),       
        GenerationId: $("li.option.selected").attr("data-value"),
    };
    if (data.Name !== "" && data.Price !== "" && data.Description != "" && data.GenerationId != "0") {
        var registrationPackage = JSON.stringify(data);
        $.ajax({
            type: 'POST',
            url: '/RegistrationPackage/EditedPackage',
            dataType: 'json',
            data: {
                registrationPackageDetails: registrationPackage
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/RegistrationPackage/Index';
                    successAlertWithRedirect(result.msg, url);
                } else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Network failure, please try again");
            },
            complete: function () {
                $('#submit-btn').html(defaultBtnValue);
                $('#submit-btn').attr("disabled", false);
            }
        });
    } else {
        errorAlert("Invalid, please fill in the required fields.");
        $('#submit-btn').html(defaultBtnValue);
        $('#submit-btn').attr("disabled", false);
    }
}

function PackageToBeDeleted(id) {
    $('#deletId').val(id);
}

function DeletePackage() {
    var id = $('#deletId').val();
    $.ajax({
        type: 'Post',
        dataType: 'json',
        url: '/RegistrationPackage/PackageDeleted',
        data:
        {
            id: id,
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/RegistrationPackage/Index';
                successAlertWithRedirect(result.msg, url)
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

function sendPayGrant() {
    var data = {};
    data.Amount = $("#amount").val();
    data.AppreciationDetails = $("#appreciateReasons").val();
    var listofUserId = memberIds;

    if (listofUserId != "" && data.Amount != "" & data.AppreciationDetails != "") {
        let details = JSON.stringify(data);
        $.ajax({
            url: '/Admin/AppreciateMember',
            type: 'POST',
            data:
            {
                userIds: listofUserId,
                details: details
            },
            success: function (result) {
                if (!result.isError) {
                    var url = '/Admin/AppreciateMember';
                    successAlertWithRedirect(result.msg, url);
                } else {
                    errorAlert(result.msg);
                }
            },
            error: function (ex) {
                errorAlert("Error occurred. Please try again.");
            }
        });

    } else {
        errorAlert("Please fill the form Correctly");
    }
}

let upgradePackageDetailsToShow = [];
let oldPackageAmount = [];

$(window).on("load", function () {

    $.ajax({
        type: 'GET',
        url: '/User/GetPackageUpgradeDetailsForPaymentForm',
        success: function (result) {
            if (!result.isError) {
                upgradePackageDetailsToShow = JSON.parse(result.data);
                oldPackageAmount = result.results;

            }
            else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Network failure, please try again", ex);
        }
    });

});

$(document).ready(function () {
    $('#packageId').change(function () {
        var selectedUpgradePackageId = $(this).val();
        displaySelectedPackageUpgradeDetails(selectedUpgradePackageId);
    });
});

function displaySelectedPackageUpgradeDetails(selectedUpgradePackageId) {
    var selectedPackageDetailsElement = $('#selectedPackageUpgradeDetails');
    selectedPackageDetailsElement.empty();
    if (selectedUpgradePackageId !== "") {
        var selectedPackageUpgrade = upgradePackageDetailsToShow.find(p => p.Id == selectedUpgradePackageId);
        var upgradePrice = selectedPackageUpgrade.Price - oldPackageAmount;

        var apps = "<div style=\"border-bottom: 2px solid #22c8db; color: white; text-align: left; padding: 5px\">" +
            "<h5 style=\"margin-bottom: 2px; color: white;\">" + "Package Name: " + selectedPackageUpgrade.Name + "</h5>" +
            "<p>" + "Package Price: $" + selectedPackageUpgrade.Price + "</p>" +
            //"<p>" + "Package Bonus Amount: $" + selectedPackageUpgrade.BonusAmount + "</p>" +
            "<p>" + "Package Maximum Generation: " + selectedPackageUpgrade.MaxGeneration + "</p>" +
            "<p>" + "Amount To Pay For Upgrade: $" + upgradePrice + "</p>" +
            "<p>" + "Description: " + selectedPackageUpgrade.Description + "</p>" +
            "</div>";
        $("#selectedPackageUpgradeDetails").append(apps);
        $("#amount").val(upgradePrice);

    }
}

function displaySelectedPackageDetailsAlert(packageId) {
    let upgradePackageDetailsToShowAlert = [];

    $.ajax({
        type: 'GET',
        url: '/User/GetPackageUpgradeAlertDetails',
        dataType: 'json',
        data: {
            packageId: packageId
        },
        success: function (result) {
            if (!result.isError) {
                upgradePackageDetailsToShowAlert = JSON.parse(result.data);
                oldPackageAmount = result.results;

                var selectedUpgradePackageIdd = upgradePackageDetailsToShowAlert.Id;

                if (selectedUpgradePackageIdd !== "") {
                    var selectedPackageUpgrade = upgradePackageDetailsToShowAlert;
                    var upgradePrice = selectedPackageUpgrade.Price - oldPackageAmount;

                    var appsDetails = {
                        name: selectedPackageUpgrade.Name,
                        description: selectedPackageUpgrade.Description,
                        price: selectedPackageUpgrade.Price,
                        //bonusAmount: selectedPackageUpgrade.BonusAmount,
                        maxGeneration: selectedPackageUpgrade.MaxGeneration,
                        upgradeBalance: upgradePrice
                    };
                    var url = '/User/Payment';

                    swal.fire({
                        title: "Upgrade Package Details",
                        html:
                            "<p><strong>Package Name:</strong> " + appsDetails.name + "</p>" +
                            "<p><strong>Package Price:</strong> $" + appsDetails.price + "</p>" +
                            //"<p><strong>Package Bonus Amount:</strong> $" + appsDetails.bonusAmount + "</p>" +
                            "<p><strong>Package Maximum Generation:</strong> " + appsDetails.maxGeneration + "</p>" +
                            "<p><strong>Amount To Pay For Upgrade:</strong> $" + appsDetails.upgradeBalance + "</p>" +
                            "<p><strong>Description:</strong> " + appsDetails.description + "</p>",

                        icon: "success",
                        showCancelButton: false,
                        showConfirmButton: true,
                        confirmButtonText: "OK",


                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = url;
                        }
                    });
                }
            } else {
                errorAlert(result.msg);
            }
        },
        error: function (ex) {
            errorAlert("Network failure, please try again", ex);
        }
    });
}

function approveUserPackagePayment(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/ApprovePackageFee',
        dataType: 'json',
        data: {
            paymentId: id
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/PaymentApproval';
                successAlertWithRedirect(result.msg, url);
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

function declineUserPackagePayments(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/DeclinePackagePaymentFee', // we are calling json method
        dataType: 'json',
        data:
        {
            paymentId: id
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/Admin/PaymentApproval';
                successAlertWithRedirect(result.msg, url);
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
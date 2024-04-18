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

var groupIds = [];
$(function () {
    $('#getCustomerGroupId').on("optionselected", function (e) {
        getSelectedIds("selected", e.detail.value);
    });
    $('#getCustomerGroupId').on("optiondeselected", function (e) {
        getSelectedIds("deselected", e.detail.value);
    });
});


function getSelectedIds(event, selectedId) {
    if (event == "selected") {
        groupIds.push(selectedId);
    }
    else {
        $.each(groupIds, function (i, id) {
            ;
            if (selectedId == id) {
                groupIds.splice(i, 1);
            }
        });
    }
}


var count = 1;
function charCount() {
    var maxlength = parseInt($("#maxlength").val());
    if (maxlength != 0) {
        var msgLength = parseInt($('#characterCount').val().length);
        if (msgLength > 0) {
            var result = msgLength % maxlength;
            if (result > 0) {
                $("#total_counter").text(result);
                if (msgLength > maxlength) {
                    count = parseInt((msgLength / maxlength) + 1);
                    $("#pageCount").text(count);
                }
            }
        }
    } else {
        errorAlert("Character length per page have not been set");
    }
   
}


$("#sendMessage").on("click", function () {
    var data = {};
    data.CustomerGroupId = groupIds;
    data.Sender = $('#sender').val();
    data.Message = $('#characterCount').val();
    if (data.CustomerGroupId != []) {
        let details = JSON.stringify(data);
        if (details != "") {
            $.ajax({
                type: 'POST',
                url: '/Message/SendMessage',
                dataType: 'json',
                data:
                {
                    messageDetails: details,
                },
                success: function (result) {
                    if (!result.isError) {
                        var url = location.href;
                        newSuccessAlert(result.msg, url);
                    }
                    else {
                        errorAlert(result.msg);
                    }
                },
                error: function (ex) {
                    errorAlert(ex);
                }
            });
        }
        else {
            errorAlert("Please fill the form correctly");
        }
    }
});

//var arraysOfAllModules = [];
//$(function () {
//    $.ajax({
//        type: 'Get',
//        dataType: 'json',
//        url: '/Patient/GetAllModules',
//        success: function (result) {
//            if (!result.isError) {
//                arraysOfAllModules = result.data;
//            }
//        },
//        error: function (ex) {
//            errorAlert("An error occured, please try again. Please contact admin if issue persists.");
//        }
//    });
//})



var modeIds = [];
$(function () {
    $('#moduleSelectId').on("optionselected", function (e) {
        getSelectedIdss("selected", e.detail.value);
    });
    $('#moduleSelectId').on("optiondeselected", function (e) {
        getSelectedIdss("deselected", e.detail.value);
    });
});
var modeIdsFromEdit = [];
$(function () {
    $('#moduleSelectIdFromEdit').on("optionselected", function (e) {
        getSelectedIdsFromEdit("selected", e.detail.value);
    });
    $('#moduleSelectIdFromEdit').on("optiondeselected", function (e) {
        getSelectedIdsFromEdit("deselected", e.detail.value);
    });
});



function getSelectedIdss(event, selectedId) {
    if (event == "selected") {
        modeIds.push(selectedId);
    }
    else {
        $.each(modeIds, function (i, id) {
            if (selectedId == id) {
                modeIds.splice(i, 1);
            }
        });
    }
}

function getSelectedIdsFromEdit(event, selectedId) {
    if (event == "selected") {
        modeIdsFromEdit.push(selectedId);
    }
    else {
        $.each(modeIdsFromEdit, function (i, id) {
            if (selectedId == id) {
                modeIdsFromEdit.splice(i, 1);
            }
        });
    }
}

function copyAllStock() {
    var defaultBtnValue = $('#submit_Btn').html();
    $('#submit_Btn').html("Please wait...");
    $('#submit_Btn').attr("disabled", true);
    var data = {}
    data.CompanyBranchId = $('#copyAllStockToSelectedCompanyBranchId').val()
    let productListViewModel = JSON.stringify(data);
    $.ajax({
        type: 'POST',
        dataType: 'Json',
        url: '/ShopProducts/CopyAllStock',
        data: {
            productListViewModel: productListViewModel
        },
        success: function (result) {
            if (!result.isError) {
                var url = '/ShopProducts/Views'
                successAlertWithRedirect(result.msg, url)
                $('#submit_Btn').html(defaultBtnValue);
            }
            else {
                $('#submit_Btn').html(defaultBtnValue);
                $('#submit_Btn').attr("disabled", false);
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            $('#submit_Btn').html(defaultBtnValue);
            $('#submit_Btn').attr("disabled", false);
            errorAlert("An error occured, please try again. Please contact admin if issue persists.");
        }
    })
}


var count = 1;
function charCounter() {
    var maxlength = parseInt($("#maxlength").val());
    if (maxlength != 0) {
        var msgLength = parseInt($('#characterCounter').val().length);
        if (msgLength > 0) {
            var result = msgLength % maxlength;
            if (result > 0) {
                $("#total_count").text(result);
                if (msgLength > maxlength) {
                    count = parseInt((msgLength / maxlength) + 1);
                    $("#pageCount").text(count);
                }
            }
        }
    } else {
        errorAlert("Character length per page have not been set");
    }

}

var customerIds = [];
$(function () {
    $('#getCustomerId').on("optionselected", function (e) {
        getSelectedId("selected", e.detail.value);
    });
    $('#getCustomerId').on("optiondeselected", function (e) {
        getSelectedId("deselected", e.detail.value);
    });
});


function getSelectedId(event, selectedId) {
    if (event == "selected") {
        customerIds.push(selectedId);
    }
    else {
        $.each(customerIds, function (i, id) {
            if (selectedId == id) {
                customerIds.splice(i, 1);
            }
        });
    }
}

$("#messageToCustomer").on("click",function () {
    var data = {};
    data.CustomersId = customerIds;
    data.Message = $('#characterCounter').val();
    data.Sender = $('#sender').val();
    if (data.CustomersId != []) {
        let details = JSON.stringify(data);
        if (details != "") {
            $.ajax({
                type: 'POST',
                url: '/Message/SendMessageToCustomer',
                dataType: 'json',
                data:
                {
                    messageDetails: details,
                },
                success: function (result) {
                    if (!result.isError) {
                        var url = location.href;
                        newSuccessAlert(result.msg, url);
                    }
                    else {
                        errorAlert(result.msg);
                    }
                },
                error: function (ex) {
                    errorAlert(ex);
                }
            });
        }
        else {
            errorAlert("Please fill the form correctly");
        }
    }
});

$(document).ready(function () {
  var datatab =  $('#dataTableSearch').DataTable({
        aLengthMenu: [
            [10, 25, 50, 100, -1],
            [10, 25, 50, 100, "All"]
        ],
        iDisplayLength: 10,
        "drawCallback": function (settings) {
            $(".more,.less").click(function () {
                var read = this
                if ($(this).hasClass("more")) {
                    var p = $(this).parent();
                    $(p).addClass("d-none");
                    $(p).next("td").removeClass("d-none");

                }
                else {
                    var pless = $(this).parent();
                    $(pless).addClass("d-none")
                    $(pless).prev("td").removeClass("d-none");

                }
            });
        },
    
  });
    datatab.search("Search...")
});

$(document).ready(function () {
    $('#reportDataTableSearch').DataTable({
        aLengthMenu: [
            [10, 25, 50, 100, -1],
            [10, 25, 50, 100, "All"]
        ],
        iDisplayLength: 10,
        dom: 'Bfrtip',
        buttons: [
            'copy', 'csv', 'excel', 'pdf', 'print'
        ]
    });
});

$('#data_TableSearch').dataTable({
    searching: false,
    
});

$('#dataTableDelivery').dataTable({
    aLengthMenu: [
        [10, 25, 50, 100, -1],
        [10, 25, 50, 100, "All"]
    ],
    iDisplayLength: 100
});

$("#getOldRecord").change(function () {
    var isChecked = $(this).is(":checked");
    if (isChecked) {
        location.href = '/Admin/DisplayPastRecord';
    }
})

function viewMessagelContent(id) {
    $.ajax({
        type: 'GET',
        url: '/SuperAdmin/GetMessageContentById', // we are calling json method
        dataType: 'json',
        data:
        {
            id: id
        },
        success: function (data) {
            
            if (!data.isError) {

                $("#contId").val(data.data.id);
                $("#msgCont").text(data.data.messageContent);
                $("#MessageContent").modal({ backdrop: 'static', keyboard: false }, 'show');

            }
        },
        error: function (ex) {
            "please fill the form correctly" + errorAlert(ex);
        }
    });
}

function editCompanyProfile(id) {
    ;
    $.ajax({
        type: 'Get',
        dataType: 'Json',
        url: '/Admin/EditCompanyProfile',
        data: {
            id: id
        },
        success: function (result) {
            ;
            if (!result.isError) {
                $('#companyProfileId').val(result.data.id);
                $('#companyName').val(result.data.name);
                $('#email').val(result.data.email);
                $('#datecreated').val(dateToInput(result.data.dateInString));
                $('#companyAddress').val(result.data.companyAddress);
                $('#phoneNumber').val(result.data.phone);
                $('#mobileNumber').val(result.data.mobile);
                $('#msgSenderName').val(result.data.messageSenderName);

            }
            else {
                errorAlert(result.msg)
            }
        },
        error: function (ex) {
            errorAlert("An error occured, please try again. Please contact admin if issue persists.");
        }
    })
}
function dateToInput(dateString) {
    var now = new Date(dateString);
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);

    var today = now.getFullYear() + "-" + (month) + "-" + (day);

    return today;
}

function editedCompanyProfile() {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Please wait...");
    $('#submit_btn').attr("disabled", true);
    var data = {}
    var CompanyLogo = document.getElementById("companyLogo").files;
    data.Id = $('#companyProfileId').val();
    data.Name = $('#companyName').val();
    data.Email = $('#email').val();
    data.Phone = $('#phoneNumber').val();
    data.DateCreated = $('#datecreated').val();
    data.CompanyAddress = $('#companyAddress').val();
    data.Mobile = $('#mobileNumber').val();
    data.MessageSenderName = $('#msgSenderName').val();
    var base64;
    data.AdminFullName = $('#adminFullName').val();
    if (CompanyLogo.length > 0) {
        const reader = new FileReader();
        reader.readAsDataURL(CompanyLogo[0]);
        
        reader.onload = function () {
            base64 = reader.result;
            ;
            if (data.Phone.length == 11 || data.Phone.length == 13) {
                if (data.Mobile.length == 11 || data.Mobile.length == 13 || data.Mobile == "") {
                    let companyProfileDetails = JSON.stringify(data);
                    $.ajax({
                        type: 'post',
                        dataType: 'Json',
                        url: '/Admin/EditedCompanyProfile',
                        data: {
                            companyProfileDetails: companyProfileDetails,
                            base64: base64
                        },
                        success: function (result) {
                            ;
                            if (!result.isError) {
                                var url = '/admin/MyCompanyProfile'
                                successAlertWithRedirect(result.msg, url)
                                $('#submit_btn').html(defaultBtnValue);
                            }
                            else {
                                $('#submit_btn').html(defaultBtnValue);
                                $('#submit_btn').attr("disabled", false);
                                errorAlert(result.msg)
                            }
                        },
                        error: function (ex) {
                            $('#submit_btn').html(defaultBtnValue);
                            $('#submit_btn').attr("disabled", false);
                            errorAlert("An error occured, please try again. Please contact admin if issue persists.");
                        }
                    })
                }
                else {
                    $('#submit_btn').html(defaultBtnValue);
                    $('#submit_btn').attr("disabled", false);
                    errorAlert("Mobile Number must be equal to 11 or 13 digit");
                }
            }
            else {
                $('#submit_btn').html(defaultBtnValue);
                $('#submit_btn').attr("disabled", false);
                errorAlert("Phone Number must be equal to 11 or 13 digit");
            }
           
        }
    }
    else {
        if (data.Phone.length == 11 || data.Phone.length == 13) {
            if (data.Mobile.length == 11 || data.Mobile.length == 13 || data.Mobile == "") {
                let companyProfileDetails = JSON.stringify(data);
                $.ajax({
                    type: 'post',
                    dataType: 'Json',
                    url: '/Admin/EditedCompanyProfile',
                    data: {
                        companyProfileDetails: companyProfileDetails,
                        base64: base64
                    },
                    success: function (result) {
                        ;
                        if (!result.isError) {
                            var url = '/admin/MyCompanyProfile'
                            successAlertWithRedirect(result.msg, url)
                            $('#submit_btn').html(defaultBtnValue);
                        }
                        else {
                            $('#submit_btn').html(defaultBtnValue);
                            $('#submit_btn').attr("disabled", false);
                            errorAlert(result.msg)
                        }
                    },
                    error: function (ex) {
                        $('#submit_btn').html(defaultBtnValue);
                        $('#submit_btn').attr("disabled", false);
                        errorAlert("An error occured, please try again. Please contact admin if issue persists.");
                    }
                })
            }
            else {
                $('#submit_btn').html(defaultBtnValue);
                $('#submit_btn').attr("disabled", false);
                errorAlert("Mobile Number must be equal to 11 or 13 digit");
            }
        }
        else {
            $('#submit_btn').html(defaultBtnValue);
            $('#submit_btn').attr("disabled", false);
            errorAlert("Phone Number must be equal to 11 or 13 digit");
        }
        
    }
    
}

var picture;
$("#uploadLogo").change(function () {
    $("#editShowPix").html("");

    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
    if (regex.test($(this).val().toLowerCase())) {
        if (typeof (FileReader) != "undefined") {

            var dats = $("#editShowPix").attr("src");
            var reader = new FileReader();
            reader.onload = function (e) {
                picture = e.target.result;
                $("#editShowPix").attr("src", picture);
            }
            reader.readAsDataURL($(this)[0].files[0]);
        } else {
            alert("This browser does not support FileReader.");
        }
    } else {
        alert("Please upload a valid image file.");
    }
});

$("#uploadLogo").change(function () {
    $("#editLogoPix").html("");

    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
    if (regex.test($(this).val().toLowerCase())) {
        if (typeof (FileReader) != "undefined") {

            var dats = $("#editLogoPix").attr("src");
            var reader = new FileReader();
            reader.onload = function (e) {
                picture = e.target.result;
                $("#editLogoPix").attr("src", picture);
            }
            reader.readAsDataURL($(this)[0].files[0]);
        } else {
            alert("This browser does not support FileReader.");
        }
    } else {
        alert("Please upload a valid image file.");
    }
});

function uploadCompanyLogo(id) {
    var defaultBtnValue = $('#submit_Btn').html();
    $('#submit_Btn').html("Please wait...");
    $('#submit_Btn').attr("disabled", true);
    var companyLogo = document.getElementById("uploadLogo").files;
    if (companyLogo[0] != null) {
        const reader = new FileReader();
        reader.readAsDataURL(companyLogo[0]);
        var base64;
        reader.onload = function () {
            base64 = reader.result;
            ;
            if (base64 != "" || base64 != 0 && id != "") {
                $.ajax({
                    type: 'Post',
                    dataType: 'Json',
                    url: '/Admin/EditCompanyProfileLogo',
                    data: {
                        id: id,
                        base64: base64,
                    },

                    success: function (result) {
                        ;
                        if (!result.isError) {
                            var url = '/Admin/MyCompanyProfile?id=' + result.id;
                            successAlertWithRedirect(result.msg, url)
                            $('#submit_Btn').html(defaultBtnValue);
                        }
                        else {
                            $('#submit_Btn').html(defaultBtnValue);
                            $('#submit_Btn').attr("disabled", false);
                            errorAlert(result.msg)
                        }
                    }, 
                    error: function (ex) {
                        $('#submit_Btn').html(defaultBtnValue);
                        $('#submit_Btn').attr("disabled", false);
                        errorAlert("An error occured, please try again. Please contact admin if issue persists.");
                    }
                })
            }
            else {
                $('#submit_Btn').html(defaultBtnValue);
                $('#submit_Btn').attr("disabled", false);
                errorAlert("Please Enter Details");
            }
        }
    }
    else {
        $('#submit_Btn').html(defaultBtnValue);
        $('#submit_Btn').attr("disabled", false);
    }
}

function viewMsgDetails(id) {
    $.ajax({
        type: 'GET',
        url: '/Company/GetMessageContentById', // we are calling json method
        dataType: 'json',
        data:
        {
            id: id
        },
        success: function (result) {
            
            if (!result.isError) {
                $("#contId").val(result.data.id);
                $("#msgCont").text(result.data.messageContent);
                $("#reciever").text(result.data.reciever);
                $("#isSent").text(result.data.isSent);
                $("#title").text(result.data.title);
                $("#dateSent").text(result.data.dateInString);
                $("#msgPhoneNumber").text(result.data.phoneNumber);
                $("#MessageContent").modal({ backdrop: 'static', keyboard: false }, 'show');
            }
        },
        error: function (ex) {
            "An error occured, please try again. Please contact admin if issue persists." + errorAlert(ex);
        }
    });
   
   
}

function PreserveVistsData() {
    var data = {};
    data.TreatmentHistory = $('#treatmentHistoryId').val();
    data.PatientId = $('#patientId').val();
    data.VaccinationHistory = $('#vaccinationHistoryId').val();
    data.EnvironmentalHistory = $('#environmentalHistoryId').val();
    data.FeedingHistory = $('#feedingHistoryId').val();
    data.GeneralExamination = $('#generalExaminationId').val();
    data.PhysicalExamination = $('#physicalExaminationId').val();
    data.PhysiologicalExamination = $('#physiologicalExaminationId').val();
    data.LaboratoryExamination = $('#laboratoryExaminationId').val();
    data.LaboratorySamples = $('#laboratorySamplesId').val();
    data.LaboratoryResults = $('#laboratoryResultsId').val();
    data.DifferentialDiagnosis = $('#differentialDiagnosisId').val();
    data.DefinitiveDiagnosis = $('#definitiveDiagnosisId').val();
    data.PrimaryComplaint = $('#primaryComplainId').val();
    data.Purpose = $('#purposeId').val();
    data.TreatmentDone = $('#treatmentDoneId').val();
    var nextDateValFromInput = $('#nextDateId').val();
    if (nextDateValFromInput == "") {
        data.nextDate = "1/1/0001 12:00:00 AM";
    }
    else {
        data.nextDate = nextDateValFromInput;
    }
    var sendSMS = $('#sendSMS').is(":checked");
    if (sendSMS) {
        data.SMSAllowed = true;
    }
    else {
        data.EmailAllowed = false;
    }
    var sendEmail = $('#sendEmail').is(":checked");
    if (sendEmail) {
        data.SendEmail = true;
    } else {
        data.SendEmail = false;
    }

    let newVisit = JSON.stringify(data);
    localStorage.setItem('AddVisitPage', newVisit);
}

$(function () {
    var visitSmsCheck = localStorage.getItem("chec");
    var visitSMS = JSON.parse(visitSmsCheck)
    if (visitSMS == true) {
        $('#sendSMS').attr('checked', true);
    }
})

$(".visit_listner").on("blur", function () {
    PreserveVistsData();
});



var maxChar = 11;
var maxchars = 13;
$("#tel").on('input', function (ev) {
        var char = $("#tel").val();
        if (char.charAt(0) == '0') {
            if (char.length < '11') {
                $("#tel").css('border', '1px solid #ff0000');
                $("#warningMsg").empty()
                $("#warningMsg").append("Phone Number must be 11 digit");
                $("#warningMsg").removeAttr("hidden");
            }
            else {
                if (char.length == 11) {
                    $("#tel").css('border',"0.5px solid #e3e3e3");
                    $("#warningMsg").empty()
                    $("#warningMsg").attr("hidden");
                }
            }
            $("#tel").val(char.substring(0, maxChar));
            

        } else {
            if (char.length < "13") {
                $("#tel").css('border', '1px solid #ff0000');
                $("#warningMsg").empty()
                $("#warningMsg").append("Phone Number must be 13 digit");
                $("#warningMsg").removeAttr("hidden");
            }
            else {
                if (char.length == 13) {
                    $("#tel").css('border', "0.5px solid #e3e3e3");
                    $("#warningMsg").empty()
                    $("#warningMsg").attr("hidden");
                }
            }
            $("#tel").val(char.substring(0, maxchars));

        }
    
});
   
$("#edit_tel").on('input', function (ev) {
    var char = $("#edit_tel").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#edit_tel").css('border', '1px solid #ff0000');
            $("#edit_warningMsg").empty()
            $("#edit_warningMsg").append("Phone Number must be 11 digit");
            $("#edit_warningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#edit_tel").css('border', "0.5px solid #e3e3e3");
                $("#edit_warningMsg").empty()
                $("#edit_warningMsg").attr("hidden");
            }
        }
        $("#edit_tel").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#edit_tel").css('border', '1px solid #ff0000');
            $("#edit_warningMsg").empty()
            $("#edit_warningMsg").append("Phone Number must be 13 digit");
            $("#edit_warningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#edit_tel").css('border', "0.5px solid #e3e3e3");
                $("#edit_warningMsg").empty()
                $("#edit_warningMsg").attr("hidden");
            }
        }
        $("#edit_tel").val(char.substring(0, maxchars));

    }
});
$("#phoneNumber").on('input', function (ev) {
    var char = $("#phoneNumber").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#phoneNumber").css('border', '1px solid #ff0000');
            $("#phoneWarningMsg").empty()
            $("#phoneWarningMsg").append("Phone Number must be 11 digit");
            $("#phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#phoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#phoneWarningMsg").empty()
                $("#phoneWarningMsg").attr("hidden");
            }
        }
        $("#phoneNumber").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#phoneNumber").css('border', '1px solid #ff0000');
            $("#phoneWarningMsg").empty()
            $("#phoneWarningMsg").append("Phone Number must be 13 digit");
            $("#phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#phoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#phoneWarningMsg").empty()
                $("#phoneWarningMsg").attr("hidden");
            }
        }
        $("#phoneNumber").val(char.substring(0, maxchars));

    }
});
$("#mobileNumber").on('input', function (ev) {
    var char = $("#mobileNumber").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#mobileNumber").css('border', '1px solid #ff0000');
            $("#moblieWarningMsg").empty()
            $("#moblieWarningMsg").append("Mobile Number must be 11 digit");
            $("#moblieWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#mobileNumber").css('border', "0.5px solid #e3e3e3");
                $("#moblieWarningMsg").empty()
                $("#moblieWarningMsg").attr("hidden");
            }
        }
        $("#mobileNumber").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#mobileNumber").css('border', '1px solid #ff0000');
            $("#moblieWarningMsg").empty()
            $("#moblieWarningMsg").append("Mobile Number must be 13 digit");
            $("#moblieWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#mobileNumber").css('border', "0.5px solid #e3e3e3");
                $("#moblieWarningMsg").empty()
                $("#moblieWarningMsg").attr("hidden");
            }
        }
        $("#mobileNumber").val(char.substring(0, maxchars));

    }
});
$("#phoneNumberP").on('input', function (ev) {
    var char = $("#phoneNumberP").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#phoneNumberP").css('border', '1px solid #ff0000');
            $("#phoneWarningMsgP").empty()
            $("#phoneWarningMsgP").append("Phone Number must be 11 digit");
            $("#phoneWarningMsgP").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#phoneNumberP").css('border', "0.5px solid #e3e3e3");
                $("#phoneWarningMsgP").empty()
                $("#phoneWarningMsgP").attr("hidden");
            }
        }
        $("#phoneNumberP").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#phoneNumberP").css('border', '1px solid #ff0000');
            $("#phoneWarningMsgP").empty()
            $("#phoneWarningMsgP").append("Phone Number must be 13 digit");
            $("#phoneWarningMsgP").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#phoneNumberP").css('border', "0.5px solid #e3e3e3");
                $("#phoneWarningMsgP").empty()
                $("#phoneWarningMsgP").attr("hidden");
            }
        }
        $("#phoneNumberP").val(char.substring(0, maxchars));

    }
});
$("#supplier_PhoneNumber").on('input', function (ev) {
    var char = $("#supplier_PhoneNumber").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#supplier_PhoneNumber").css('border', '1px solid #ff0000');
            $("#supplier_phoneWarningMsg").empty()
            $("#supplier_phoneWarningMsg").append("Phone Number must be 11 digit");
            $("#supplier_phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#supplier_PhoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#supplier_phoneWarningMsg").empty()
                $("#supplier_phoneWarningMsg").attr("hidden");
            }
        }
        $("#supplier_PhoneNumber").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#supplier_PhoneNumber").css('border', '1px solid #ff0000');
            $("#supplier_phoneWarningMsg").empty()
            $("#supplier_phoneWarningMsg").append("Phone Number must be 13 digit");
            $("#supplier_phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#supplier_PhoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#supplier_phoneWarningMsg").empty()
                $("#supplier_phoneWarningMsg").attr("hidden");
            }
        }
        $("#supplier_PhoneNumber").val(char.substring(0, maxchars));

    }
});
$("#edit_supplier_PhoneNumber").on('input', function (ev) {
    var char = $("#edit_supplier_PhoneNumber").val();
    if (char.charAt(0) == '0') {
        if (char.length < '11') {
            $("#edit_supplier_PhoneNumber").css('border', '1px solid #ff0000');
            $("#edit_supplier_phoneWarningMsg").empty()
            $("#edit_supplier_phoneWarningMsg").append("Phone Number must be 11 digit");
            $("#edit_supplier_phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 11) {
                $("#edit_supplier_PhoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#edit_supplier_phoneWarningMsg").empty()
                $("#edit_supplier_phoneWarningMsg").attr("hidden");
            }
        }
        $("#edit_supplier_PhoneNumber").val(char.substring(0, maxChar));

    } else {
        if (char.length < "13") {
            $("#edit_supplier_PhoneNumber").css('border', '1px solid #ff0000');
            $("#edit_supplier_phoneWarningMsg").empty()
            $("#edit_supplier_phoneWarningMsg").append("Phone Number must be 13 digit");
            $("#edit_supplier_phoneWarningMsg").removeAttr("hidden");
        }
        else {
            if (char.length == 13) {
                $("#edit_supplier_PhoneNumber").css('border', "0.5px solid #e3e3e3");
                $("#edit_supplier_phoneWarningMsg").empty()
                $("#edit_supplier_phoneWarningMsg").attr("hidden");
            }
        }
        $("#edit_supplier_PhoneNumber").val(char.substring(0, maxchars));

    }
});


function selectSavedModules(data) {
    $(".selected-items").empty();
    modeIdsFromEdit = [];
    $(".custom-checkbox").prop("checked", false);
    $.each(data, function (i, module) {
        var isSelected = $('span:contains(' + module.name + ')').length;
        if (isSelected == 0) {
            var option = '<span id="'+module.id+'" data-id="moduleSelectIdFromEdit-' + module.id + '" class="item">' +
                module.name
                + '<button type="button" onclick="removeSelectSavedModules(event)" tabindex="-1">x</button></span>';
            $(".selected-items").append(option);
            $(".placeholder").attr("hidden", true);
            $(".custom-checkbox").filter(function () {
                return $(this).val() == module.id;
            }).prop("checked", true);

            modeIdsFromEdit.push(module.id);
            $("#moduleSelectIdFromEdit--1-chbx").prop("checked", false);
        }

    });
}

function removeSelectSavedModules(event) {
    var spanToRemoveId = $(event.currentTarget).parent()[0];
    var spanId =   spanToRemoveId.id
    $(spanToRemoveId).remove();
    $(".custom-checkbox").filter(function () {
        return $(this).val() == spanId;
    }).prop("checked", false);
    const index = modeIdsFromEdit.indexOf(parseInt(spanId));
    if (index > -1) {
        modeIdsFromEdit.splice(index, 1);
    }
}

var companyIds = [];
$(function () {
    $('#getCompaniesId').on("optionselected", function (e) {
        getConpanyAdminSelectedIds("selected", e.detail.value);
    });
    $('#getCompaniesId').on("optiondeselected", function (e) {
        getConpanyAdminSelectedIds("deselected", e.detail.value);
    });
});

$(function () {
    $('#getCompaniesIdEmail').on("optionselected", function (e) {
        getConpanyAdminSelectedIds("selected", e.detail.value);
    });
    $('#getCompaniesIdEmail').on("optiondeselected", function (e) {
        getConpanyAdminSelectedIds("deselected", e.detail.value);
    });
});

var companygroupIds = [];
$(function () {
    $('#getCompaniesId').on("optionselected", function (e) {
        getAllSelectedIds("selected", e.detail.value);
    });
    $('#getCompaniesId').on("optiondeselected", function (e) {
        getAllSelectedIds("deselected", e.detail.value);
    });
});

function getAllSelectedIds(event, selectedId) {
    if (event == "selected") {
        companygroupIds.push(selectedId);
    }
    else {
        $.each(companygroupIds, function (i, id) {
            ;
            if (selectedId == id) {
                companygroupIds.splice(i, 1);
            }
        });
    }
}


function getConpanyAdminSelectedIds(event, selectedId) {
    debugger;
    if (event == "selected") {
        companyIds.push(selectedId);
    }
    else {
        $.each(companygroupIds, function (i, id) {
            debugger;
            if (selectedId == id) {
                companyIds.splice(i, 1);
            }
        });
    }
}
$(function () {
    $("#Patient_Select_bank").select2();
    $("#Patient_Select_payment").select2();
    $("#Select_bank_QuickSales").select2();
    $("#Select_bank").select2();
    $("#Select_payment").select2();
});


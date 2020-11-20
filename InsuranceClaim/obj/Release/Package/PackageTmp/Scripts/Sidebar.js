
function WebCustomerPage() {
    SetValueIntoSession();

    $(".loading-area").show();
    window.location.href = '/WebCustomer/Index';
}


function CustomerPage() {
    $(".loading-area").show();
    window.location.href = '/WebCustomer/Index';
}

function RiskDetilsPage() {
    $(".loading-area").show();
    window.location.href = '/CustomerRegistration/RiskDetail';
}

function SummaryDetailsPage() {
    $(".loading-area").show();
    window.location.href = '/CustomerRegistration/SummaryDetail';
}


function SetActiveSidebar() {
    $(".sm").removeClass("active");
    var menuName = $.cookie('menu');
    $("." + menuName).addClass("active");
}


function RemoveCookies()
{
    $.removeCookie("menu");
}


function SetValueIntoSession()
{
    $.ajax({
        url: "/InsuranceHome/SetValueIntoSession",
        type: "POST",
        data: { regNo: $("#regNo").val(), nationalId: $("#nationalId").val() },
        dataType: "json",
        async: false,
    success: function (result) {
                  
    }
    });



}


function CallBackoUser()
{
    var userPhone = $("#txtUserPhone").val();
    var userName = $("#txtUserName").val();

    if (userPhone=="")
    {
        $("#txtUserPhone").focus();
        $("#txtUserPhone").addClass("border border-danger");
        return;
    }
    else
    {
        $("#txtUserPhone").removeClass("border border-danger");
    }

    if (userName == "") {
        $("#txtUserName").focus();
        $("#txtUserName").addClass("border border-danger");
        return;
    }
    else
    {
        $("#txtUserName").removeClass("border border-danger");
    }

    $(".loading-area").show();

  
    setTimeout(function () {
        $(".loading-area").hide();
        $.ajax({
            url: "/InsuranceHome/SendCallBackEmail",
            type: "POST",
            data: { UserName: $("#txtUserName").val(), UserPhone: $("#txtUserPhone").val() },
            dataType: "json",
            async: false,
            success: function (result) {
                $("#requestModal").modal('show');
                $(".loading-area").hide();

                $("#txtUserPhone").val('');
                $("#txtUserName").val('');

            }
        });
    }, 500);
}



$('#txtUserPhone').keypress(function (event) {
    var keycode = event.which;
    if (!(event.shiftKey == false && (keycode == 46 || keycode == 8 || keycode == 37 || keycode == 39 || (keycode >= 48 && keycode <= 57)))) {
        event.preventDefault();
    }
});





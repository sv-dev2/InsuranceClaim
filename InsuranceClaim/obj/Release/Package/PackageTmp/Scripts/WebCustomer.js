function DisplayPresoanlDetails() {
    if (ValidateVehicle()) {

        var CoverType = $("input[name='RiskDetail.CoverTypeName']:checked").val();
        if (CoverType == "Comprehensive") {
            $("#RiskDetail_CoverTypeId").val(4);
        }

        if (CoverType == "Third Party") {
            $("#RiskDetail_CoverTypeId").val(1);
        }


        $("#riskdetial").hide();
        $("#presonal_detail").show();
    }
}

function DisplayVehicleDetial() {
    if (ValidateCustomer()) {
        $("#presonal_detail").hide();
        $("#vehicledetials").show();

        $(".loading-area").show();
        setTimeout(function () {
            $(".loading-area").hide();
            CalculatePremiumByCoverType();
        }, 1000);

    }
}

function DisplaySummaryDetail() {
    $("#vehicledetials").hide();
    $("#Summary_detail").show();
}

function BackToRiskDetail() {
    $("#presonal_detail").hide();
    $("#riskdetial").show();
}

function BackToCustomerDetail() {
    $("#vehicledetials").hide();
    $("#presonal_detail").show();
}

function BackToVehicleDetail() {
    $("#Summary_detail").hide();
    $("#vehicledetials").show();
}

function ShowHideSumInsured() {
    var radioValue = $("input[name='RiskDetail.CoverTypeName']:checked").val();
    if (radioValue == 'Comprehensive') {
        $("#divSumInsured").show();
    }
    else {
        $("#divSumInsured").hide();
    }
}

function ValidateVehicle() {
    debugger;
    var result = true;
    var RegistrationNo = $("#RiskDetail_RegistrationNo").val();

    var paymentTermId = $("#RiskDetail_PaymentTermId").val();
    var CoverType = $("input[name='RiskDetail.CoverTypeName']:checked").val();
    var ProductId = $("#RiskDetail_ProductId").val();
    var isZinaraLicense = $('input[name="RiskDetail.IncludeLicenseFee"]:checked').val();
    var isRadioLicense = $('input[name="RiskDetail.IncludeRadioLicenseCost"]:checked').val();

    var zinaraPaymentTermId = $("#RiskDetail_ZinaraLicensePaymentTermId").val();
    var radioPaymentTermId = $("#RiskDetail_RadioLicensePaymentTermId").val();

    var vehicleUsage = $("#RiskDetail_VehicleUsage").val();

    if (RegistrationNo=="")
    {
        $("#errorDiv").show();
        $("#errorMsg").text("Please enter registration number on previous screen.");
        result = false;
    }

   else if ($("input[name='RiskDetail.CoverTypeName']:checked").length == 0) {
        $("#errorDiv").show();
        $("#errorMsg").text("Please select cover type.");
        result = false;
    }

    else if (CoverType == "Comprehensive") {
        if ($("#RiskDetail_SumInsured").val() == "" || $("#RiskDetail_SumInsured").val() == "0.00") {
            $("#errorDiv").show();
            $("#RiskDetail_SumInsured").focus();
            $("#errorMsg").text("Please Enter Sum Insured.");
            result = false;
        }
        else {
            $("#errorDiv").hide();
            $("#errorMsg").text("");
            result = true;
        }

    }

    else if (ProductId == "") {
        $("#errorDiv").show();
        $("#errorMsg").text("Please select product.");
        result = false;
    }

    else if (paymentTermId == "") {
        $("#errorDiv").show();
        $("#errorMsg").text("Please select payment term.");
        result = false;
    }
    else if (isZinaraLicense && zinaraPaymentTermId=="") {
      
            $("#errorDiv").show();
            $("#errorMsg").text("Please select Zinara License payment term.");
            result = false;
        
    }
    else if (isRadioLicense && radioPaymentTermId=="") {
            $("#errorDiv").show();
            $("#errorMsg").text("Please select Radio License payment term.");
            result = false;       
    }

    else if (isRadioLicense && !isZinaraLicense) {    
            $("#errorDiv").show();
            $("#errorMsg").text("Only radio License is not allowed. Please select Zinara also.");
            result = false;     
    }
    else if (vehicleUsage == "") {
        $("#errorDiv").show();
        $("#errorMsg").text("Please select VehicleUsage.");
        result = false;
    }

    else {
        $("#errorDiv").hide();
        $("#errorMsg").text("");
        result = true;
    }

    return result;
}


function ValidateCustomer() {
    debugger;
    var result = true;
    var Customer_FirstName = $("#Customer_FirstName").val();
    var Customer_EmailAddress = $("#Customer_EmailAddress").val();
    var Customer_LastName = $("#Customer_LastName").val();
    var Customer_PhoneNumber = $("#Customer_PhoneNumber").val();
    var Customer_DateOfBirth = $("#Customer_DateOfBirth").val();
    var AddressLine1 = $("#AddressLine1").val();

    var City = $("#Customer_City").val();

    if (Customer_FirstName == "") {
        $("#errorDiv").show();
        $("#Customer_FirstName").focus();
        $("#errorMsg").text("Please enter first name");
        result = false;
    }

    else if (Customer_LastName == "") {
        $("#errorDiv").show();
        $("#Customer_LastName").focus();
        $("#errorMsg").text("Please enter sur name");
        result = false;
    }

    else if (Customer_EmailAddress == "") {
        $("#errorDiv").show();
        $("#Customer_EmailAddress").focus();
        $("#errorMsg").text("Please enter email address");
        result = false;
    }
    else if (IsEmail(Customer_EmailAddress) == false) {
        $("#errorDiv").show();
        $("#Customer_EmailAddress").focus();
        $("#errorMsg").text("invalid email");
        return false;
    }
    else if (Customer_DateOfBirth == "") {
        $("#errorDiv").show();
        $("#Customer_DateOfBirth").focus();
        $("#errorMsg").text("Please enter DOB");
        result = false;
    }
    else if (Customer_PhoneNumber == "") {
        $("#errorDiv").show();
        $("#Customer_PhoneNumber").focus();
        $("#errorMsg").text("Please enter phone number");
        result = false;
    }
    else if ($("input[name='Customer.Gender']:checked").length == 0) {
        $("#errorDiv").show();
        $("#errorMsg").text("Please select Gender.");
        result = false;
    }

    else if (AddressLine1 == "") {
        $("#errorDiv").show();
        $("#AddressLine1").focus();
        $("#errorMsg").text("Please enter address.");
        result = false;
    }

    else if (City == "") {
        $("#errorDiv").show();
        $("#AddressLine1").focus();
        $("#errorMsg").text("Please select city.");
        result = false;
    }

    else {
        $("#errorDiv").hide();
        $("#errorMsg").text("");
        result = true;
    }
    return result;
}


function IsEmail(email) {
    var regex = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    if (!regex.test(email)) {
        return false;
    } else {
        return true;
    }
}


function CalculatePremiumByCoverType() {
    if ($("#RiskDetail_PaymentTermId").val() != "" && $("#RegistrationNo").val() != "" && $("#CoverTypeId").val() != "") {
        var coverytype = $('#CoverTypeId option:selected').val();
        generateQuotewithICEcash();
    }
}


function generateQuotewithICEcash() {
    debugger;
    var CoverType = $("input[name='RiskDetail.CoverTypeName']:checked").val();
    var coverTypeId = 1;

    if (CoverType == "Comprehensive") {
        coverTypeId = 4;
        if ($("#RiskDetail_SumInsured").val() == "" || $("#RiskDetail_SumInsured").val() == "0.00") {
            $("#errorDiv").show();
            $("#RiskDetail_SumInsured").focus();
            $("#errorMsg").text("Please Enter Sum Insured !!");
        }
    }
    else {
        coverTypeId = 1;
        if ($("#RiskDetail_SumInsured").val() == "" || $("#RiskDetail_SumInsured").val() == "0.00") {
            $("#RiskDetail_SumInsured").val('0');
        }
    }


    ValidateRadioLicense();


    var Customer_FirstName = $("#Customer_FirstName").val();
    var Customer_EmailAddress = $("#Customer_EmailAddress").val();
    var Customer_LastName = $("#Customer_LastName").val();
    var Customer_PhoneNumber = $("#Customer_PhoneNumber").val();
    var Customer_DateOfBirth = $("#Customer_DateOfBirth").val();
    var AddressLine1 = $("#AddressLine1").val();
    var NationalId = $("#Customer_NationalIdentificationNumber").val();


    var isLicense = $("#RiskDetail_IncludeLicenseFee").is(':checked');
    var isRadioLicense = $("#RiskDetail_IncludeRadioLicenseCost").is(':checked');

    var radioLicensePaymentTerm = $("#RiskDetail_RadioLicensePaymentTermId").val();
    var zinaraLicensePaymentTerm = $("#RiskDetail_ZinaraLicensePaymentTermId").val();



    var vehilceUsage = $("#RiskDetail_VehicleUsage").val();

    //var isLicense = false;
    //var isRadioLicense = false;

    var taxClass = $("#RiskDetail_TaxClassId").val();

    $(".loading-area").show();

    //if (flag) {
    //$(".loading-area").show();
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        url: "/WebCustomer/getPolicyDetailsFromICEcash",
        data: { regNo: $("#RiskDetail_RegistrationNo").val(), PaymentTerm: $("#RiskDetail_PaymentTermId").val(), suminsured: $("#RiskDetail_SumInsured").val(), CoverTypeId: coverTypeId, VehicleType: $("#RiskDetail_ProductId").val(), VehilceLicense: isLicense, RadioLicense: isRadioLicense, firstName: Customer_FirstName, lastName: Customer_LastName, email: Customer_EmailAddress, address: AddressLine1, phone: Customer_PhoneNumber, nationalId: NationalId, radioLicensePaymentTerm: radioLicensePaymentTerm, zinaraLicensePaymentTerm: zinaraLicensePaymentTerm, vehilceUsage: vehilceUsage },
        success: function (data) {
            debugger;
            if (data.result == 0) {
                $("#errorDiv").show();
                $("#RiskDetail_SumInsured").focus();
                $("#errorMsg").text(data.message);

                $("#RiskDetail_InsuranceId").val("");
                $("#RiskDetail_LicenseId").val("");
                $("#RiskDetail_ArrearsAmt").val("0");
                $("#RiskDetail_PenaltiesAmt").val("0");
                $("#RiskDetail_VehicleLicenceFee").val("0");
                $("#RiskDetail_RadioLicenseCost").val("0");
                $("#RiskDetail_LicExpiryDate").val("");
                $("#RiskDetail_CombinedID").val("");
                $("#isVehicleRegisteredonICEcash").val(0);
                $(".loading-area").hide();
            }
            if (data.result == 1) {

                // for displaying selected make and model

                var vehicle = {};
                vehicle = data.Data.Response.Quotes[0].Vehicle;

                $("#RiskDetail_TaxClassId").val(vehicle.TaxClass);

                GetVehicleMake();

                var vMake = vehicle.Make;
                $("#RiskDetail_MakeId option").filter(function () { return this.text == vMake.toUpperCase(); }).attr('selected', true);

                //  $("#MakeId").addClass("inputDisabled");
                var vModel = vehicle.Model;
                GetVehicleModels(vModel);

                var vYearManufacture = vehicle.YearManufacture;
                if (vYearManufacture == null)
                    $("#RiskDetail_VehicleYear").val("1900"); // default value
                else
                    $("#RiskDetail_VehicleYear").val(vYearManufacture);



                var vUsage = vehicle.VehicleType;
                //ajax vehicle u -> prod id
                // getProductIdbyVehicleUsageId(vUsage);

                $("#RiskDetail_InsuranceId").val(data.Data.Response.Quotes[0].InsuranceID); // 25_march 2019

                var policy = {};
                policy = data.Data.Response.Quotes[0].Policy;

                if (data.Data.Response.Quotes[0] != null && data.Data.Response.Quotes[0].Licence != null) {
                    $("#RiskDetail_LicenseId").val(data.Data.Response.Quotes[0].LicenceID);

                    var arrAmount = parseFloat(data.Data.Response.Quotes[0].Licence.TotalLicAmt).toFixed(2);
                    var penaltiesAmount = parseFloat(data.Data.Response.Quotes[0].Licence.PenaltiesAmt).toFixed(2);

                    $("#RiskDetail_ArrearsAmt").val(arrAmount);
                    $("#RiskDetail_PenaltiesAmt").val(penaltiesAmount);

                    //if(penaltiesAmount>0)
                    //    toastr.error("You have outstanding penalties, please contact our Contact Centre for assistance on 086 77 22 33 44.");


                    if (isRadioLicense) {
                        $("#RiskDetail_RadioLicenseCost").val(parseFloat(data.Data.Response.Quotes[0].Licence.TotalRadioTVAmt).toFixed(2));

                    }


                    $("#RiskDetail_VehicleLicenceFee").val(parseFloat(arrAmount + penaltiesAmount).toFixed(2));
                    $("#RiskDetail_LicExpiryDate").val(data.Data.Response.Quotes[0].Licence.LicExpiryDate);
                    $("#RiskDetail_CombinedID").val(data.Data.Response.Quotes[0].CombinedID);
                }

                var StampDuty = policy.StampDuty;
                $("#RiskDetail_StampDuty").val(parseFloat(StampDuty).toFixed(2));
                var GovernmentLevy = policy.GovernmentLevy;
                $("#RiskDetail_ZTSCLevy").val(parseFloat(GovernmentLevy).toFixed(2));
                var CoverAmount = policy.CoverAmount;
                //$("#Premium").val(parseFloat(CoverAmount).toFixed(2));
                $("#RiskDetail_Discount").val(parseFloat(data.Data.LoyaltyDiscount).toFixed(2));

                var calulatePremium = CoverAmount - data.Data.LoyaltyDiscount;

                $("#RiskDetail_Premium").val(parseFloat(calulatePremium).toFixed(2));
                $("#RiskDetail_PremiumWithDiscount").val(parseFloat(CoverAmount).toFixed(2));

                $("#RiskDetail_InsuranceId").val(data.Data.Response.Quotes[0].InsuranceID);

                $("#basicPremiumICEcash").val(parseFloat(CoverAmount).toFixed(2));

                $("#stampDutyICEcash").val(parseFloat(StampDuty).toFixed(2))
                $("#LevyICEcash").val(parseFloat(GovernmentLevy).toFixed(2))


                $("#isVehicleRegisteredonICEcash").val(1);
                QuoteCalculation();
                $(".loading-area").hide();
            }
            else {
                QuoteCalculation();
                $(".loading-area").hide();
            }

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $(".loading-area").hide();
            if (textStatus == 'timeout') {
                //doe iets
            } else if (textStatus == 'error') {
                //doe iets
            }
        } //EINDE error
    }); //EINDE ajax
}

function QuoteCalculation() {
    debugger;

    var isReinsuranceConfirmed = false;

    if ($("#RiskDetail_SumInsured").val() == "") {
        $("#RiskDetail_SumInsured").val("0");
    }
    var sumInsured = parseFloat($("#RiskDetail_SumInsured").val());

    if (sumInsured > 100000 && !isReinsuranceConfirmed) {
        bootbox.confirm("Sum Insured above than 100000 will be considered as Reinsurance case. Are you sure you want to continue ?", function (result) {
            debugger;
            if (!result) {
                bootbox.hideAll();
                return false;
            }
            else {
                isReinsuranceConfirmed = true;
                callQuoteCalculation(sumInsured);
            }
        });
    }
    else {
        callQuoteCalculation(sumInsured);
    }
}


function callQuoteCalculation(sumInsured) {

    debugger;
    if (sumInsured < 1) {
        //return false;
    }
    var vehicleUsageId = parseInt($("#RiskDetail_VehicleUsage").val());
    if (vehicleUsageId < 1) {
        return false;
    }


    var CoverType = $("input[name='RiskDetail.CoverTypeName']:checked").val();
    var coverTypeId = 1;

    if (CoverType == "Comprehensive") {
        coverTypeId = 4;
    }



    var excess = 0;
    var excessType = 0;

    var AddThirdPartyAmount = 0;
    var NumberofPersons = 0;
    var PassengerAccidentCover = false;
    var ExcessBuyBack = false;
    var RoadsideAssistance = false;
    var MedicalExpenses = false;
    var Addthirdparty = false;
    var RadioLicenseCost = 0;
    var AgentCommissionId = 0;


    var IncludeRadioLicenseCost = $("#RiskDetail-IncludeRadioLicenseCost").is(":checked");
    var policytermid = $("#RiskDetail_PaymentTermId").val();

    var currencyId = $("#RiskDetail_CurrencyId").val();

    if (NumberofPersons == undefined || NumberofPersons == null || NumberofPersons == "") {
        NumberofPersons = 0;
    }

    var ProductId = $("#RiskDetail_ProductId").val();
    var vehicleUsage = $("#RiskDetail_VehicleUsage").val();

    $(".loading-area").show();

    $.ajax({
        type: "POST",
        async: false,
        url: "/CustomerRegistration/CalculatePremium",
        data: { vehicleUsageId: vehicleUsage, sumInsured: sumInsured, coverType: coverTypeId, excessType: excessType, excess: excess, AddThirdPartyAmount: AddThirdPartyAmount, NumberofPersons: NumberofPersons, Addthirdparty: Addthirdparty, PassengerAccidentCover: PassengerAccidentCover, ExcessBuyBack: ExcessBuyBack, RoadsideAssistance: RoadsideAssistance, MedicalExpenses: MedicalExpenses, RadioLicenseCost: RadioLicenseCost, IncludeRadioLicenseCost: IncludeRadioLicenseCost, policytermid: policytermid, isVehicleRegisteredonICEcash: $("#isVehicleRegisteredonICEcash").val() == "1", BasicPremium: $("#basicPremiumICEcash").val(), StampDuty: $("#stampDutyICEcash").val(), ZTSCLevy: $("#LevyICEcash").val(), ProductId: ProductId, currencyId: currencyId },
        success: function (data) {
            debugger;
            $("#RiskDetail_Premium").val(parseFloat(data.Premium).toFixed(2));
            $("#RiskDetail_PremiumWithDiscount").val(parseFloat(data.Premium + data.Discount).toFixed(2));

            $("#RiskDetail_StampDuty").val(parseFloat(data.StamDuty).toFixed(2));
            $("#RiskDetail_ZTSCLevy").val(parseFloat(data.ZtscLevy).toFixed(2));

            $("#RiskDetail_PassengerAccidentCoverAmount").val(parseFloat(data.PassengerAccidentCoverAmount).toFixed(2));
            $("#RiskDetail_ExcessBuyBackAmount").val(parseFloat(data.ExcessBuyBackAmount).toFixed(2));
            $("#RiskDetail_RoadsideAssistanceAmount").val(parseFloat(data.RoadsideAssistanceAmount).toFixed(2));
            $("#RiskDetail_MedicalExpensesAmount").val(parseFloat(data.MedicalExpensesAmount).toFixed(2));
            $("#RiskDetail_MedicalExpensesPercentage").val(parseFloat(data.MedicalExpensesPercentage).toFixed(2));
            $("#RiskDetail_RoadsideAssistancePercentage").val(parseFloat(data.RoadsideAssistancePercentage).toFixed(2));
            $("#RiskDetail_ExcessBuyBackPercentage").val(parseFloat(data.ExcessBuyBackPercentage).toFixed(2));
            $("#RiskDetail_PassengerAccidentCoverAmountPerPerson").val(parseFloat(data.PassengerAccidentCoverAmountPerPerson).toFixed(2));
            $("#RiskDetail_ExcessAmount").val(parseFloat(data.ExcessAmount).toFixed(2));

            $("#RiskDetail_AnnualRiskPremium").val(parseFloat(data.AnnualRiskPremium).toFixed(2));
            $("#RiskDetail_TermlyRiskPremium").val(parseFloat(data.TermlyRiskPremium).toFixed(2));
            $("#RiskDetail_QuaterlyRiskPremium").val(parseFloat(data.QuaterlyRiskPremium).toFixed(2));
            $("#RiskDetail_Discount").val(parseFloat(data.Discount).toFixed(2));
            //$("#InsuranceId").val(data.Data.Response.Quotes[0].InsuranceID);

            var vehicleLicensefee = $("#RiskDetail_VehicleLicenceFee").val();
            var radioLicenseCost = $("#RiskDetail_RadioLicenseCost").val();
            if (radioLicenseCost == "")
                radioLicenseCost = 0;

            var penaltiesAmt = $("#RiskDetail_PenaltiesAmt").val();

            vehicleLicensefee = parseFloat(vehicleLicensefee).toFixed(2)
            radioLicenseCost = parseFloat(radioLicenseCost).toFixed(2)
            penaltiesAmt = parseFloat(penaltiesAmt).toFixed(2)

            var InsurncePremium = parseFloat(vehicleLicensefee + radioLicenseCost + penaltiesAmt).toFixed(2)

            var totalPremium = parseFloat(data.Premium + data.StamDuty + data.ZtscLevy).toFixed(2);

            totalPremium = parseFloat(totalPremium) + parseFloat(InsurncePremium);

            $("#SummaryDetail_TotalPremium").val(totalPremium.toFixed(2));


            //$("#NumberofPersons").val(data.NumberofPersons);
            //$("#AddThirdPartyAmount").val(data.AddThirdPartyAmount);

            if (data.Status == false) {

                toastr.warning(data.Message);
            }

            $(".loading-area").hide();

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (textStatus == 'timeout') {
                //doe iets
            } else if (textStatus == 'error') {
                //doe iets
            }
        } //EINDE error
    }); //EINDE ajax

}





function GetVehicleMake() {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/CustomerRegistration/GetVehicleMake1",
        data: "{}",
        dataType: "json",
        async: false,
        success: function (Result) {

            var $dropdown = $("#RiskDetail_MakeId");
            $dropdown.empty();

            $.each(Result, function (key, value) {
                $("#RiskDetail_MakeId").append($("<option></option>").val
                (value.MakeCode).html(value.MakeDescription));
            });
        },
        error: function (Result) {
            alert("Error");
        }
    });
}


function ValidateRadioLicense() {
    var isLicense = $("#RiskDetail_IncludeLicenseFee").is(':checked');
    var isRadioLicense = $("#RiskDetail_IncludeRadioLicenseCost").is(':checked');

    if (isRadioLicense && !isLicense) {
        toastr.warning("Only Radio license will be not allowed.");
        $("#RiskDetail_IncludeRadioLicenseCost").prop("checked", false);
    }
}


function GetVehicleModels(ModelName) {
    $.ajax({
        url: "/CustomerRegistration/GetVehicleModel",
        type: "POST",
        data: { makeCode: $("#RiskDetail_MakeId").val() },
        dataType: "json",
        success: function (result) {

            var $dropdown = $("#RiskDetail_ModelId");
            $dropdown.empty();
            $.each(result, function () {
                $dropdown.append($("<option />").val(this.ModelCode).text(this.ModelDescription));
            });
            if (ModelName != "") {
                $("#RiskDetail_ModelId option").filter(function () { return this.text == ModelName.toUpperCase(); }).attr('selected', true);
                //  $("#ModelId option:contains(" + ModelName.toUpperCase() + ")").attr('selected', 'selected');
                //$("#ModelId").attr("readonly","readonly");
                //  $("#ModelId").addClass("inputDisabled")
                // $("#ProductId").addClass("inputDisabled") // on 04-oct

                //$("#ProductId").attr("readonly","readonly");
            }
        }
    });
}





$(".IsLicenseDiskNeeded").click(function () {
    var isChecked = $(".IsLicenseDiskNeeded").is(":checked");
    if (isChecked == true) {

        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', true);
        $("#myAddressModal").modal('show');
    }
    else {

        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', false);
        $("#myAddressModal").modal('hide');
    }

});



$("#inputcheckbox").click(function () {
    var isChecked = $("#inputcheckbox").is(":checked");


    if (isChecked == true) {
     
        

        $("#inputaddress1").val($("#AddressLine1").val());
        $("#RiskDetail_LicenseAddress1").val($("#AddressLine1").val());
        $("#RiskDetail_LicenseDeliveryWay").val("Want the licence disk");

        $("#inputaddress1").addClass("inputDisabled");
        $("#inputaddress2").val($("#AddressLine2").val());
        $("#RiskDetail_LicenseAddress2").val($("#AddressLine2").val());

        $("#inputaddress2").addClass("inputDisabled");
        $("#inputcity").val($("#Customer_City").val());
        $("#RiskDetail_LicenseCity").val($("#Customer_City").val());
        $("#inputcity").addClass("inputDisabled");
    }
    else { 
        //alert("unChecked case");
        //1. Emptying controls
   
        $("#inputaddress1").val("");
        $("#RiskDetail_LicenseAddress1").val("");
        $("#RiskDetail_LicenseDeliveryWay").val("");
        $("#inputaddress1").removeClass("inputDisabled");
        $("#inputaddress2").val("");
        $("#RiskDetail_LicenseAddress2").val("");
        $("#inputaddress2").removeClass("inputDisabled");
        $("#inputcity").val("");
        $("#RiskDetail_LicenseCity").val("");
        $("#inputcity").removeClass("inputDisabled");
    }

});


$(".ShowSelfCollect").click(function () {

    debugger;

    var isChecked = $(".ShowSelfCollect").is(":checked");
    if (isChecked == true) {

        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', true);

        $("#myAddressModal").modal('show');
        $("#inputaddress1").val("Genetic Financial Services ZB Centre 4th Floor South Wing");
        $("#RiskDetail_LicenseAddress1").val("Genetic Financial Services ZB Centre 4th Floor South Wing");

        $("#RiskDetail_LicenseDeliveryWay").val("SelfCollect");

        $("#inputaddress1").addClass("inputDisabled");
        $("#inputaddress2").val("cnr First Street & Kwame Nkrumah Avenue Harare");
        $("#RiskDetail_LicenseAddress2").val("cnr First Street & Kwame Nkrumah Avenue Harare");

        $("#inputaddress2").addClass("inputDisabled");
        $("#inputcity").val("Harare");
        $("#RiskDetail_LicenseCity").val("Harare");
        $("#inputcity").addClass("inputDisabled");

    }
    else {

        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', true);

        $("#myAddressModal").modal('hide');

        $("#inputaddress1").val("");
        $("#RiskDetail_LicenseAddress1").val("");
        $("#RiskDetail_LicenseDeliveryWay").val("");
        $("#inputaddress1").removeClass("inputDisabled");
        $("#inputaddress2").val("");
        $("#RiskDetail_LicenseAddress2").val("");
        $("#inputaddress2").removeClass("inputDisabled");
        $("#inputcity").val("");
        $("#RiskDetail_LicenseCity").val("");
        $("#inputcity").removeClass("inputDisabled");

    }
});




$(".ShowGoogleMap").click(function () {

    var isChecked = $(".ShowGoogleMap").is(":checked");

    if (isChecked)
    {
        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', true);
        $("#RiskDetail_LicenseDeliveryWay").val("collect on Alm");
    }
    else
    {
        $("#RiskDetail.IsLicenseDiskNeeded").prop('checked', false);
        $("#RiskDetail_LicenseDeliveryWay").val("");
    }
});





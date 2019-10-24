$(document).ready(function () {
})

function GoToProductDetail(json) {
    debugger

    if (json.IsError == true) {
       window.location.href = '/CustomerRegistration/ProductDetail';
    }
    else {
        var errorMessage = json.error;
        if (errorMessage != null && errorMessage != '') {
            toastr.error(errorMessage)
            if (errorMessage == "Sucessfully update")
            {
                window.location.href = '/Account/QuotationList';
            }
            
        }
    }
}



function GoProNextDetails(json) {
    debugger;
    var errorMessage = json.error;
    if (json.IsError == true) {
        if (errorMessage != null && errorMessage != '') {
            toastr.error(errorMessage)
        }
    }

    if (errorMessage == "Your email have been updated sucessfully") {
        debugger;
        toastr.success(errorMessage)
        window.location.href = '/Endorsement/EndorsementInsertRiskDetails';
    }
}

function GoToNextDetail(json) {
    debugger;
    var errorMessage = json.error;
    if (json.IsError == true) {
        if (errorMessage != null && errorMessage != '') {
            toastr.error(errorMessage)
        }
    }

    if (errorMessage == "Sucessfully update") {            
        toastr.error(errorMessage)      
        window.location.href = '/Account/EndorsementRiskDetails';
    }
}



function GoToRiskDetail(json) {   
    debugger;
    if (json.Status == true) {
        window.location.href = '/CustomerRegistration/RiskDetail/' + json.Id;
    }
    else {
        toastr.error(json.Message);
    }
}

function GoToSummaryDetail(json) {
    if (json == true) {
        window.location.href = '/CustomerRegistration/SummaryDetail';
    }
   
}

function GoToPaymentDetail(json) {
    window.location.href = '/CustomerRegistration/PaymentDetail';
}
//Renew 16 Jan


function GoToProductsDetails(json) {
    debugger

    if (json.IsError == true) {
        window.location.href = '/Renew/RiskDetail';
    }
    else {
        var errorMessage = json.error;
        if (errorMessage != null && errorMessage != '') {
            toastr.error(errorMessage)
            if (errorMessage == "Please Try Again") {
                window.location.href = '/Renew/Index';
            }

        }
    }
}

function GoToRenewTo(json) {
    debugger

    if (json.IsError == true) {
        window.location.href = '/Renew/Viewriskdetail';
    }
    else {
        var errorMessage = json.error;
        if (errorMessage != null && errorMessage != '') {
            toastr.error(errorMessage)
            if (errorMessage == "Sucessfully update") {
                window.location.href = '/Account/QuotationList';
            }

        }
    }
}




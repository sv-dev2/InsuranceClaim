using InsuranceClaim.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Insurance.Service;
using System.Globalization;
using System.IO;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;


namespace InsuranceClaim.Controllers
{
    public class PaypalController : Controller
    {
        private ApplicationUserManager _userManager;
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

        string _pdfPath = "";
        string _pdfCode = "";
        string _iceCashRef = "";
        string _webCustomerRoleId = "fe19c887-f8a9-4353-939f-65e19afe0c4w";

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        //
        // GET: /Paypal/

        public ActionResult Index(int id)
        {
            // ApproveVRNToIceCash(6153099, 1);
            return View();
        }


        public ActionResult IceCashPayment(int id = 0, string amount = "0")
        {
            if (id != 0)
            {
                TempData["SummaryId"] = id;
            }
            else if (id == 0)
            {
                if (TempData["PaymentDetail"] != null)
                    id = Convert.ToInt32(TempData["PaymentDetail"]);
            }


            int paymentMehtod = 0;
            if (TempData["PaymentMethodId"] != null)
            {
                paymentMehtod = Convert.ToInt32(TempData["PaymentMethodId"]);
            }


            IceCashCardDetailModel iceCashmodel = new IceCashCardDetailModel { SummaryId = id, Amount = Convert.ToDecimal(amount), PaymentMethod = paymentMehtod };

            Session["IceCashPayment"] = iceCashmodel;
            var iceCashPaymentUrl = System.Configuration.ConfigurationManager.AppSettings["IceCash"];

            IceCashModel model = new IceCashModel();

            model.partner_id = "20045172";
            var amounts = Math.Round(Convert.ToDecimal(amount), 0); // Output: 1;
            model.amount = amounts;
            model.client_reference = Guid.NewGuid();
            // model.IceCashRequestUrl = "http://test.api.ice.cash/payments/test_send_request";
            ViewBag.IceCashRequestUrl = "http://test.api.ice.cash/payments/request";
            //  model.IceCashRequestUrl = "https://api.icecash.co.zw/payments/request";
            //https://api.icecash.co.zw/payments/request
            model.success_url = iceCashPaymentUrl + "/Paypal/success_url";
            model.failed_url = iceCashPaymentUrl + "/Paypal/failed_url";
            model.results_url = iceCashPaymentUrl + "/Paypal/results_url";
            model.details = "Trasaction details";

            ICEcashService service = new ICEcashService();
            model.check_hash = service.check_hash(model);

            return View(model);
        }

        public ActionResult success_url()
        {
            if (Session["IceCashPayment"] != null)
            {
                var IceCashPaymentDetails = (IceCashCardDetailModel)Session["IceCashPayment"];
                SaveDetailList(IceCashPaymentDetails.SummaryId, "", Convert.ToString(IceCashPaymentDetails.PaymentMethod));
            }
            return RedirectToAction("ThankYou");
        }

        public ActionResult failed_url(SummaryDetailModel model)
        {
            if (Session["PayNowSummmaryId"] != null)
            {
                model.Id = (int)Session["PayNowSummmaryId"];
            }
            return View(model);
        }


        public ActionResult Panow(int Id = 0)
        {
            string invoiceNumber = "";

            var summaryDetail = InsuranceContext.SummaryDetails.Single(Id);

            if (summaryDetail != null)
            {

                var query = "select top 1 PolicyDetail.PolicyNumber as InvoiceNumber from SummaryDetail ";
                query += " join SummaryVehicleDetail on SummaryDetail.Id = SummaryVehicleDetail.SummaryDetailId ";
                query += " join VehicleDetail on VehicleDetail.Id = SummaryVehicleDetail.VehicleDetailsId ";
                query += " join PolicyDetail on PolicyDetail.Id = VehicleDetail.PolicyId where SummaryDetail.Id=" + Id;

                var detail = InsuranceContext.Query(query).Select(x => new PolicyDetail()
                {
                    PolicyNumber = x.InvoiceNumber,

                }).FirstOrDefault();

                if(detail!=null)
                    invoiceNumber = detail.PolicyNumber;
                
                if(Session["PollUrl"]!=null) // it represent
                {
                    CustomerRegistrationController customer = new CustomerRegistrationController();
                    var payNow = customer.PayNow(summaryDetail.Id, invoiceNumber, Convert.ToInt32(summaryDetail.PaymentMethodId), Convert.ToDecimal(summaryDetail.TotalPremium));

                    if (payNow.IsSuccessPayment)
                    {
                        Session["PayNowSummmaryId"] = Id;
                        Session["PollUrl"] = payNow.PollUrl;
                        return Redirect(payNow.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("failed_url", "Paypal");
                    }
                }
                
                



            }

            return RedirectToAction("failed_url");
        }




        public ActionResult results_url()
        {

            return View();
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            if (!isValid(model.ExpiryDate))
            {
                ModelState.AddModelError("PaymentError", "Card expire date is not valid");
                return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId, erroMsg = "Card expire date is not valid." });
            }


            Session["CardDetail"] = model;
            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = InsuranceContext.SummaryDetails.Single(model.SummaryDetailId);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.SummaryDetailId}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            //var summaryDetail = (SummaryDetailModel)Session["SummaryDetailed"];
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            //var policy = (PolicyDetail)Session["PolicyData"];
            //var customer = (CustomerModel)Session["CustomerDataModal"];
            var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);

            //var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(model.SummaryDetailId);

            double totalPremium = Convert.ToDouble(summaryDetail.TotalPremium);
            //var term = summaryDetail.PaymentTermId;
            //if (term == Convert.ToInt32(ePaymentTerm.Monthly))
            //{
            //    totalPremium = Math.Round(totalPremium / 12, 2);
            //}
            //else if (term == Convert.ToInt32(ePaymentTerm.Quarterly))
            //{
            //    totalPremium = Math.Round(totalPremium / 4, 2);
            //}
            //else if (term == Convert.ToInt32(ePaymentTerm.Termly))
            //{
            //    totalPremium = Math.Round(totalPremium / 3, 2);
            //}
            //check if single decimal place
            string zeros = string.Empty;
            try
            {
                var percision = totalPremium.ToString().Split('.');
                var length = 2 - percision[1].Length;
                for (int i = 0; i < length; i++)
                {
                    zeros += "0";
                }
            }
            catch
            {
                zeros = ".00";

            }

            #region test code PAYpal


            //Item item = new Item();
            //item.name = product.ProductName;            
            //item.currency = currency.Name;
            //item.price = totalPremium.ToString() + zeros;
            //item.quantity = "1";
            //item.sku = "sku";

            ////Now make a List of Item and add the above item to it
            ////you can create as many items as you want and add to this list
            //List<Item> itms = new List<Item>();
            //itms.Add(item);
            //ItemList itemList = new ItemList();
            //itemList.items = itms;

            ////Address for the payment
            //Address billingAddress = new Address();
            //billingAddress.city = "NewYork";
            //billingAddress.country_code = "US";
            //billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            //billingAddress.postal_code = "43210";
            //billingAddress.state = "NY";


            ////Now Create an object of credit card and add above details to it
            //PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            //crdtCard.billing_address = billingAddress;
            //crdtCard.cvv2 = model.CVC;
            //crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            //crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);
            //crdtCard.first_name = model.NameOnCard;
            ////crdtCard.last_name = "Thakur";
            //crdtCard.number = model.CardNumber;
            //crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            //// Specify details of your payment amount.
            //Details details = new Details();
            //details.shipping = "0.00";
            //details.subtotal = totalPremium.ToString() + zeros;
            //details.tax = "0.00";

            //// Specify your total payment amount and assign the details object
            //Amount amnt = new Amount();
            //amnt.currency = currency.Name;
            //// Total = shipping tax + subtotal.
            //amnt.total = totalPremium.ToString() + zeros;            
            //// amnt.details = details;

            //// Now make a trasaction object and assign the Amount object
            //Transaction tran = new Transaction();
            //tran.amount = amnt;
            //tran.description = "Online vehicle insurance purchased";
            //tran.item_list = itemList;
            //tran.invoice_number = summaryDetail.DebitNote;

            //// Now, we have to make a list of trasaction and add the trasactions object
            //// to this list. You can create one or more object as per your requirements

            //List<Transaction> transactions = new List<Transaction>();
            //transactions.Add(tran);

            //// Now we need to specify the FundingInstrument of the Payer
            //// for credit card payments, set the CreditCard which we made above

            //FundingInstrument fundInstrument = new FundingInstrument();
            //fundInstrument.credit_card = crdtCard;

            //// The Payment creation API requires a list of FundingIntrument

            //List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            //fundingInstrumentList.Add(fundInstrument);

            //PayerInfo pi = new PayerInfo();
            //pi.email = "noemail@noemail.com";
            //pi.first_name = "Stack";
            //pi.last_name = "Overflow";
            //pi.shipping_address = new ShippingAddress
            //{
            //    city = "San Mateo",
            //    country_code = "US",
            //    line1 = "SO TEST",
            //    line2 = "",
            //    postal_code = "94002",
            //    state = "CA",
            //};

            //// Now create Payer object and assign the fundinginstrument list to the object
            //Payer payr = new Payer();
            //payr.funding_instruments = fundingInstrumentList;
            //payr.payment_method = "credit_card";
            //payr.payer_info = pi;

            //// finally create the payment object and assign the payer object & transaction list to it
            //Payment pymnt = new Payment();
            //pymnt.intent = "sale";
            //pymnt.payer = payr;
            //pymnt.transactions = transactions;

            #endregion

            List<Item> itms = new List<Item>();
            foreach (var vehicledetail in SummaryVehicleDetails.ToList())
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(vehicledetail.VehicleDetailsId);
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                Item item = new Item();
                item.name = make.MakeDescription + "/" + _model.ModelDescription;
                item.currency = "USD";
                item.price = Convert.ToString((_vehicle.Premium + _vehicle.StampDuty + _vehicle.ZTSCLevy + _vehicle.VehicleLicenceFee + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? _vehicle.RadioLicenseCost : 0.00m)) - _vehicle.BalanceAmount);
                item.quantity = "1";
                item.sku = _vehicle.RegistrationNo;

                itms.Add(item);
            }



            Session["itemData"] = itms;


            ItemList itemList = new ItemList();
            itemList.items = itms;

            Address billingAddress = new Address();
            billingAddress.city = customer.City;
            billingAddress.country_code = "US";
            billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            billingAddress.line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2;

            if (customer.Zipcode == null)
            {
                billingAddress.postal_code = "00263";
            }
            else
            {
                billingAddress.postal_code = customer.Zipcode;
            }

            billingAddress.state = customer.NationalIdentificationNumber;

            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);

            //crdtCard.first_name = "fgdfg";
            //crdtCard.last_name = "rffd";

            var name = model.NameOnCard.Split(' ');
            if (name.Length == 1)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = null;
            }
            if (name.Length == 2)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = name[1];
            }

            crdtCard.number = model.CardNumber; //use some other test number if it fails
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            Details details = new Details();
            details.tax = "0";
            details.shipping = "0";
            details.subtotal = (summaryDetail.AmountPaid.ToString().IndexOf('.') > -1 ? summaryDetail.AmountPaid.ToString() : summaryDetail.AmountPaid.ToString() + zeros);

            Amount amont = new Amount();
            amont.currency = "USD";
            amont.total = (summaryDetail.AmountPaid.ToString().IndexOf('.') > -1 ? summaryDetail.AmountPaid.ToString() : summaryDetail.AmountPaid.ToString() + zeros);
            amont.details = details;

            Transaction tran = new Transaction();
            tran.amount = amont;
            tran.description = "trnx desc";
            tran.item_list = itemList;

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);



            var User = UserManager.FindById(customer.UserID);
            PayerInfo pi = new PayerInfo();
            pi.email = User.Email;
            pi.first_name = customer.FirstName;
            pi.last_name = customer.LastName;
            pi.shipping_address = new ShippingAddress
            {
                city = customer.City,
                country_code = "US",
                line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1,
                line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2,
                postal_code = customer.Zipcode,
                state = customer.NationalIdentificationNumber,
            };



            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";
            payr.payer_info = pi;

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactions;

            try
            {
                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                //Code for the configuration class is provided next

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                Payment createdPayment = pymnt.Create(apiContext);
                //paymentInformations.PaymentTransctionId = createdPayment.id;


                // condition for failure
                if (createdPayment != null && string.IsNullOrEmpty(createdPayment.id))
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
                }

                decimal amount = 0;

                if (createdPayment != null && createdPayment.transactions.Count() > 0)
                {
                    var trasactinList = createdPayment.transactions;

                    foreach (var item in trasactinList)
                    {
                        if (item.amount != null)
                        {
                            amount = Convert.ToDecimal(item.amount.total);
                            break;
                        }
                    }

                    if (amount < 1)
                    {
                        ModelState.AddModelError("PaymentError", "Payment not approved");
                        return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId, erroMsg = "Payment not approved" });
                    }
                }


                Session["PaymentId"] = createdPayment.id;

                //if the createdPayment.State is "approved" it means the payment was successfull else not
                creatInvoice(User, customer);


                // ApproveVRNToIceCash(model.SummaryDetailId);

                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId, erroMsg = "Payment not approved" });
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                var error = json_serializer.DeserializeObject(((PayPal.ConnectionException)ex).Response);
                return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId, erroMsg = "Payment not approved" });
            }

            return RedirectToAction("SaveDetailList", "Paypal", new { id = model.SummaryDetailId });
        }


        private bool isValid(string dateString)
        {
            DateTime dt1 = Convert.ToDateTime(dateString);
            DateTime dt2 = DateTime.Now.Date;
            int result = DateTime.Compare(dt1, dt2);
            if (result < 0)
                return false;
            else
                return true;
        }


        class Test
        {
            String test;

            String getTest() { return test; }
            void setTest(String test) { this.test = test; }

        }
        private ActionResult creatInvoice(ApplicationUser User, Customer customer)
        {
            APIContext apiContext = Configuration.GetAPIContext();

            var data = (List<Item>)Session["itemData"];

            var invoice = new Invoice()
            {

                merchant_info = new MerchantInfo()
                {
                    email = "ankit.dhiman-facilitator@kindlebit.com",
                    first_name = "Genetic Financial Services",
                    last_name = "ZB Centre, 4th Floor, South Wing, cnr First Street & Kwame Nkrumah Avenue, Harare",
                    business_name = "Insurance Claim",
                    website = "insuranceclaim.com",
                    //tax_id = "47-4559942",

                    phone = new Phone()
                    {
                        country_code = "001",
                        national_number = "08677007491"
                    },
                    address = new InvoiceAddress()
                    {
                        line1 = customer.AddressLine1,
                        city = customer.AddressLine2,
                        state = customer.City + "/ " + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"

                    }
                },

                billing_info = new List<BillingInfo>()
                            {
                                new BillingInfo()
                                {

                                    email = User.Email,//"amit.kamal@kindlebit.com",
                                    first_name=customer.FirstName,
                                    last_name=customer.LastName
                                }
                            },

                items = new List<InvoiceItem>()
                            {
                                new InvoiceItem()
                                {
                                    name = data[0].name,
                                    quantity = 1,
                                    unit_price = new PayPal.Api.Currency()
                                    {
                                        currency = "USD",
                                        value =data[0].price

                                    },
                                },
                            },
                note = "Your  Invoce has been created successfully.",

                shipping_info = new ShippingInfo()
                {
                    first_name = customer.FirstName,
                    last_name = customer.LastName,
                    business_name = "InsuranceClaim",
                    address = new InvoiceAddress()
                    {
                        //line1 = userdata.State.ToString(),
                        city = customer.City,
                        state = customer.City + "/" + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"
                    }
                }
            };
            var createdInvoice = invoice.Create(apiContext);
            Session["InvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }
        public ActionResult PaymentWithPaypal()
        {
            //getting the apiContext as earlier
            APIContext apiContext = Configuration.GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist

                    //it is returned by the create function call of the payment class

                    // Creating a payment

                    // baseURL is the url on which paypal sendsback the data.

                    // So we have provided URL of this controller only

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPayPal?";

                    //guid we are generating for storing the paymentID received in session

                    //after calling the create function and it is used in the payment execution

                    var guid = Convert.ToString((new Random()).Next(100000));

                    //CreatePayment function gives us the payment approval url

                    //on which payer is redirected for paypal acccount payment

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This section is executed when we have received all the payments parameters

                    // from the previous call to the function Create

                    // Executing a payment

                    var guid = Request.Params["guid"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error" + ex.Message);
                return View("FailureView");
            }

            return View("SuccessView");
        }

        private PayPal.Api.Payment payment;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            itemList.items.Add(new Item()
            {
                name = "Item Name",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            // similar as we did for credit card, do here and create details object
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = "5"
            };

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = "7", // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = "your invoice number",
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);

        }
        //public async Task<ActionResult> SaveDetailList(Int32 id, string invoiceNumber = "")
        public string SaveQRCode(string Policyno)
        {
            string path = "";
            try
            {
                var urlPath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                Insurance.Domain.QRCode Codes = new Insurance.Domain.QRCode();

                //var Policy =Convert.ToString (TempData["Registrationno"]);

                using (MemoryStream ms = new MemoryStream())
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(Policyno, QRCodeGenerator.ECCLevel.Q);
                    QRCoder.QRCode QrCode = new QRCoder.QRCode(qrCodeData);
                    using (Bitmap bitMap = QrCode.GetGraphic(6))
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        Base64ToImage(Convert.ToBase64String(ms.ToArray())).Save(Server.MapPath("~/QRCode/" + Policyno + ".jpg"));
                        //path = "/QRCode/" + Policyno + ".jpg";
                        path = urlPath + "/QRCode/" + Policyno + ".jpg";
                    }

                    //path = Request.Url.Scheme + System.Uri.SchemeDelimiter + "/" + Request.Url.Host + "/QRCode/" + Policyno + ".jpg";


                    //LinkedResource lr = new LinkedResource("path",MediaTypeNames.Image.Jpeg);
                    //lr.ContentId = "qrImage";
                    //path = Server.MapPath("~/QRCode/" + Policyno + ".jpg");
                    //LinkedResource lr = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                    //lr.ContentId = "image1";
                    //AlternateView av = AlternateView.CreateAlternateViewFromString(str, null, MediaTypeNames.Text.Html);
                    //lr.ContentId = "image1";
                    //av.LinkedResources.Add(lr);
                    //message.AlternateViews.Add(av);


                    // path = "https://gene.co.zw/QRCode/" + Policyno + ".jpg";
                    // path = Url.ss "http://geneinsureclaim.kindlebit.com/QRCode"  + Policyno + ".jpg";

                    //path = Request.Url.Authority+"/QRCode/" + Policyno + ".jpg";

                    // path = "/QRCode/" + Policyno + ".jpg";




                    Codes.PolicyNo = Policyno;
                    Codes.Qrcode = Policyno;
                    Codes.ReadBy = "";
                    Codes.Deliverto = "";
                    Codes.Createdon = DateTime.Now;
                    Codes.Comment = "";

                    //   var QRCodedata = Mapper.Map<QRCode, QRCode>(Codes);
                    InsuranceContext.QRCodes.Insert(Codes);
                }

            }
            catch (Exception ex)
            {

            }

            return path;
        }


        public System.Drawing.Image Base64ToImage(string base64String)
        {

            byte[] imageBytes = Convert.FromBase64String(base64String.ToString());
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }

        public async Task<ActionResult> SaveDetailList(Int32 id, string invoiceNumber = "", string Paymentid = "")
        {
            //var PaymentId = Session["PaymentId"];
            //var InvoiceId = Session["InvoiceId"];

            if (Session["PollUrl"] != null)
            {
                string Integration_ID = System.Configuration.ConfigurationManager.AppSettings["PayNowIntegration_ID"];
                string Integration_Key = System.Configuration.ConfigurationManager.AppSettings["PayNowIntegration_Key"];

                var paynow = new Webdev.Payments.Paynow(Integration_ID, Integration_Key);
                var status = paynow.PollTransaction((string)Session["PollUrl"]);

                //var resPa=  status.GetData();

                //  var data = resPa["status"];

                if (!status.Paid())
                {
                    return RedirectToAction("failed_url");
                }

            }

            if (id == 0)
            {
                id = Convert.ToInt32(TempData["SummaryId"]);
                if (id == 0)
                {
                    id = Convert.ToInt32(TempData["PaymentDetail"]);
                }
            }

            SummaryDetailService detailService = new SummaryDetailService();
            var currencylist = detailService.GetAllCurrency();
            string currencyName = "$";

            string PaymentMethod = "";
            if (Paymentid == "1")
            {
                PaymentMethod = "CASH";
            }
            //else if (Paymentid == "2")
            //{
            //    PaymentMethod = "MasterCard";
            //}
            else if (Paymentid == "3")
            {
                PaymentMethod = "paynow";
                // PaymentMethod = "EcoCash";
            }
            else if (Paymentid == "6")
            {
                // PaymentMethod = "paynow";

                PaymentMethod = "Zimswitch";
            }
            else if (Paymentid == "")
            {
                PaymentMethod = "CASH";
            }


            //if (Paymentid == Convert.ToString((int)paymentMethod.ecocash))
            //{
            //    var result = ApproveVRNToIceCash(id, Convert.ToInt16(Paymentid)); // for now comment

            //    if (result != "Approved")
            //    {
            //        return RedirectToAction("SummaryDetail", "CustomerRegistration", new { summaryDetailId = id, paymentError = result });
            //    }
            //    else
            //    {

            //    }
            //}

            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);

            if (summaryDetail != null && summaryDetail.isQuotation)
            {
                summaryDetail.isQuotation = false;
                InsuranceContext.SummaryDetails.Update(summaryDetail);
            }

            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);

            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            //Generate QR Code
            var path = SaveQRCode(policy.PolicyNumber);

            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));
            // var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);
            var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(id);
            var user = UserManager.FindById(customer.UserID);

            var DebitNote = summaryDetail.DebitNote;
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            objSaveDetailListModel.CurrencyId = policy.CurrencyId;
            objSaveDetailListModel.PolicyId = vehicle.PolicyId;
            objSaveDetailListModel.CustomerId = summaryDetail.CustomerId.Value;
            objSaveDetailListModel.SummaryDetailId = id;
            objSaveDetailListModel.DebitNote = summaryDetail.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;
            //objSaveDetailListModel.PaymentId = PaymentId == null ? "CASH" : PaymentId.ToString();
            //objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
            objSaveDetailListModel.PaymentId = PaymentMethod;
            objSaveDetailListModel.InvoiceId = invoiceNumber;

            objSaveDetailListModel.CreatedBy = customer.Id;
            objSaveDetailListModel.CreatedOn = DateTime.Now;
            //objSaveDetailListModel.InvoiceNumber = invoiceNumber;
            objSaveDetailListModel.InvoiceNumber = policy.PolicyNumber;

            if (Session["PollUrl"] != null)
                objSaveDetailListModel.PollURL = Convert.ToString(Session["PollUrl"]); // paynow payment details url

            Session["PollUrl"] = null;


            List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();

            //if (paymentInformations == null)
            //{


            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


            var dbPaymentInformation = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId='{id}'");
            if (dbPaymentInformation == null)
            {
                InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
            }
            else
            {
                objSaveDetailListModel.Id = dbPaymentInformation.Id;
                InsuranceContext.PaymentInformations.Update(objSaveDetailListModel);
            }

            if (Paymentid != Convert.ToString((int)paymentMethod.ecocash))
            {

                if (string.IsNullOrEmpty(Paymentid))
                    Paymentid = "1";

                string res = ApproveVRNToIceCash(id, Convert.ToInt16(Paymentid));

            }

            if (!userLoggedin)
            {
                string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                var attachementFile1 = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "WelCome Letter ");
                List<string> _attachements = new List<string>();
                _attachements.Add(attachementFile1);
                //_attachements.Add(_yAtter);


                if (customer.IsCustomEmail) // if customer has custom email
                {
                    objEmailService.SendEmail(LoggedUserEmail(), "", "", "Account Creation", Body, _attachements);
                }
                else
                {
                    objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, _attachements);
                }



                string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you.";
                var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, body);

                SmsLog objsmslog = new SmsLog()
                {
                    Sendto = user.PhoneNumber,
                    Body = body,
                    Response = result,
                    CreatedBy = customer.Id,
                    CreatedOn = DateTime.Now
                };

                InsuranceContext.SmsLogs.Insert(objsmslog);
            }

            //var data = (List<Item>)Session["itemData"];
            //if (data != null)
            //{
            //var totalprem = data.Sum(x => Convert.ToDecimal(x.price));

            // string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentEmail.cshtml"; 24_jan_2019

            //var currencyDetails = currencylist.FirstOrDefault(c => c.Id == vehicle.CurrencyId);
            //if(currencyDetails!=null)

            currencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);

            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
            var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
                .Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName)
                .Replace("#LastName#", customer.LastName)
                .Replace("#AccountName#", customer.FirstName + ", " + customer.LastName)
                .Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2)
                .Replace("#currencyName#", currencyName)
                .Replace("#QRpath#", path)
                .Replace("#Amount#", Convert.ToString(summaryDetail.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Cash" : (summaryDetail.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

            #region Payment Email
            var attachementFile = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Invoice");
            //var yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            #region Payment Email
            //objEmailService.SendEmail(User.Identity.Name, "", "", "Payment", Body2, attachementFile);
            #endregion

            List<string> attachements = new List<string>();
            attachements.Add(attachementFile);


            if (customer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Invoice", Body2, attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Invoice", Body2, attachements);
            }



            #endregion

            #region Send Payment SMS

            // done
            //   string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Your payment of " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " has been received. Policy number is : " + policy.PolicyNumber + "\n" + "\nThanks.";


            //string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Please pay " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " upon receiving your policy to merchant code 249341. Policy number is " + policy.PolicyNumber + "\n" + "\nThanks.";

            string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Please pay " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " upon receiving your policy to merchant code 249341. Policy number is " + policy.PolicyNumber + "\n" + "\nThanks.";



            var Recieptresult = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, Recieptbody);

            SmsLog objRecieptsmslog = new SmsLog()
            {
                Sendto = user.PhoneNumber,
                Body = Recieptbody,
                Response = Recieptresult,
                CreatedBy = customer.Id,
                CreatedOn = DateTime.Now
            };

            InsuranceContext.SmsLogs.Insert(objRecieptsmslog);

            #endregion


            #region Send License PDFVerficationcode SMS

            if (_pdfPath != "" && _pdfCode != "")
            {
                string RecieptbodyPdf = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Your license pdf verifation code is: " + _pdfCode + "\n" + "\nThanks.";
                var RecieptresultPdf = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, RecieptbodyPdf);
                SmsLog objRecieptsmslogPdf = new SmsLog()
                {
                    Sendto = user.PhoneNumber,
                    Body = RecieptbodyPdf,
                    Response = RecieptresultPdf,
                    CreatedBy = customer.Id,
                    CreatedOn = DateTime.Now
                };
                InsuranceContext.SmsLogs.Insert(objRecieptsmslog);
            }
            #endregion



            foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
            {
                var itemVehicle = InsuranceContext.VehicleDetails.Single(itemSummaryVehicleDetails.VehicleDetailsId);
                //  MiscellaneousService.AddLoyaltyPoints(summaryDetail.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(itemVehicle), user.Email, filepath);      disable loyalty on 02_sep_2019
                ListOfVehicles.Add(itemVehicle);
            }


            #region Payment PDF
            //MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Reciept Payment");
            #endregion
            //}

            decimal totalpaymentdue = 0.00m;

            //if (vehicle.PaymentTermId == 1)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium;
            //}
            //else if (vehicle.PaymentTermId == 4)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 3;
            //}
            //else if (vehicle.PaymentTermId == 3)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 4;
            //}

            string Summeryofcover = "";
            var RoadsideAssistanceAmount = 0.00m;
            var MedicalExpensesAmount = 0.00m;
            var ExcessBuyBackAmount = 0.00m;
            var PassengerAccidentCoverAmount = 0.00m;
            var ExcessAmount = 0.00m;
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };


            foreach (var item in ListOfVehicles)
            {
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);

                var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);

                //string   paymentTermsName = (item.PaymentTermId == 1 ? paymentTermVehicel.Name + "(1 Year)" : paymentTermVehicel.Name + " Months");

                string paymentTermsName = "";
                if (item.PaymentTermId == 1)
                    paymentTermsName = "Annual";
                else if (item.PaymentTermId == 4)
                    paymentTermsName = "Termly";
                else
                    paymentTermsName = paymentTermVehicel.Name + " Months";


                string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");

                currencyName = detailService.GetCurrencyName(currencylist, item.CurrencyId);

                //Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + item.SumInsured + "</td><td style='padding: 7px 10px; font - size:15px;'>" + (item.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td style='padding: 7px 10px; font - size:15px;'>$0.00</td><td style='padding: 7px 10px; font - size:15px;'>$" + Convert.ToString(item.Excess) + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + Convert.ToString(item.Premium) + "</td></tr>";
                Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td> <td> " + item.CoverNote + " </td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + Convert.ToString(item.Premium + item.Discount) + "</font></td></tr>";
            }
            //for (int i = 0; i < SummaryVehicleDetails.Count; i++)
            //{
            //    var _vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[i].VehicleDetailsId);

            //}

            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicle.PaymentTermId);
            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
            //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summaryDetail.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summaryDetail.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(vehicle.ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(vehicle.MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(vehicle.PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(vehicle.RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(vehicle.Discount));
            //  var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(vehicle.Discount)).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##",Convert.ToString(vehicle.VehicleLicenceFee));

            var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).
                Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber)
                .Replace("##currencyName##", currencyName)
                .Replace("##QRpath##", path)
                .Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email)
                .Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy"))
                .Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy"))
                .Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover)
                .Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)"))
                .Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty))
                .Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies))
                .Replace("##PremiumDue##", Convert.ToString(ListOfVehicles.Sum(x => x.Premium) + ListOfVehicles.Sum(x => x.Discount)))
                .Replace("##PenaltiesAmt##", Convert.ToString(ListOfVehicles.Sum(x => x.PenaltiesAmt)))
                .Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount))
                .Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount))
                .Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount)))
                .Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

            //var attachementFile = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Reciept Payment");

            #region Invoice PDF
            var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Schedule-motor");
            var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";


            #endregion

            #region new policy

            // user

            var role = user.Roles.FirstOrDefault();
            if(role.RoleId== _webCustomerRoleId)
            {
                MiscellaneousService.SendEmailNewPolicy(customer.FirstName + " " + customer.LastName, customer.AddressLine1, customer.AddressLine2, policy.PolicyNumber, summaryDetail.TotalPremium, summaryDetail.PaymentTermId, PaymentMethod, ListOfVehicles);
            }


            #endregion

            List<string> __attachements = new List<string>();
            __attachements.Add(attacehmetnFile);
            //if (!userLoggedin)
            //{
            __attachements.Add(Atter);
            //}

            #region Invoice EMail


            if (customer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Schedule-motor", Bodyy, __attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Schedule-motor", Bodyy, __attachements);
            }



            #endregion

            //}

            #region Remove  All Sessions
            try
            {
                Session.Remove("CustomerDataModal");
                Session.Remove("PolicyData");
                Session.Remove("VehicleDetails");
                Session.Remove("SummaryDetailed");
                Session.Remove("CardDetail");
                Session.Remove("issummaryformvisited");
                Session.Remove("PaymentId");
                Session.Remove("InvoiceId");
            }
            catch (Exception ex)
            {
                Session.Remove("InvoiceId");
                Session.Remove("PaymentId");
                Session.Remove("issummaryformvisited");
                Session.Remove("CardDetail");
                Session.Remove("SummaryDetailed");
                Session.Remove("VehicleDetails");
                Session.Remove("PolicyData");
                Session.Remove("CustomerDataModal");
            }

            #endregion

            var IswebCustomer = User.IsInRole("Web Customer");

            if (_pdfPath != "" && !IswebCustomer)
            {
                TempData["file"] = System.Configuration.ConfigurationManager.AppSettings["urlPath"] + _pdfPath;
                TempData["vehicleId"] = vehicle.Id;
            }

            //var IsStaff = User.IsInRole("Staff");
            //if(IsStaff && _pdfPath!="")
            //{
            //    return RedirectToAction("CertificateSerialNumber", "CustomerRegistration", new { VehicleId = vehicle.Id });
            //}

            


            return RedirectToAction("ThankYou");
        }


       

      



        public string LoggedUserEmail()
        {
            string email = "";
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                email = _User.Email;
            }
            return email;

        }
        public string ApproveVRNToIceCash(int id, int paymentMethod)
        {
            #region update  TPIQuoteUpdate
            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
            string result = "";

            try
            {
                var tokenObject = new ICEcashTokenResponse();
                var PartnerToken = "";

                var customerDetails = new Customer();
                ICEcashService iceCash = new ICEcashService();
                var summaryDetial = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId = '" + id + "'");

                if (summaryDetial != null)
                {
                    foreach (var item in summaryDetial)
                    {
                        var vichelDetails = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                        if (vichelDetails != null)
                        {
                            string InsuranceID = vichelDetails.InsuranceId;
                            // end is null
                            customerDetails = InsuranceContext.Customers.Single(vichelDetails.CustomerId);

                            var policyDetils = InsuranceContext.PolicyDetails.Single(vichelDetails.PolicyId);

                            if (policyDetils != null)
                            {
                                var policyNum = policyDetils.PolicyNumber;
                            }
                        }


                        PartnerToken = SummaryDetailService.GetLatestToken();

                        var res = new ResultRootObject();

                        if (vichelDetails != null && vichelDetails.CombinedID != null && vichelDetails.VehicleLicenceFee > 0)
                        {
                            ResultRootObject quoteresponse = ICEcashService.TPILICUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);
                            if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
                            {
                                tokenObject = iceCash.getToken();
                                SummaryDetailService.UpdateToken(tokenObject);
                                PartnerToken = tokenObject.Response.PartnerToken;
                                quoteresponse = ICEcashService.TPILICUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);

                            }

                            if (quoteresponse.Response != null && quoteresponse.Response.Result == 1)
                            {
                                _iceCashRef = quoteresponse.Response.ICEcashReference;
                            }

                            res = ICEcashService.TPILICResult(vichelDetails, PartnerToken);
                            if (res.Response != null && res.Response.Message.Contains("Partner Token has expired"))
                            {
                                tokenObject = iceCash.getToken();
                                //   tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                                SummaryDetailService.UpdateToken(tokenObject);
                                // tokenObject = service.CheckSessionExpired();
                                PartnerToken = tokenObject.Response.PartnerToken;
                                res = ICEcashService.TPILICResult(vichelDetails, PartnerToken);
                            }

                            if (res.Response != null && res.Response.LicenceCert != null)
                                _pdfPath = MiscellaneousService.LicensePdf(res.Response.LicenceCert, vichelDetails.Id.ToString());

                            string format = "yyyyMMdd";
                            if (res.Response != null && res.Response.LicExpiryDate!=null)
                            {
                                try
                                {
                                    DateTime LicExpiryDate = DateTime.ParseExact(res.Response.LicExpiryDate, format, CultureInfo.InvariantCulture);
                                    vichelDetails.LicExpiryDate = LicExpiryDate.ToShortDateString();
                                }
                                catch(Exception ex)
                                {

                                }                       
                            }
                        }

                        else if (vichelDetails != null && vichelDetails.InsuranceId != null)
                        {

                            ResultRootObject quoteresponse = ICEcashService.TPIQuoteUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);
                            if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
                            {
                                tokenObject = iceCash.getToken();
                                SummaryDetailService.UpdateToken(tokenObject);
                                PartnerToken = tokenObject.Response.PartnerToken;
                                quoteresponse = ICEcashService.TPIQuoteUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);

                            }

                            if (quoteresponse.Response != null && quoteresponse.Response.Result == 1)
                            {
                                _iceCashRef = quoteresponse.Response.ICEcashReference;
                            }




                            //if (vichelDetails != null && vichelDetails.LicenseId != null && vichelDetails.VehicleLicenceFee > 0)
                            //{
                            //    quoteresponse = ICEcashService.LICQuoteUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);

                            //    if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
                            //    {
                            //        //  log.WriteLog(quoteresponse.Response.Quotes[0].Message + " reg no: " + vichelDetails.RegistrationNo);
                            //        tokenObject = iceCash.getToken();
                            //        // tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                            //        //  tokenObject = service.CheckSessionExpired();
                            //        SummaryDetailService.UpdateToken(tokenObject);
                            //        PartnerToken = tokenObject.Response.PartnerToken;


                            //        ICEcashService.LICQuoteUpdate(customerDetails, vichelDetails, PartnerToken, paymentMethod);
                            //    }
                            //}
                            //   System.Threading.Thread.Sleep(10000); // wait for 20 second       

                            res = ICEcashService.TPIPolicy(vichelDetails, PartnerToken);

                            if (res.Response != null && res.Response.Message.Contains("Partner Token has expired"))
                            {
                                tokenObject = iceCash.getToken();
                                //   tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                                SummaryDetailService.UpdateToken(tokenObject);
                                // tokenObject = service.CheckSessionExpired();
                                PartnerToken = tokenObject.Response.PartnerToken;
                                res = ICEcashService.TPIPolicy(vichelDetails, PartnerToken);
                            }
                        }

                        if (res.Response != null && res.Response.Message.Contains("Policy Retrieved"))
                        {
                            //if (res.Response.Status == "Approved")
                            // OTPConfirmationObject optMod = new OTPConfirmationObject();

                            result = res.Response.Status;
                            vichelDetails.InsuranceStatus = "Approved";
                            vichelDetails.CoverNote = res.Response.PolicyNo;
                           
                                
                           

                            if (res.Response.LicenceCert != null && res.Response.LicenceCert.Length > 1)
                                _pdfCode = "PD" + vichelDetails.Id + "" + DateTime.Now.Month;


                            vichelDetails.PdfCode = _pdfCode;
                            InsuranceContext.VehicleDetails.Update(vichelDetails);

                        }
                        else
                        {
                            result = res.Response.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //   log.WriteLog("to approve");

            }

            return result;
            #endregion
        }



        [HttpPost]
        public ActionResult SaveDetailList(PaymentInformationsModel model)
        {

            return View();
        }


        public ActionResult Test1()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveDeleverLicence(bool IsCheck, int Id)
        {
            try
            {
                var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(Id);
                paymentInformations.DeleverLicence = IsCheck;
                InsuranceContext.PaymentInformations.Update(paymentInformations);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremiumPaid, string PolicyNumber, string Email)
        {
            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

            List<Item> itms = new List<Item>();

            foreach (var vehicledetail in SummaryVehicleDetails.ToList())
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(vehicledetail.VehicleDetailsId);
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                Item item = new Item();
                item.name = make.MakeDescription + "/" + _model.ModelDescription;
                item.currency = "USD";
                item.price = Convert.ToString(_vehicle.Premium);
                item.quantity = "1";
                item.sku = _vehicle.RegistrationNo;

                itms.Add(item);
            }

            //Item item = new Item();
            //item.name = product.ProductName;
            //item.currency = "USD";
            //item.price = vehicle.Premium.ToString();
            //item.quantity = "1";
            //item.sku = "sku";
            //item.currency = "USD";



            Session["itemData"] = itms;

            Insurance.Service.PaynowService paynowservice = new Insurance.Service.PaynowService();
            PaynowResponse paynowresponse = new PaynowResponse();

            paynowresponse = await paynowservice.initiateTransaction(Convert.ToString(id), TotalPremiumPaid, PolicyNumber, Email);

            if (paynowresponse.status == "Ok")
            {
                string strScript = "location.href = '" + paynowresponse.browserurl + "';";
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){" + strScript + "});</script>";
            }
            else
            {
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){$('#errormsg').text('" + paynowresponse.error + "');});</script>";
            }



            return View();
            //return RedirectToAction("SaveDetailList", "Paypal", new { id = id });
        }

        public ActionResult EcoCashPayment(int id, string invoiceNumber, int Paymentid)
        {
            EcoCashModel model = new EcoCashModel();
            model.PaymentMethodId = Paymentid;
            model.SummaryId = id;
            model.InvoiceNumber = invoiceNumber;

            ApproveVRNToIceCash(model.SummaryId, model.PaymentMethodId);

            return View(model);
        }

        [HttpPost]
        public ActionResult EcoCashPayment(EcoCashModel model)
        {

            if (ModelState.IsValid)
            {
                ICEcashService iceCash = new ICEcashService();


                OTPConfirmationObject otpModel = new OTPConfirmationObject();
                otpModel.OTP = model.OTP;
                otpModel.ICEcashReference = _iceCashRef;

                if (_iceCashRef == "")
                {
                    return RedirectToAction("failed_url", "Paypal");
                }

                var tokenObject = new ICEcashTokenResponse();
                tokenObject = iceCash.getToken();
                SummaryDetailService.UpdateToken(tokenObject);
                string PartnerToken = tokenObject.Response.PartnerToken;


                var res = ICEcashService.OTPConfirmation(otpModel, PartnerToken);

                if (res.Response != null && res.Response.Result == 1)
                {
                    return RedirectToAction("SaveDetailList", "Paypal", new { id = model.SummaryId, invoiceNumer = res.Response.TransactionID, Paymentid = model.PaymentMethodId });
                }
                else
                {
                    return RedirectToAction("failed_url", "Paypal");
                }
            }

            return View(model);
        }

        public ActionResult ThankYou()
        {
           


            LicenseModel model = new LicenseModel();
            if (TempData["file"] != null)
            {
                ViewBag.file = (string)TempData["file"];
                model.FilePath = (string)TempData["file"];
                model.VehicleId = (int)TempData["vehicleId"];
            }

            return View(model);
        }






    }







}



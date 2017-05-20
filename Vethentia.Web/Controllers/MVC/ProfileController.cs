namespace Keysme.Web.Controllers.MVC
{
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Data.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Services.Data.Contracts;
    using ViewModels.Profile;
    using Image = System.Drawing.Image;
    using System.IO;
    using System.Linq;
    using ViewModels.Paypal;
    using PayPal.Api;
    using System.Xml;
    using Keysme.Web.Common;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using ViewModels.HotelCancel;
    using System.Web.Configuration;
    using System.Text;
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IUsersService usersService;
        private readonly IHotelBookingService hotelBookingService;
        private readonly IHotelContent HotelContent;
        public ProfileController(IUsersService usersService, IHotelBookingService hotelBookingService, IHotelContent HotelContent)
        {
            this.usersService = usersService;
            this.hotelBookingService = hotelBookingService;
            this.HotelContent = HotelContent;
        }

        public ApplicationSignInManager SignInManager => this.HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

        public ApplicationUserManager UserManager => this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        [HttpGet]
        public ActionResult Index()
        {
            var user = this.usersService.GetUser(this.User.Identity.GetUserId());
            var model = new ProfileViewModel();
            model.ChangeInfoViewModel = this.Mapper.Map<ChangeInfoViewModel>(user);
            model.ChangePasswordViewModel = new ChangePasswordViewModel();
            model.RequestVerificationViewModel = new RequestVerificationViewModel();
            var verification = this.usersService.GetVerificationByUserId(user.Id);
            if (verification != null)
            {
                model.IsVerified = verification.IsApproved;
                model.RequestVerificationViewModel.HasRequestedVerification = true;
                model.RequestVerificationViewModel.CountryCode = verification.CountryCode;
                model.RequestVerificationViewModel.VerificationType = verification.Type;
                model.RequestVerificationViewModel.FrontImagePath = verification.FrontPicture;
                model.RequestVerificationViewModel.BackImagePath = verification.BackPicture;
            }
            if (model.ChangeInfoViewModel.PhoneNumber != null)
            {
                int CountryCode = Convert.ToInt32(model.ChangeInfoViewModel.PhoneNumber.Split('-')[0].Remove(0, 1));
                model.ChangeInfoViewModel.PhoneNumber = model.ChangeInfoViewModel.PhoneNumber.Split('-')[1];
                PhoneNumberCountryCode enumDisplayStatus = (PhoneNumberCountryCode)CountryCode;
                model.ChangeInfoViewModel.PhoneNumberCountryCode = enumDisplayStatus;
            }


            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeInfo(ChangeInfoViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error.";
                return this.RedirectToAction("Index");
            }

            var monthDaysCount = DateTime.DaysInMonth(model.BirthYear, model.BirthMonth);
            var birthDate = new DateTime(model.BirthYear, model.BirthMonth, model.BirthDay > monthDaysCount ? monthDaysCount : model.BirthDay);
            model.BirthDate = birthDate;

            var value = Enum.Parse(typeof(PhoneNumberCountryCode), Convert.ToString(model.PhoneNumberCountryCode));
            int integerValue = (int)value;
            string PhoneNumber = "+" + integerValue + "-" + Convert.ToString(model.PhoneNumber);
            model.PhoneNumber = PhoneNumber;

            var user = this.Mapper.Map<User>(model);
            this.usersService.Update(this.User.Identity.GetUserId(), user);

            this.TempData["Success"] = "Profile information has been updated.";

            return this.RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error.";
                return this.RedirectToAction("Index");
            }
            var result = await this.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await this.UserManager.FindByIdAsync(this.User.Identity.GetUserId());
                if (user != null)
                {
                    await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                this.TempData["Success"] = "Password has been changed.";
                return this.RedirectToAction("Index");
            }

            this.TempData["Error"] = "Error.";
            return this.RedirectToAction("Index");
        }

        [HttpPost]

        public JsonResult UploadProfileImage(string abc)
        {

            var aaaa = abc.Split(',')[1].ToString();
            byte[] decodedFromBase64 = Convert.FromBase64String(aaaa);
            byte[] bytes = Convert.FromBase64String(aaaa);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            //var image = Image.FromStream(file.InputStream);            
            this.usersService.AddProfileImage(this.User.Identity.GetUserId(), image);

            this.TempData["Success"] = "Profile image has been changed.";
            var user = this.usersService.GetUser(this.User.Identity.GetUserId());
            return Json(user.ProfileImage, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestVerification(RequestVerificationViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error.";
                return this.RedirectToAction("Index");
            }

            try
            {
                var frontImage = Image.FromStream(model.Front.InputStream);
                var backImage = Image.FromStream(model.Back.InputStream);
                this.usersService.RequestVerification(this.User.Identity.GetUserId(), model.VerificationType.GetValueOrDefault(), model.CountryCode.GetValueOrDefault(), frontImage, backImage);

                this.TempData["Success"] = "Verification has been requested.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.TempData["Error"] = "Error.";
                return this.RedirectToAction("Index");
            }
        }

        [HttpPost]

        public ActionResult ImageUpload(string abc)
        {

            var aaaa = abc.Split(',')[1].ToString();
            byte[] decodedFromBase64 = Convert.FromBase64String(aaaa);
            return View();
        }

        public ActionResult BookingDetails()
        {
            var user = this.usersService.GetUser(this.User.Identity.GetUserId());
            var model = new ProfileViewModel();
            model.ChangeInfoViewModel = this.Mapper.Map<ChangeInfoViewModel>(user);
            model.ChangePasswordViewModel = new ChangePasswordViewModel();
            model.RequestVerificationViewModel = new RequestVerificationViewModel();
            var verification = this.usersService.GetVerificationByUserId(user.Id);
            if (verification != null)
            {
                model.IsVerified = verification.IsApproved;
                model.RequestVerificationViewModel.HasRequestedVerification = true;
                model.RequestVerificationViewModel.CountryCode = verification.CountryCode;
                model.RequestVerificationViewModel.VerificationType = verification.Type;
                model.RequestVerificationViewModel.FrontImagePath = verification.FrontPicture;
                model.RequestVerificationViewModel.BackImagePath = verification.BackPicture;
            }
            List<HotelBookingDetails> detailsList = new List<HotelBookingDetails>();
            detailsList = this.hotelBookingService.GetHotelBookingResultByUserId(this.User.Identity.GetUserId());
            model.HotelBookingDetailList = this.Mapper.Map<List<HotelBookingDetailsViewModel>>(detailsList);

            var transactionList = this.hotelBookingService.GetHotelBookingTransactionByUserID(this.User.Identity.GetUserId());
            var allPhotoResult = this.HotelContent.GetHotelPhotoList();
            foreach (var item in model.HotelBookingDetailList)
            {
                item.HotelBookingTransaction = transactionList.Where(x => x.HotelId == item.HotelId && x.ResultId == item.ResultId).FirstOrDefault();
                item.PaypalBookingTransaction= this.hotelBookingService.GetPaypalBookingTransactionByUserId(this.User.Identity.GetUserId(), item.HotelBookingTransaction.ResultId);
                var image = allPhotoResult.Where(x => x.HotelContentId == Convert.ToInt64(item.HotelId)).SingleOrDefault();
                if (image != null)
                {

                    item.image = "http://www.roomsxml.com" + image.Url;

                }
                item.cancelList = this.hotelBookingService.GetCancellationDetailByBookingID(item.HotelBookingTransaction.BookingID);
            }

            return View(model);
        }
        /// <summary>
        /// THis methos will be used after getting status sucess from roomxml
        /// </summary>
        /// <returns></returns>
        public ActionResult SaleRefund(string bookingID)
        {

            var hotelTransaction = this.hotelBookingService.GetHotelBookingTransactionByBookingID(bookingID, this.User.Identity.GetUserId());
           // var BookingContent = this.hotelBookingService.GetHotelBookingContent(this.User.Identity.GetUserId(), hotelTransaction.re);
            //var hotelBookingResult = this.hotelBookingService.GetHotelBookingResultByUserId(this.User.Identity.GetUserId());
            var paypalTransaction = this.hotelBookingService.GetPaypalBookingTransactionByUserId(this.User.Identity.GetUserId(), hotelTransaction.ResultId);
            BookingCancelResponse cancelObj = CancelHotelBooking(Convert.ToString(hotelTransaction.BookingID));
            var cancellationDetails = this.hotelBookingService.GetCancellationDetailByBookingID(hotelTransaction.BookingID);
            var sortedReadings = cancellationDetails.OrderBy(x => x.CancelFromDate.TimeOfDay)
                .ThenBy(x => x.CancelFromDate.Date)
                 .ThenBy(x => x.CancelFromDate.Year);
            double refundAmount = 0;
            string cancelAmount = "";
            foreach(var item in sortedReadings)
            {
                if (item.CancelFromDate.Date> DateTime.UtcNow.Date )
                {
                    cancelAmount = item.CancelAmount;
                    break;
                }
              
            }
            refundAmount = Convert.ToDouble(hotelTransaction.Amount) - Convert.ToDouble(cancelAmount);
            string Amount= String.Format("{0:0.00}", refundAmount);
            if (cancelObj.BookingCancelResult.CommitLevel.ToLower() == "confirm")
            {
                var apiContext = Configuration.GetAPIContext();
                var refund = new Refund()
                {
                    amount = new PayPal.Api.Amount()
                    {
                        currency = "USD",
                        total = Amount
                    }
                };

                var sale = new Sale()
                {
                    id = paypalTransaction.SaleId
                };

                var response = sale.Refund(apiContext, refund);

                if(response.state== "completed")
                {
                    PaypalBookingTransaction PaypalBookingTransaction = new PaypalBookingTransaction();
                    PaypalBookingTransaction.BookingDate = paypalTransaction.BookingDate;
                    PaypalBookingTransaction.UserId = this.User.Identity.GetUserId();
                    PaypalBookingTransaction.TransactionId = paypalTransaction.TransactionId;
                    PaypalBookingTransaction.ResultId = paypalTransaction.ResultId;
                    PaypalBookingTransaction.SaleId = response.sale_id;
                    PaypalBookingTransaction.BookingAmount = paypalTransaction.BookingAmount;
                    PaypalBookingTransaction.RefundAmount = response.amount.total;
                    PaypalBookingTransaction.CancelId = response.id;
                    PaypalBookingTransaction.Status = response.state;
                    PaypalBookingTransaction.CancelDate = Convert.ToDateTime(response.create_time);
                    PaypalBookingTransaction.Type = "C";
                    this.hotelBookingService.AddPaypalBookingTransaction(PaypalBookingTransaction);

                    hotelTransaction.CancelBy = this.User.Identity.GetUserId(); 
                    this.hotelBookingService.UpdateHotelBookingTransaction(hotelTransaction);
                }
            }

            return RedirectToAction("BookingDetails");
        }

        /// <summary>
        /// THis Method is used for Cancellation for RoomXML
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        private BookingCancelResponse CancelHotelBooking(string bookingId)
        {

            StringBuilder xmlString = new StringBuilder();
            xmlString.Append("<?xml version='1.0' encoding='UTF-8'?>");
            xmlString.Append("<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>");
            xmlString.Append("<soap:Body>");
            xmlString.Append("<BookingCancel xmlns='http://www.reservwire.com/namespace/WebServices/Xml'>");
            xmlString.Append("<xiRequest>");
            xmlString.Append("<Authority>");
            xmlString.Append("<Org>" + WebConfigurationManager.AppSettings["Org"] + "</Org>");
            xmlString.Append("<User>" + WebConfigurationManager.AppSettings["User"] + "</User>");
            xmlString.Append("<Password>" + WebConfigurationManager.AppSettings["Password"] + "</Password>");
            xmlString.Append("<Currency>USD</Currency>");
            xmlString.Append("<TestMode>" + WebConfigurationManager.AppSettings["TestMode"] + "</TestMode>");
            xmlString.Append("<TestDebug>false</TestDebug>");
            xmlString.Append("<Version>1.25</Version>");
            xmlString.Append("</Authority>");
            xmlString.Append("<BookingId>" + bookingId + "</BookingId>");
            xmlString.Append("<CommitLevel>confirm</CommitLevel>");
            xmlString.Append("</xiRequest>");
            xmlString.Append("</BookingCancel>");
            xmlString.Append("</soap:Body>");
            xmlString.Append("</soap:Envelope>");

            XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(Convert.ToString(xmlString));

            string type = "BookingCancel";
            string response = RoomXml.SendSOAPRequest(soapEnvelopeXml.InnerXml, type);

            XmlDocument lXMLDoc = new XmlDocument();
            lXMLDoc.LoadXml(response);

            var Value = XDocument.Parse(lXMLDoc.OuterXml);
            XNamespace ns = @"http://schemas.xmlsoap.org/soap/envelope/";
            var unwrappedResponse = Value.Descendants((XNamespace)"http://schemas.xmlsoap.org/soap/envelope/" + "Body").First().FirstNode;
            XmlSerializer oXmlSerializer = new XmlSerializer(typeof(BookingCancelResponse));
            var responseObj = (BookingCancelResponse)oXmlSerializer.Deserialize(unwrappedResponse.CreateReader());


            return responseObj;
        }

    }
}
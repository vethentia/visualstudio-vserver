
namespace Vethentia.Controllers.WebApi
{
    using Global.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Vethentia.Data.Models;
    using Vethentia.Services.Interfaces;
    using Vethentia.ViewModels.WebApi.Payment;

    [RoutePrefix("api/payments")]
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService paymentService;
        private readonly IUserService userService;
        private readonly IMerchantService merchantService;
        private readonly INotificationService notificationService;

        public PaymentController(IPaymentService paySvc, IUserService usrSvc, IMerchantService merSvc, INotificationService nSvc)
        {
            paymentService = paySvc;
            userService = usrSvc;
            merchantService = merSvc;
            notificationService = nSvc;
        }

        /// <summary>
        /// Content-Type: application/json
        /// Sample body 
        /*
{
"msgId": 1,
"tid": 123456,
"ttime": "2016-12-08 18:30:01",
"vid": "htbanh@yahoo",
"shippingInfo": "123 King street, Los Angeles, CA 91733",
"amount": 99.78,
"countryCode": "US",
"currencyCode": "USD",
"merchantIdentifier": "d290d3d15e6c4a1491dd9024e29d1a9c",
"merchantName": "ABC company",
"lineItems": "1 item; 2 item; 3 item",
"publicKey": "--whatispublickey--",
"messageauthenticationcode": "--whereismessageauthenticationcodecomefrom?--",
"mac": "123",
"supportedNetwork": 12,
"merchantCapabilities": "123",
"paymentMethodTokenizationType": "sometokenhere"
}        
*/
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("PayRequest")]
        [HttpPost]
        public IHttpActionResult PayRequest(PayRequestBindingModel model)
        {
            PayResponseBindingModel pr = new PayResponseBindingModel()
            {
                msgId = 2,
                tid = model.tid,
                token = "0",
                status = 1
            };


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = userService.GetUserByEmail(model.vid);
            if (user == null)
            {
                // Invalid user email
                pr.status = 1;
                return Ok(pr);
            }

            Merchant merchant = merchantService.GetMerchantByAlias(model.merchantIdentifier);
            if (merchant == null)
            {
                // Invalid merchant
                pr.status = 2;
                return Ok(pr);
            }

            var pay = new PaymentLog()
            {
                TId = model.tid.ToString(),
                TTime = DateTime.Parse(model.ttime),
                ShippingInfo = model.shippingInfo,
                Amount = decimal.Parse(model.amount),
                CountryCode = model.countryCode,
                CurrencyCode = model.currencyCode,
                MerchantId = model.merchantIdentifier,
                MechantName = model.merchantName,
                LineItems = model.lineItems,
                PublicKey = model.publicKey,
                Email = model.vid,
                MerchantCapability = merchant.MerchantCapabilities,
                SupportedNetwork = merchant.SupportNetwork,
                PaymentMethodTokenizationType = merchant.PaymnetMethodTokenizationType
            };

            string val = paymentService.RequestPay(pay);
            if (string.IsNullOrEmpty(val))
            {
                ModelState.AddModelError("", string.Format("Error PayRequest"));
                return BadRequest(ModelState);
            }

            TokenRequestModel trModel = PaymentLogToTokenRequestModel(pay);
            notificationService.TokenRequest(Global.NotificationHubPNS.APNS, trModel);

            //Uri locationHeader = new Uri(Url.Link("GetPaymentById", new { id = pay.Id }));
            //return Created(locationHeader, ThePaymentModelFactory.Create(pay));

            return Ok(pr);
        }

        [Route("getall")]
        public IHttpActionResult GetPayments()
        {
            var listLog = this.paymentService.Payments();
            return Ok(listLog);
        }

        [AllowAnonymous]
        [Route("payment/{id:guid}", Name = "GetPaymentById")]
        public IHttpActionResult GetPayment(string id)
        {
            var pment = this.paymentService.GetPayment(id);

            if (pment != null)
            {
                return Ok(pment);
            }

            return NotFound();

        }

        [AllowAnonymous]
        [Route("tokenresponse/{msgId}/{id:guid}/{token}/{status}")]
        public IHttpActionResult TokenResponse(int msgId, string tid, string token, int status)
        {
            PaymentLog log = new PaymentLog();
            log.Token = token;
            log.Status = status;
            paymentService.UpdatePaymentByTid(tid, log);

            return Ok();
        }


        private TokenRequestModel PaymentLogToTokenRequestModel(PaymentLog pay)
        {
            TokenRequestModel trModel = new TokenRequestModel();
            return trModel;
        }

        /*
        /// <summary>
        /// Content-Type: application/json
        /// Sample body 
        /// {
        ///  "TId": "TIdtest2",
        ///   "TTime": "2016-11-26 13:44:00",
        ///   "Email": "htbanh@yahoo.com",
        ///   "Mac": "Mac1",
        ///   "ShippingInfo": "Address, Elkridge, MD 12345",
        ///   "Amount": 2.99,
        ///   "CountryCode": "US",
        ///   "CurrencyCode": "USD",
        ///   "SupportedNetwork": 1,
        ///   "MerchantId": "d290d3d15e6c4a1491dd9024e29d1a9c",
        ///   "MechantName": "Banh",
        ///   "MerchantCapability": "capability12",
        ///   "LineItems": "lineitem1",
        ///   "PaymentMethodTokenizationType": "paymenttoktype12",
        ///   "PublicKey": "publickey12"
        /// }
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("PayLoad")]
        [HttpPost]
        public IHttpActionResult PayLoad(PayLoadBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pay = new PaymentLog()
            {
                TId = model.tid.ToString(),
                //TTime = model.ttime,
                Email = model.emailAddress,
                //Mac = model.Mac,
                ShippingInfo = model.shippingInfo,
                //Amount = model.amount,
                CountryCode = model.countryCode,
                CurrencyCode = model.currencyCode,
                //SupportedNetwork = model.,
                MerchantId = model.merchantIdentifier,
                MechantName = model.mechantName,
                MerchantCapability = model.merchantCapabilities.ToString(),
                LineItems = model.lineItems,
                PaymentMethodTokenizationType = model.paymentMethodTokenizationType,
                PublicKey = model.publicKey,
            };

            string val = paymentService.RequestPay(pay);
            if (string.IsNullOrEmpty(val))
            {
                ModelState.AddModelError("", string.Format("Error PayRequest"));
                return BadRequest(ModelState);
            }

            Uri locationHeader = new Uri(Url.Link("GetPaymentById", new { id = pay.Id }));

            return Created(locationHeader, ThePaymentModelFactory.Create(pay));
        }
        */



    }
}

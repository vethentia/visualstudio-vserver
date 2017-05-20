
namespace Vethentia.Controllers.WebApi
{
    using Global.Models;
    using Microsoft.Azure.NotificationHubs.Messaging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using Vethentia.Data.Models;
    using Vethentia.Services.Interfaces;
    using Vethentia.ViewModels.WebApi.Payment;
    using Web.Controllers;

    //    [EnableCors(origins: "http://localhost:8383", headers: "*", methods: "*", SupportsCredentials = true)]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    //[EnableCors(origins: "http://sample-env-1.xmnpfvmvsh.us-west-1.elasticbeanstalk.com", headers: "*", methods: "*", SupportsCredentials = true)]
    [RoutePrefix("api/payments")]
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService paymentService;
        private readonly IUserService userService;
        private readonly IMerchantService merchantService;
        private readonly INotificationService notificationService;

        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public PaymentController(IPaymentService paySvc, IUserService usrSvc, IMerchantService merSvc, INotificationService nSvc)
        {
            paymentService = paySvc;
            userService = usrSvc;
            merchantService = merSvc;
            notificationService = nSvc;

        }

        /// <summary>
        /// User by Browser to send the Payrequest to the vServer.
        /// 
        /// Content-Type: application/json
        /// Sample body 
        /*
http://vethentia.azurewebsites.net/api/payments/payrequest
http://localhost:58536/api/payments/payrequest
{
"msgId": 1,
"tid": 201703011,
"ttime": "2017-3-10 18:30:01",
"vid": "htbanh@hotmail.com",
"shippingInfo": "123 King street, Los Angeles, CA 20175",
"amount": "99.78",
"countryCode": "US",
"currencyCode": "USD",
"merchantIdentifier": "merchant.com.vray.vpay",
"merchantName": "ABC company",
"lineItems": "1 item; 2 item; 3 item",
"publicKey": "--whatispublickey--",
"messageauthenticationcode": "--whereismessageauthenticationcodecomefrom?--"
}  
*/
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("payrequest")]
        [HttpPost]
        public IHttpActionResult PayRequest(PayRequestBindingModel model)
        {
            string slog = string.Format("PayRequest Begin model: {0}", JsonConvert.SerializeObject(model));
            logger.Debug(slog);

            PayResponseBindingModel pr = new PayResponseBindingModel()
            {
                msgId = 2,
                tid = model.tid,
                token = "",
                status = 2
            };

            if (!ModelState.IsValid)
            {
                logger.Debug("PayRequest ModelState is invalid");
                pr.status = -1;
                return Ok(pr);
            }


            ///////////////////////////////////////
            // Check for valid vid or email
            ///////////////////////////////////////
            User user = userService.GetUserByEmail(model.vid);
            if (user == null)
            {
                // Invalid user email
                pr.status = 1;
                return Ok(pr);
            }
            else if (user != null && user.EmailConfirmed == false)
            {
                // User hasn't confirmed email.
                pr.status = 1;
                return Ok(pr);
            }

            ///////////////////////////////////////
            // Check for valid merchant
            ///////////////////////////////////////
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
                TTime = model.ttime,
                ShippingInfo = model.shippingInfo,
                Amount = model.amount,
                CountryCode = model.countryCode,
                CurrencyCode = model.currencyCode,
                MerchantId = model.merchantIdentifier,
                MechantName = model.merchantName,
                LineItems = model.lineItems,
                PublicKey = model.publicKey,
                Email = model.vid,
                MerchantCapability = merchant.MerchantCapabilities,
                SupportedNetwork = merchant.SupportNetwork,
                PaymentMethodTokenizationType = merchant.PaymnetMethodTokenizationType,
                CodeCheck = string.Empty,
                CodeCheckCount = 0,
                IsCodeCheckValidated = false                
            };

            string codeCheck = GetRamdomCodeCheck();
            bool correctShippingInfo = paymentService.IsValidUserShippingInfo(user, model.shippingInfo);
            if (!correctShippingInfo)
            {
                pay.CodeCheck = codeCheck;
            }

            ///////////////////////////////////////
            // Save payment to database PaymentLog table
            ///////////////////////////////////////
            PaymentLog checkLog = paymentService.GetPaymentByTid(pay.TId);
            if (checkLog == null)
            { 
                // Save pay to the database
                string val = paymentService.RequestPay(pay);
                if (string.IsNullOrEmpty(val))
                {
                    //ModelState.AddModelError("", string.Format("Error PayRequest"));
                    //return BadRequest(ModelState);
                    logger.Debug("PayRequest can't add to the database PaymentLog table");
                    pr.status = -1;
                    return Ok(pr);
                }
            }
            //else
            //{
            //    //ModelState.AddModelError("", string.Format("Error tid is already exist"));

            //    return BadRequest(ModelState);
            //}

            ///////////////////////////////////////
            // Check to make sure Shipping Info is to the vid or email user
            ///////////////////////////////////////
            if (!correctShippingInfo)
            {
                ///////////////////////////////////////
                // Insert key to PayTokenManager PaymentRequest
                ///////////////////////////////////////
                PayTokenManager.Instance.SavePaymentLog(model.tid, pay);

                // Send CodeCheck back to the browser
                CodeCommandBindingModel codeCommand = new CodeCommandBindingModel()
                {
                    msgId = 5,
                    tid = model.tid,
                    code = codeCheck
                };
                return Ok(codeCommand);
            }

            try
            {
                ///////////////////////////////////////
                // Get token value from PayTokenManger
                ///////////////////////////////////////

                TokenRequestModel trModel = PaymentLogToTokenRequestModel(pay, user, merchant);
                string token = SendMobileTokenRequest(model.tid, user.Id, trModel);
                if (!string.IsNullOrEmpty(token))
                {
                    pr.status = 0;
                }
                else
                {
                    pr.status = 4;
                }
                pr.token = token;


                /*
                ///////////////////////////////////////
                // Notify the mobile applet
                ///////////////////////////////////////
                TokenRequestModel trModel = PaymentLogToTokenRequestModel(model, user, merchant);
                notificationService.TokenRequest(Global.NotificationHubPNS.APNS, user.Id, trModel);

                ///////////////////////////////////////
                // Insert key to PayTokenManager
                ///////////////////////////////////////
                PayTokenManager.Instance.Save(model.tid, string.Empty);

                ///////////////////////////////////////
                // Waiting for TokenResponse from applet to update the PayTokenManager
                ///////////////////////////////////////

                int timerPayRequest = 10;
                int.TryParse(ConfigurationManager.AppSettings["TimerPayRequest"].ToString(), out timerPayRequest);

                timerPayRequest *= 1000;

                int tCount = 30;
                int interval = timerPayRequest / tCount;
                int timer = 0;
                while (timer < timerPayRequest)
                {
                    Thread.Sleep(interval);
                    timer += interval;
                    string tok;
                    PayTokenManager.Instance.Get(model.tid, out tok);
                    if (!string.IsNullOrEmpty(tok))
                    {
                        break;
                    }
                }

                ///////////////////////////////////////
                // Get token value from PayTokenManger
                ///////////////////////////////////////
                string token = "";
                if (PayTokenManager.Instance.Get(model.tid, out token) && !string.IsNullOrEmpty(token))
                {
                    pr.status = 0;
                    PayTokenManager.Instance.Delete(model.tid);
                }
                else
                {
                    pr.status = 4;
                }
                pr.token = token;
                */

                if (pr.status == 0)
                {
                    paymentService.SavePaymentRequestConfimred(model.tid.ToString());
                }
                else
                {
                    paymentService.SavePaymentRequestRejected(model.tid.ToString());
                }
                PayTokenManager.Instance.Delete(model.tid);
            }
            catch (MessagingException e)
            {
                logger.Error("PayRequest MessageException", e);
                //ReturnGoneIfHubResponseIsGone(e);
                pr.status = 4;
                return Ok(pr);
            }
            catch (Exception ex)
            {
                logger.Error("PayRequest", ex);
                pr.status = 4;
                return Ok(pr);
            }

            slog = string.Format("PayRequest Returns: {0}", JsonConvert.SerializeObject(pr));
            logger.Debug(slog);

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

        /// <summary>
        /// Use by Mobile to send a token to vServer
        /// 
        /// http://vethentia.azurewebsites.net/api/payments/tokenresponse/8/12345/token1/0
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="tid"></param>
        /// <param name="token"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("tokenresponse/{msgId}/{tid}/{token}/{status}")]
        [HttpPut]
        public IHttpActionResult TokenResponse(int msgId, long tid, string token, int status)
        {
            PaymentLog log = new PaymentLog();
            log.Token = token;
            log.Status = status;
            string slog = string.Format("TokenResponse begin from mobile msgId {0} tid {1} token {2} status {3}", msgId, tid, token, status);
            logger.Debug(slog);

            if (!paymentService.UpdatePaymentByTid(tid.ToString(), log))
            {
                return BadRequest(string.Format("Invalid tid {0}", tid));
            }

            PayTokenManager.Instance.Save(tid, token);


            return Ok();
        }

        /// <summary>
        /// 
        /*
http://vethentia.azurewebsites.net
http://localhost:58536/api/payments/tokenresquest
{
"msgId": 7,
"deviceID": "appledevicetoken123",
"tid": 12345,
"gatewayId": 1,
"amount": "123.02",
"countryCode": "US",
"currencyCode": "USD",
"vid": "htbanh@yahoo.com",
"ttime": "2016-12-11 18:04:01",
"shippingInfo": "1 Way blvd. San Diego, CA 12345",
"supportedNetwork": 7,
"merchantIdentifier": "merchant.com.vray.vpay",
"merchantCapabilities": 1,
"merchantName": "Merchant.com",
"lineItems": "line 1",
"paymentMethodTokenizationType": "paymentokenhere",
"publicKey": "keyabc123"
}
        */
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("tokenresquest")]
        [HttpPost]
        public IHttpActionResult TokenResquest(TokenRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PayResponseBindingModel pr = new PayResponseBindingModel()
            {
                msgId = 7,
                tid = model.tid,
                token = "0",
                status = 1
            };

            string userId = string.Empty;
            ///////////////////////////////////////
            // Check for valid vid or email
            ///////////////////////////////////////
            User user = userService.GetUserByEmail(model.vid);
            if (user == null)
            {
                userId = user.Id;
            }

            try
            { 
                notificationService.TokenRequest(Global.NotificationHubPNS.APNS, userId, model);
            }
            catch (MessagingException e)
            {
                ReturnGoneIfHubResponseIsGone(e);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            pr.status = 0;
            return Ok(pr);
        }

        /// <summary>
        /*
        http://vethentia.azurewebsites.net/api/payments/codersp
        {
        "msgId": 19,
        "tid": 21171151,
        "status": "0"
        }
        */
        /*
                /// </summary>
                /// <param name="msgId"></param>
                /// <param name="tid"></param>
                /// <param name="status"></param>
                /// <returns></returns>
                [AllowAnonymous]
                [Route("codersp")]
                [HttpPost]
                public IHttpActionResult CodeRsp(CodeRxBindingModel inModel)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    int msgId = inModel.msgId;
                    long tid = inModel.tid;


                    // Default assume status is RecieveCodefailure
                    CodeRxBindingModel model = new CodeRxBindingModel()
                    {
                        msgId = 19,
                        tid = tid,
                        status = 7
                    };

                    PaymentLog log = paymentService.GetPaymentByTid(tid.ToString());
                    if (log != null && !log.IsCodeCheckValidated)
                    {
                        string slog = string.Format("CodeRsp model: {0}", JsonConvert.SerializeObject(log));
                        logger.Debug(slog);


                        // Save shippinginfo for the user to database
                        User usr = userService.GetUserByEmail(log.Email);
                        Merchant merchant = merchantService.GetMerchantByAlias(log.MerchantId);
                        if (usr != null && merchant != null)
                        {
                            //if (usr != null && !paymentService.IsValidUserShippingInfo(usr, log.ShippingInfo))
                            //    paymentService.SaveUserShippingInfo(usr.Id, log.ShippingInfo);

                            ///////////////////////////////////////
                            // Get token value from PayTokenManger
                            ///////////////////////////////////////

                            TokenRequestModel trModel = PaymentLogToTokenRequestModel(log, usr, merchant);
                            string token = SendMobileTokenRequest(tid, usr.Id, trModel);
                            if (!string.IsNullOrEmpty(token))
                            {
                                model.status = 0;
                            }
                        }
                    }
                    return Ok(model);

                }
        */

        /*
        http://vethentia.azurewebsites.net/api/payments/codersp
        {
        "msgId": 19,
        "tid": 21171151,
        "status": "0"
        }
        */

        [AllowAnonymous]
        [Route("codersp")]
        [HttpPost]
        public IHttpActionResult CodeRsp(CodeRxBindingModel inModel)
        {
            string slog = string.Format("CodeRsp begin input: {0}", JsonConvert.SerializeObject(inModel));
            logger.Debug(slog);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int msgId = inModel.msgId;
            long tid = inModel.tid;

            ///////////////////////////////////////
            // Waiting for PaymentLog.IsCodeCheckValidated
            ///////////////////////////////////////
            int timerPayRequest = 30;
            int.TryParse(ConfigurationManager.AppSettings["TimerPayRequest"].ToString(), out timerPayRequest);

            timerPayRequest *= 1000;

            CodeRxBindingModel model = new CodeRxBindingModel()
            {
                msgId = 19,
                tid = tid,
                status = 7
            };

            int tCount = 60;
            int interval = timerPayRequest / tCount;
            int timer = 0;

            PaymentLog log;

            try
            {
                while (timer < timerPayRequest)
                {
                    Thread.Sleep(interval);
                    timer += interval;

                    ///////////////////////////////////////
                    // Get PayTokenManager PaymentRequest based on tid
                    ///////////////////////////////////////
                    bool isTrue = PayTokenManager.Instance.GetPaymentLog(model.tid, out log);

                    if (isTrue && log != null)
                    {
                        //slog = string.Format("CodeRsp log IsCodeCheckValidate: {0}", log.IsCodeCheckValidated);
                        //logger.Debug(slog);

                        if (log.IsCodeCheckValidated == true)
                        { 
                            model.status = 0;

                            // Save shippinginfo for the user to database
                            User usr = userService.GetUserByEmail(log.Email);

                            // if (usr != null && !paymentService.IsValidUserShippingInfo(usr, log.ShippingInfo))
                            //     paymentService.SaveUserShippingInfo(usr.Id, log.ShippingInfo);

                            ///////////////////////////////////////
                            // Get token value from PayTokenManger
                            ///////////////////////////////////////
                            //Merchant merchant = merchantService.GetMerchantByAlias(log.MerchantId);
                            //TokenRequestModel trModel = PaymentLogToTokenRequestModel(log, usr, merchant);
                            //string token = SendMobileTokenRequest(model.tid, usr.Id, trModel);
                            //if (!string.IsNullOrEmpty(token))
                            //{
                            //    PayTokenManager.Instance.Delete(model.tid);
                            //}

                            slog = string.Format("CodeRsp while loop IsCodeCheckValidated = true: timer ({0}) < timerPayRequest ({1}), interval: {2}", timer, timerPayRequest, interval);
                            logger.Debug(slog);

                            slog = string.Format("CodeRsp Log data: {0}", JsonConvert.SerializeObject(log));
                            logger.Debug(slog);

                            break;
                        }
                    }
                }

                if (model.status == 0)
                {
                    paymentService.SavePaymentRequestConfimred(model.tid.ToString());
                }
                else
                {
                    paymentService.SavePaymentRequestRejected(model.tid.ToString());
                }
                PayTokenManager.Instance.DeletePaymentLog(tid);
            }
            catch (Exception ex)
            {
                logger.Error("CodeRsp Error", ex);
            }
            slog = string.Format("CodeRsp end: {0}", JsonConvert.SerializeObject(model));
            logger.Debug(slog);

            return Ok(model);
        }
        


        /// <summary>
        /// 
        /// Send by mobile the validation code to vServer
        /*
        http://vethentia.azurewebsites.net/api/payments/codeindication
        {
        "msgId": 6,
        "vid": "htbanh@hotmail.co",
        "deviceToken": "f6fe285d995dc49a440a62776958a23238b9a29f531b32c35dc69144a736d560",
        "rxCode": "1234",
        "phoneNumber": "+17173683207"
        }
        */
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="vid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="rxCode"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("codeindication")]
        [HttpPost]
        public IHttpActionResult CodeIndication(CodeIndicationBindingModel inModel)
        {
            string slog = string.Format("codeindication begin input: {0}", JsonConvert.SerializeObject(inModel));
            logger.Debug(slog);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PaymentLog log = paymentService.CheckPaymentByDeviceTokenCode(inModel.deviceToken, inModel.rxCode);
            if (log != null)
            {
                slog = string.Format("codeindication retrieve PaymentLog model: {0}", JsonConvert.SerializeObject(log));
                logger.Debug(slog);

                //PaymentLog newlog = new PaymentLog()
                //{
                //    Token = log.Token,
                //    Status = log.Status,
                //    CodeCheck = string.Empty,
                //    IsCodeCheckValidated = log.IsCodeCheckValidated,
                //    UpdatedAt = DateTime.Now
                //};
                //newlog.CodeCheckCount = ++log.CodeCheckCount;

                log.UpdatedAt = DateTime.Now;
                log.CodeCheckCount = log.CodeCheckCount++;

                log.IsCodeCheckValidated = true;
                paymentService.UpdatePaymentByTid(log.TId, log);
                long tid = long.Parse(log.TId);
                PayTokenManager.Instance.SavePaymentLog(tid, log);

                slog = string.Format("codeindication PaymentLog after updated model: {0}", JsonConvert.SerializeObject(log));
                logger.Debug(slog);

                return Ok();
            }
            else
            {
                return BadRequest("Invalid deviceToken");    
            }
        }


        /// <summary>
        /// Test sending notification to mobile APNS but not waiting for the mobile to call back to VServer
        /// Http Post and Content-Type: application/json and the sample json
        /// Note: make sure vid email is the devicetoken register.
/*
http://vethentia.azurewebsites.net/api/payments/testnotification
{
"msgId":7,
"deviceID":"make sure vid has the corrected email with devicetoken in databse --e72c4246f9b08389b9f3da033f6efb6641238d96e921b5000a--",
"tid":1492961141600,
"gatewayId":0,
"amount":100.99,
"countryCode":"US",
"currencyCode":"usd",
"ttime":"Sun Apr 23 2017 08:25:41 GMT-0700 (PDT)",
"vid":"diego.dude@live.com", 
"shippingInfo":"123 abc street, 92121",
"supportedNetwork":7,
"merchantIdentifier":"merchant.com.vray.vpay",
"merchantCapabilities":1,
"merchantName":"merchant.com.vray.vpay",
"lineItems":"1 item;2 item;3 item",
"paymentMethodTokenizationType":"NETWORK_TOKEN",
"publicKey":null
}
*/
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("testnotification")]
        public IHttpActionResult TestNotification(TokenRequestModel model)
        {
            string slog = string.Format("TestNotification tokenReqModel: {0}", JsonConvert.SerializeObject(model));
            logger.Debug(slog);

            HttpStatusCode ret = HttpStatusCode.InternalServerError;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = userService.GetUserByEmail(model.vid);
            if (user != null)
            {
                try
                {
                    ///////////////////////////////////////
                    // Notify the mobile applet
                    ///////////////////////////////////////
                    notificationService.TokenRequest(Global.NotificationHubPNS.APNS, user.Id, model);

                }
                catch (MessagingException e)
                {
                    ReturnGoneIfHubResponseIsGone(e);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("Bad UserId");
            }
            return Ok(ret);
        }




        private string GetRamdomCodeCheck()
        {
            string val = Guid.NewGuid().ToString("n").Substring(0, 4).ToUpper();
            return val;
        }

        /// <summary>
        /// Send the token to the mobile and wait for respond
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="userId"></param>
        /// <param name="tokenReqModel"></param>
        /// <returns></returns>
        private string SendMobileTokenRequest(long tid, string userId, TokenRequestModel tokenReqModel)
        {
            string token = string.Empty;

            string slog = string.Format("SendMobileTokenRequest begin tokenReqModel: {0}", JsonConvert.SerializeObject(tokenReqModel));
            logger.Debug(slog);

            ///////////////////////////////////////
            // Notify the mobile applet
            ///////////////////////////////////////
            notificationService.TokenRequest(Global.NotificationHubPNS.APNS, userId, tokenReqModel);

            ///////////////////////////////////////
            // Insert key to PayTokenManager
            ///////////////////////////////////////
            PayTokenManager.Instance.Save(tid, string.Empty);

            ///////////////////////////////////////
            // Waiting for TokenResponse from applet to update the PayTokenManager
            ///////////////////////////////////////

            int timerPayRequest = 60;
            //int.TryParse(ConfigurationManager.AppSettings["TimerPayRequest"].ToString(), out timerPayRequest);

            timerPayRequest *= 1000;

            int tCount = 60;
            int interval = timerPayRequest / tCount;
            int timer = 0;

            ///////////////////////////////////////
            // Get token value from PayTokenManger
            ///////////////////////////////////////
            while (timer < timerPayRequest)
            {
                Thread.Sleep(interval);
                timer += interval;
                PayTokenManager.Instance.Get(tid, out token);
                if (!string.IsNullOrEmpty(token))
                {
                    slog = string.Format("SendMobileTokenRequest timer ({0}) < timerPayRequest ({1}), sleep interval msec: {2}", timer, timerPayRequest, interval);
                    logger.Debug(slog);
                    slog = string.Format("SendMobileTokenRequest got Token: {0}", token);
                    logger.Debug(slog);
                    PayTokenManager.Instance.Delete(tid);
                    break;
                }
            }

            return token;
        }


        private TokenRequestModel PaymentLogToTokenRequestModel(PaymentLog model, User user, Merchant merchant)
        {
            TokenRequestModel trModel = new TokenRequestModel()
            {
                msgId = 7,
                deviceID = user.PhoneDeviceToken,
                tid =  long.Parse(model.TId),
                gatewayId = 0,
                amount = model.Amount,
                countryCode = model.CountryCode,
                currencyCode = model.CurrencyCode,
                ttime = model.TTime,
                vid = model.Email,
                shippingInfo = model.ShippingInfo,
                supportedNetwork = merchant.SupportNetwork,
                merchantIdentifier = model.MerchantId,
                merchantCapabilities = merchant.MerchantCapabilities,
                merchantName = merchant.MerchantName,
                lineItems = model.LineItems,
                paymentMethodTokenizationType = merchant.PaymnetMethodTokenizationType,
                publicKey = model.PublicKey
            };
            return trModel;
        }

        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
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

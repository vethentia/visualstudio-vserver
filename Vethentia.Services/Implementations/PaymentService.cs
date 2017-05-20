
namespace Vethentia.Services.Implementations
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Models;
    using Data;
    using AutoMapper;
    class PaymentService : IPaymentService
    {
        private readonly IRepository<User> user;
        private readonly IRepository<Merchant> merchant;
        private readonly IRepository<PaymentLog> payment;
        private readonly IRepository<UserShippingInfo> userShippingInfo;
        private readonly IRepository<PaymentLogConfirmed> paymentConfirmed;
        private readonly IRepository<PaymentLogRejected> paymentRejected;


        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public PaymentService(IRepository<User> userRep, IRepository<Merchant> merchantRep, IRepository<PaymentLog> payRep, IRepository<UserShippingInfo> usrShipping, IRepository<PaymentLogConfirmed> payConfirmed, IRepository<PaymentLogRejected> payRejected)
        {
            user = userRep;
            merchant = merchantRep;
            payment = payRep;
            userShippingInfo = usrShipping;
            paymentConfirmed = payConfirmed;
            paymentRejected = payRejected;
        }


        public string RequestPay(PaymentLog paymentLog)
        {
            string ret = string.Empty;
            //ret = paymentLog.Key = Guid.NewGuid().ToString("N");
            payment.Add(paymentLog);
            payment.SaveChanges();
            ret = paymentLog.Id; 
            return ret;
        }

        public IEnumerable<PaymentLog> Payments()
        {
            var list = payment.All().OrderByDescending(t => t.RegisteredAt).Take(20);
            return list;
        }

        public PaymentLog GetPayment(string id)
        {
            PaymentLog p = payment.GetById(id);
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="log"></param>
        /// <returns>
        /// true is successful</returns>
        public bool UpdatePaymentByTid(string tid, PaymentLog log)
        {
            bool ret = false;
            PaymentLog p = payment.All().Where(x => x.TId == tid).FirstOrDefault();
            if (p != null)
            {
                if (!string.IsNullOrEmpty(log.Token))
                    p.Token = log.Token;

                if (log.Status > -1)
                    p.Status = log.Status;

                if (!string.IsNullOrEmpty(log.CodeCheck))
                    p.CodeCheck = log.CodeCheck;

                if (log.CodeCheckCount > 0)
                    p.CodeCheckCount = log.CodeCheckCount;

                if (log.IsCodeCheckValidated == true)
                    p.IsCodeCheckValidated = true;

                this.payment.SaveChanges();
                ret = true;
            }
            return ret;
        }

        public bool IsValidUserShippingInfo(User user, string shippingInfo)
        {
            bool ret = false;
            string streetNumber = string.Empty;
            string zipCode = string.Empty;
            string shippingAddress = shippingInfo.Trim();

            // Check street number
            int index = shippingAddress.IndexOf(user.StreetAddress);
            if (index >= 0)
            {
                // Check zipcode
                index = shippingAddress.IndexOf(user.PostalCode);
                if (index > 0)
                { 
                    ret = true;
                }
            }

            if (ret == false)
            {
                string slog = string.Format("IsValidUserShippingInfo Fail user: {0} number: {1}, zip: {2} -- shippingInfo: {3}", user.Email, user.StreetAddress, user.PostalCode, shippingInfo);
                logger.Debug(slog);
            }

            //int index = shippingAddress.IndexOf(' ');
            //if (index > 0)
            //{
            //    streetNumber = shippingAddress.Substring(0, index);
            //}
            //index = shippingAddress.LastIndexOf(' ') + 1;
            //if (index > 0)
            //{
            //    zipCode = shippingAddress.Substring(index);
            //}

            //var obj = userShippingInfo.All().Where(m => m.UserId == userId && m.ShippingInfo.ToLower() == shippingAddress.ToLower()).FirstOrDefault();
            //if (obj != null)
            //{
            //    ret = true;
            //}

            //if (streetNumber == user.StreetAddress && zipCode == user.PostalCode)
            //{
            //    ret = true;
            //}
            return ret;
        }

        public void SaveUserShippingInfo(string userId, string shippingInfo)
        {
            var obj = userShippingInfo.All().Where(m => m.UserId == userId && m.ShippingInfo.ToLower() == shippingInfo.ToLower()).FirstOrDefault();
            if (obj == null)
            {
                UserShippingInfo si = new UserShippingInfo()
                {
                    UserId = userId,
                    ShippingInfo = shippingInfo
                };
                userShippingInfo.Add(si);
                userShippingInfo.SaveChanges();
            }
        }

        public PaymentLog GetPaymentByTid(string tid)
        {
            PaymentLog p = payment.All().Where(m => m.TId == tid).FirstOrDefault();
            return p;
        }

        public PaymentLog CheckPaymentByDeviceTokenCode(string deviceToken, string code)
        {
            PaymentLog ret = null;
            User  u = user.All().Where(m => m.PhoneDeviceToken == deviceToken).FirstOrDefault();
            if (u != null)
            {
                ret = payment.All().Where(m => m.Email == u.Email && m.CodeCheck == code).OrderByDescending(m => m.RegisteredAt).FirstOrDefault();
            }
            return ret;
        }

        public void SavePaymentRequestConfimred(string tid)
        {
            PaymentLog log = GetPaymentByTid(tid);
            if (log != null)
            {
                //PaymentLogConfirmed confirmed; // = new PaymentLogConfirmed();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<PaymentLog, PaymentLogConfirmed>());
                var mapper = config.CreateMapper();

                PaymentLogConfirmed confirmed = mapper.Map<PaymentLogConfirmed>(log);
                paymentConfirmed.Add(confirmed);
                paymentConfirmed.SaveChanges();
                payment.Delete(log);
            }
        }

        public void SavePaymentRequestRejected(string tid)
        {
            PaymentLog log = GetPaymentByTid(tid);
            if (log != null)
            {
                //                PaymentLogRejected rejected;  // = new PaymentLogRejected();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<PaymentLog, PaymentLogRejected>());
                var mapper = config.CreateMapper();

                PaymentLogRejected rejected = mapper.Map<PaymentLogRejected>(log);
                paymentRejected.Add(rejected);
                paymentRejected.SaveChanges();
                payment.Delete(log);
            }
        }
    }
}

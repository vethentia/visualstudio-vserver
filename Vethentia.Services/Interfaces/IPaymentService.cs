
namespace Vethentia.Services.Interfaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Vethentia.Data.Models;

    public interface IPaymentService
    {
        string RequestPay(PaymentLog payment);

        IEnumerable<PaymentLog> Payments();

        PaymentLog GetPayment(string id);

        bool UpdatePaymentByTid(string tid, PaymentLog log);

        bool IsValidUserShippingInfo(User user, string shippingInfo);

        void SaveUserShippingInfo(string userId, string shppingInfo);

        PaymentLog GetPaymentByTid(string tid);
        PaymentLog CheckPaymentByDeviceTokenCode(string deviceToken, string code);

        void SavePaymentRequestConfimred(string tid);

        void SavePaymentRequestRejected(string tid);
    }
}

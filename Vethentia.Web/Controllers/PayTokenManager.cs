using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vethentia.Data.Models;
using Vethentia.ViewModels.WebApi.Payment;

namespace Vethentia.Web.Controllers
{
    public sealed class PayTokenManager
    {
        private Dictionary<long, string> _dictValue;
        private Dictionary<long, PaymentLog> _dictPaymentLog;

        private static readonly PayTokenManager instance = new PayTokenManager();

        private PayTokenManager()
        {
            _dictValue = new Dictionary<long, string>();
            _dictPaymentLog = new Dictionary<long, PaymentLog>();
        }

        public static PayTokenManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="token"></param>
        public void Save (long tid, string token)
        {
            string test;
            if (instance._dictValue.TryGetValue(tid, out test)) // Returns true.
            {
                instance._dictValue[tid] = token;
            }
            else
            { 
                instance._dictValue.Add(tid, token);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <returns>true: successfully, key existed</returns>
        public bool Get (long key, out string Value)
        {
            return instance._dictValue.TryGetValue(key, out Value);
        }

        public void Delete (long key)
        {
            instance._dictValue.Remove(key);
        }

        public int Count ()
        {
            return instance._dictValue.Count();
        }

        public void Clear()
        {
            instance._dictValue.Clear();
        }

        ////////////////////////////////////////////
        //
        // hold single instance of PaymentLog
        //
        ////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="pay"></param>
        public void SavePaymentLog(long tid, PaymentLog pay)
        {
            PaymentLog test;
            if (instance._dictPaymentLog.TryGetValue(tid, out test)) // Returns true.
            {
                instance._dictPaymentLog[tid] = pay;
            }
            else
            {
                instance._dictPaymentLog.Add(tid, pay);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <returns>true: successfully, key existed</returns>
        public bool GetPaymentLog(long key, out PaymentLog Value)
        {
            return instance._dictPaymentLog.TryGetValue(key, out Value);
        }

        public void DeletePaymentLog(long key)
        {
            instance._dictPaymentLog.Remove(key);
        }

        public int CountPaymentLog()
        {
            return instance._dictPaymentLog.Count();
        }

        public void ClearPaymentLog()
        {
            instance._dictPaymentLog.Clear();
        }

    }
}

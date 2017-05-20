using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vethentia.Data.Models;

namespace Vethentia.Services.Interfaces
{
    public interface IMerchantService
    {
        Merchant GetMerchant(string id);

        Merchant GetMerchantByAlias(string alias);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>returns the MerchantId or an empty string if it is error</returns>
        string SaveMerchant(string id, Merchant model);


        long SaveUserMerchant(long id, UserMerchant model);

    }
}

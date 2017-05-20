using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vethentia.Data;
using Vethentia.Data.Models;
using Vethentia.Services.Interfaces;

namespace Vethentia.Services.Implementations
{
    class MerchantService : IMerchantService
    {
        private readonly IRepository<User> user;
        private readonly IRepository<Merchant> merchant;
        private readonly IRepository<UserMerchant> userMerchant;

        public MerchantService(IRepository<User> userRep, IRepository<Merchant> merchantRep)
        {
            user = userRep;
            merchant = merchantRep;
        }

        public Merchant GetMerchant(string id)
        {
            Merchant mer = merchant.GetById(id);
            return mer;
        }

        public Merchant GetMerchantByAlias(string alias)
        {
            return merchant.All().Where(k => k.Alias == alias).FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns>Return the merchantId or an empty string if it is error</returns>
        public string SaveMerchant(string id, Merchant model)
        {
            string retId = string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                merchant.Add(model);
                int val = merchant.SaveChanges();
                retId = model.Id;
            }
            else
            {
                Merchant mer = GetMerchant(id);
                if (mer != null)
                {
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<Merchant, Merchant>());
                    var mapper = config.CreateMapper();

                    mer = mapper.Map<Merchant>(model);
                    mer.Id = id;

                    merchant.Update(mer);
                    merchant.SaveChanges();
                    retId = id;
                }

            }
            return retId;
        }


        public long SaveUserMerchant(long id, UserMerchant model)
        {
            long retId = 0;
            if (id <= 0)
            {
                userMerchant.Add(model);
                int val = userMerchant.SaveChanges();
                retId = model.Id;
            }
            else
            {
                UserMerchant usrMer = userMerchant.GetById(id);
                if (usrMer != null)
                {
                    usrMer.UserId = model.UserId;
                    usrMer.MerchantId = model.MerchantId;
                    userMerchant.Update(usrMer);
                    userMerchant.SaveChanges();
                    retId = id;
                }

            }
            return retId;
        }


    }
}


namespace Vethentia.Services.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Data;
    using Data.Models;
    using System;
    using AutoMapper;

    public class UserService : IUserService
    {
        private readonly IRepository<User> user;

        public UserService(IRepository<User> userRep)
        {
            user = userRep;
        }

        public bool IsPhoneNumberRegistered(string number)
        {
            bool ret = true;
            User usr = user.All().Where(m => m.PhoneNumber.Equals(number)).FirstOrDefault();
            if (usr == null)
            {
                ret = false;
            }

            return ret;
        }
        public bool ConfirmPhoneCode(string userId, string code)
        {
            bool ret = false;
            User usr = GetUser(userId);
            if (usr != null)
            {
                if (usr.PhoneNumberCode == code)
                {
                    usr.PhoneNumberConfirmed = true;
                    this.user.SaveChanges();
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// by Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUser(string userId)
        {
            return this.user.GetById(userId);
        }

        /// <summary>
        /// by Email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            return this.user.All().Where(k => k.Email == email).FirstOrDefault();
        }


        public void Update(string userId, User user)
        {
            var existingUser = this.user.GetById(userId);

            //MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<User, User>());
            //IMapper mapper = config.CreateMapper();
            //mapper.Map<User, User>(user, existingUser);
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.StreetAddress = user.StreetAddress;
            existingUser.City = user.City;
            existingUser.State = user.State;
            existingUser.PostalCode = user.PostalCode;
            existingUser.PhoneDeviceToken = user.PhoneDeviceToken;
            existingUser.APNSRegistrationId = user.APNSRegistrationId;
            existingUser.Cookie = user.Cookie;
            existingUser.PhoneNumberCode = user.PhoneNumberCode;
            existingUser.PhoneDeviceToken = user.PhoneDeviceToken;
            existingUser.LastSeen = DateTime.Now;
            
            this.user.SaveChanges();
        }
    }
}

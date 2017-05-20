
namespace Vethentia.Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using Models;

    public class Configuration : DbMigrationsConfiguration<VethentiaDbContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(VethentiaDbContext context)
        {
            var manager = new UserManager<User>(new UserStore<User>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string adminuser = "admin@vrayinc.com";
            var user = new User
            {
                Email = adminuser,
                UserName = adminuser,
                FirstName = "Admin",
                LastName = "Admin",
                EmailConfirmed = true,
                PhoneNumber = "+17173683214",
                Cookie = "private@htbanh@yahoo.com",
                StreetAddress = "123",
                PostalCode = "21075",
                PhoneDeviceToken = "012306e5879cf758a1ce6ec56717081d51052045d7854752c245534d1258ae68",
                RegisteredAt = DateTime.Now
            };

            manager.Create(user, "admin@vrayinc.com");

            if (!roleManager.Roles.Any())
            {
                roleManager.Create(new IdentityRole { Name = "AdminSite" });
                roleManager.Create(new IdentityRole { Name = "AdminMerchant" });
            }

            var adminUser = manager.FindByName(adminuser);

            manager.AddToRole(adminUser.Id, "AdminSite");

            AddMerchant(context, adminUser);
        }

        private void AddMerchant(VethentiaDbContext context, User adminUser)
        {
            string guid = "0123456789abcdef01234567abcdef01";
            Merchant newMerchant = new Merchant
            {
                Id = guid,
                Alias = "merchant.com.vray.vpay",
                MerchantName = "Merchant.com",
                SupportNetwork = 7,
                MerchantCapabilities = 1,
                PaymnetMethodTokenizationType = "NETWORK_TOKEN",
                Email = adminUser.Email,
                FirstName = "Hai",
                LastName = "Banh",
                Phone = "123456789",
                StreetAddress = "123 King St",
                City = "San Diego",
                State = "CA",
                PostalCode = "92093",
                RegisteredAt = DateTime.Now
            };

            if (!context.Merchants.Any())
            { 
                context.Merchants.Add(newMerchant);
                var lastMerchant = context.Merchants.Where(x => x.Email == adminUser.Email).FirstOrDefault();
                if (lastMerchant == null)
                { 
                    UserMerchant um = new UserMerchant
                    {
                        UserId = adminUser.Id,
                        MerchantId = newMerchant.Id
                    };
                    context.UserMerchants.Add(um);
                    context.SaveChanges();

                    AddPayment(context, adminUser, newMerchant);
                }
            }
        }

        private void AddPayment(VethentiaDbContext context, User adminUser, Merchant merchant)
        {
            PaymentLog pay = new PaymentLog()
            {
                TId = "TIdtest1",
                TTime = DateTime.Now.ToString(),
                Email = "htbanh@yahoo.com",
                Mac = "Mac1",
                ShippingInfo = "Address, City, CA 12345",
                Amount = "1.99",
                CountryCode = "US",
                CurrencyCode = "USD",
                SupportedNetwork = 1,
                MerchantId = merchant.Alias,
                MechantName = merchant.MerchantName,
                MerchantCapability = merchant.MerchantCapabilities,
                LineItems = "lineitem1",
                PaymentMethodTokenizationType = merchant.PaymnetMethodTokenizationType,
                PublicKey = "publickey1"
            };
            context.PaymentLogs.Add(pay);
            context.SaveChanges();

        }

    }

}

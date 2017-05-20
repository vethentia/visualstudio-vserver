namespace Vethentia.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Linq;

    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using Models.Base;

    public class VethentiaDbContext : IdentityDbContext<User>, IVethentiaDbContext
    {
        public VethentiaDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public IDbSet<Merchant> Merchants { get; set; }

        public IDbSet<UserMerchant> UserMerchants { get; set; }

        public IDbSet<PaymentLog> PaymentLogs { get; set; }

        public IDbSet<PaymentLogConfirmed> PaymentLogConfirmeds { get; set; }

        public IDbSet<PaymentLogRejected> PaymentLogRejecteds { get; set; }

        public IDbSet<UserShippingInfo> UserShippingInfos { get; set; }

        public IDbSet<Log4Net> Log4Net { get; set; }

        public IDbSet<UserSession> UserSessions { get; set; }


        public static VethentiaDbContext Create()
        {
            return new VethentiaDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentLog>().ToTable("PaymentLogs");
            modelBuilder.Entity<PaymentLog>().HasKey(j => j.Id);

            modelBuilder.Entity<Merchant>().ToTable("Merchants");
            modelBuilder.Entity<Merchant>().HasKey(j => j.Id);

            modelBuilder.Entity<Log4Net>().ToTable("Log4Net");
            modelBuilder.Entity<Log4Net>().HasKey(j => j.Id);

            modelBuilder.Entity<UserMerchant>().ToTable("UserMerchants");
            modelBuilder.Entity<UserMerchant>().HasKey(j => j.Id);

            modelBuilder.Entity<UserShippingInfo>().ToTable("UserShippingInfoes");
            modelBuilder.Entity<UserShippingInfo>().HasKey(j => j.Id);

            base.OnModelCreating(modelBuilder);
        }


        public override int SaveChanges()
        {
            this.ApplyAuditInfoRules();
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.GetType() + "." + x.PropertyName + ": " + x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(
                    ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(
                    exceptionMessage, ex.EntityValidationErrors);
            }
        }

        private void ApplyAuditInfoRules()
        {
            foreach (var entry in
                this.ChangeTracker.Entries()
                    .Where(
                        e =>
                        e.Entity is IAuditInfo && ((e.State == EntityState.Added) || (e.State == EntityState.Modified))))
            {
                var entity = (IAuditInfo)entry.Entity;
                if (entry.State == EntityState.Added && entity.RegisteredAt == default(DateTime))
                {
                    entity.RegisteredAt = DateTime.UtcNow;
                }
                else
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

    }
}

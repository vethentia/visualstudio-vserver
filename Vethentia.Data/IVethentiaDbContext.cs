namespace Vethentia.Data
{
    using Models;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public interface IVethentiaDbContext
    {
        IDbSet<Merchant> Merchants { get; set; }
        IDbSet<UserMerchant> UserMerchants { get; set; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        void Dispose();

        int SaveChanges();
    }
}

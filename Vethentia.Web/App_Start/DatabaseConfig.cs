﻿namespace Vethentia.Web
{
    using System.Data.Entity;

    using Data;
    using Data.Migrations;

    public static class DatabaseConfig
    {
        public static void Initialize()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<VethentiaDbContext, Configuration>());
            VethentiaDbContext.Create().Database.Initialize(true);
        }
    }
}
namespace Vethentia.Web
{
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Mvc;

    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;

    using Data;

    using Services.Interfaces;
    public static class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            Register(builder);
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<VethentiaDbContext>().As<IVethentiaDbContext>().InstancePerRequest();
            //builder.RegisterType<VethentiaDbContext>().As<IVethentiaDbContext>().SingleInstance();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerRequest();

            var servicesAssembly = Assembly.GetAssembly(typeof(IUserService));
            builder.RegisterAssemblyTypes(servicesAssembly).AsImplementedInterfaces();

        }
    }
}
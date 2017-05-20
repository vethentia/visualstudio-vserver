namespace Vethentia.Web.Controllers.Api
{
    using System.Web.Http;

    using Automapper;


    public abstract class BaseController : ApiController
    {
        protected IMapper Mapper => AutoMapperConfig.Configuration.CreateMapper();
    }
}
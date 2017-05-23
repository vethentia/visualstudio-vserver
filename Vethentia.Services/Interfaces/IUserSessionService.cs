using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vethentia.Services.Interfaces
{
    public interface IUserSessionService
    {
        void CreateUserSession(string username, string authToken);

        void InvalidateUserSession();

        bool ReValidateSession();

        void DeleteExpiredSessions();
    }
}

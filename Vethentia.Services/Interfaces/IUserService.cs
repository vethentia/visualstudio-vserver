namespace Vethentia.Services.Interfaces
{
    using Data.Models;

    public interface IUserService
    {
        void Update(string userId, User user);

        User GetUser(string userId);

        User GetUserByEmail(string email);

        bool ConfirmPhoneCode(string userId, string code);

        bool IsPhoneNumberRegistered(string number);

    }
}

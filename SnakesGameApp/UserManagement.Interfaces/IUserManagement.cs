using Microsoft.ServiceFabric.Services.Remoting;
using SharedModels;

namespace UserManagement.Interfaces
{
    public interface IUserManagement : IService
    {
        Task<bool> RegisterUser(string email, string password, string nickname, string region, int highestScore);
        Task<bool> LoginUser(string email, string password);
        Task<(bool, UserModel)> GetUserData(string email);
        Task UpdatePassword(ResetPasswordModel model);
        Task UpdateScore(ScoreUpdateModel model);
        Task<bool> IsCorrectPassword(string email, string password);
    }
}

using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using SharedModels;


namespace UserActor.Interfaces
{
    public interface IUserActor : IActor
    {
        Task<bool> RegisterUser(string email, string password, string nickname, string region, int highestScore);

        Task<bool> UserExists();

        Task SetHighestScore(int score);

        Task UpdatePassword(string newPassword);

        Task<UserModel> GetUserData();
    }
}

using Microsoft.ServiceFabric.Services.Remoting;
using UserLogic.Models;

namespace RegionalBestScoresService.Interfaces
{
    public interface IRegionalBestScoresService : IService
    {
        Task AddPlayerAsync(UserModel playerInfo);

        Task UpdatePlayerNicknameAsync(string email, string newNickname);

        Task<UserModel[]> GetTop5PlayersAsync();
    }
}

using Microsoft.ServiceFabric.Services.Remoting;
using SharedModels;

namespace DataUpdates.Interfaces
{
    public interface IDataUpdates : IService
    {
        Task<List<UserModel>> GetAllUsersAsync();
        Task<LeaderboardData> UpdateLeaderboardsAsync();
    }
}

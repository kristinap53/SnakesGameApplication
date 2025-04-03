using Microsoft.ServiceFabric.Services.Remoting;
using SharedModels;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace RealTimeBroadcast
{
    public interface IRealTimeBroadcast : IService
    {
        Task InitializeDashboard(string connectionId);
        Task UpdateDashboard(string connectionId);
        Task UpdateScore(string connectionId, ScoreUpdateModel model);

    }
}

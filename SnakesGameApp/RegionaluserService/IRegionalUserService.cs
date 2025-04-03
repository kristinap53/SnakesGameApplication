using Microsoft.ServiceFabric.Services.Remoting;
using SharedModels;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace RegionalUserService.Interfaces
{
    public interface IRegionalUserService : IService
    {
        public Task<bool> AddUserToRegion(UserModel model);
        public Task<bool> UpdateUserInRegion(UserModel model);
        public Task<bool> IsUserInRegion(string email);
        public Task<List<UserModel>> GetUsersInRegion();

    }
}

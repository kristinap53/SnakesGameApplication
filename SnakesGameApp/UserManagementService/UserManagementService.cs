using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System.Threading.Tasks;
using UserActor.Interfaces;
using UserManagement.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using SharedModels;
using Microsoft.AspNetCore.Mvc;
using DataUpdates.Interfaces;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using RegionalUserService.Interfaces;

namespace UserManagementService
{
    public class UserManagementService : StatelessService, IUserManagement
    {
        public UserManagementService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        public async Task<bool> RegisterUser(string email, string password, string nickname, string region, int highestScore)
        {
            var actorId = new ActorId(email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));
            await userActor.RegisterUser(email, password, nickname, region, highestScore);
            return true;
        }

        public async Task<bool> LoginUser(string email, string password)
        {
            var actorId = new ActorId(email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));
            var userData = await userActor.GetUserData();
            return userData.Email == email;
        }

        public async Task UpdatePassword(ResetPasswordModel model)
        {
            var actorId = new ActorId(model.Email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));

            await userActor.UpdatePassword(model.newPassword);
        }

        public async Task UpdateScore(ScoreUpdateModel model)
        {
            var actorId = new ActorId(model.Email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));
            await userActor.SetHighestScore(model.NewScore);
            var _regionalUserServiceProxy = ServiceProxy.Create<IRegionalUserService>(
                new Uri("fabric:/SnakesGameApp/RegionaluserService"),
                new ServicePartitionKey(model.Region));
            var userData = await userActor.GetUserData();
            await _regionalUserServiceProxy.UpdateUserInRegion(userData);
        }

        public async Task<(bool, UserModel)> GetUserData(string email)
        {
            var actorId = new ActorId(email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));
            var userData = await userActor.GetUserData();
            return (true, userData);
        }

        public async Task<bool> IsCorrectPassword(string email, string password)
        {
            var actorId = new ActorId(email);
            var userActor = ActorProxy.Create<IUserActor>(actorId, new Uri("fabric:/SnakesGameApp/UserActorService"));
            var userData = await userActor.GetUserData();
            return userData.Password == password;
        }
    }

}

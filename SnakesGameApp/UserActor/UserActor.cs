using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using SharedModels;
using System.Threading.Tasks;
using UserActor.Interfaces;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]

namespace UserActor
{
    [StatePersistence(StatePersistence.Persisted)]
    public class UserActor : Actor, IUserActor
    {

        private const string UserStateName = "userData";
        private UserModel _userModel;

        public UserActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "UserActor activated.");

            _userModel = await StateManager.GetOrAddStateAsync<UserModel>(
                UserStateName,
                new UserModel(string.Empty, string.Empty, string.Empty, string.Empty, 0)
            );
        }

        public async Task<bool> RegisterUser(string email, string password, string nickname, string region, int highestScore)
        {
            if (_userModel != null && !string.IsNullOrEmpty(_userModel.Email))
            {
                return false;
            }
            _userModel = new UserModel(email, password, nickname, region, highestScore);
            await StateManager.SetStateAsync(UserStateName, _userModel);
            return true;
        }

        public async Task<bool> UserExists()
        {
            return _userModel != null && !string.IsNullOrEmpty(_userModel.Email);
        }

        public async Task SetHighestScore(int score)
        {
            _userModel.HighestScore = score;
            await StateManager.SetStateAsync(UserStateName, _userModel);
        }

        public async Task UpdatePassword(string newPassword)
        {
            _userModel.Password = newPassword;
            await StateManager.SetStateAsync(UserStateName, _userModel);
        }

        public Task<UserModel> GetUserData()
        {
            return Task.FromResult(_userModel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using RegionalUserService.Interfaces;
using System.Fabric;
using SharedModels;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using UserManagement.Interfaces;
using System.Fabric.Description;

namespace RegionalUserService
{
    internal sealed class RegionalUserService : StatefulService, IRegionalUserService
    {
        private const string RegionalUsersDictionary = "regionalUsersDictionary";
        private IReliableDictionary<string, UserModel> _regionalUsers;
        public RegionalUserService(StatefulServiceContext context)
            : base(context) { }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            if (this.Partition.PartitionInfo is NamedPartitionInformation partitionInfo)
            {
                ServiceEventSource.Current.Message($"Starting RegionalUserService for partition: {partitionInfo.Name}");
                _regionalUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserModel>>(RegionalUsersDictionary);
                await AddDummyUsersAsync(partitionInfo.Name);
            }
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        private IUserManagement GetUserManagementProxy()
        {
            return ServiceProxy.Create<IUserManagement>(
                new Uri("fabric:/SnakesGameApp/UserManagementService"));
        }

        private async Task AddDummyUsersAsync(string partitionName)
        {
            var users = new List<UserModel>
            {
                new UserModel("jane23@gmail.com", "jane123", "Jane", "NA", 30),
                new UserModel("john23@gmail.com", "john123", "John", "NA", 90),
                new UserModel("alice23@gmail.com", "alice123", "Alice", "NA", 150),
                new UserModel("bob23@gmail.com", "bob123", "Bob", "NA", 0),
                new UserModel("charlie23@gmail.com", "charlie123", "Charlie", "NA", 70),
                new UserModel("eve23@gmail.com", "eve123", "Eve", "EU", 50),
                new UserModel("grace23@gmail.com", "grace123", "Grace", "EU", 120),
                new UserModel("hank23@gmail.com", "hank123", "Hank", "EU", 70),
                new UserModel("ivy23@gmail.com", "ivy123", "Ivy", "EU", 100),
                new UserModel("steve23@gmail.com", "steve123", "Steve", "EU", 10),
                new UserModel("haruto23@gmail.com", "haruto123", "Haruto", "AS", 70),
                new UserModel("minho23@gmail.com", "minho123", "Minho", "AS", 0),
                new UserModel("sakura23@gmail.com", "sakura123", "Sakura", "AS", 80),
                new UserModel("mei23@gmail.com", "mei123", "Mei", "SA", 50),
                new UserModel("jakari23@gmail.com", "jakari123", "Jakari", "AF", 110),
                new UserModel("ermias23@gmail.com", "ermias123", "Ermias", "AF", 130),
                new UserModel("ayana23@gmail.com", "ayana123", "Ayana", "AF", 30)
            };

            var currentRegionUsers = users.Where(u => u.Region == partitionName).ToList();
            if (_regionalUsers == null)
            {
                _regionalUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserModel>>(RegionalUsersDictionary);
            }

            using (var tx = StateManager.CreateTransaction())
            {
                foreach (var user in currentRegionUsers)
                {
                    await _regionalUsers.SetAsync(tx, user.Email, user);
                    var userManagementProxy = GetUserManagementProxy();
                    await userManagementProxy.RegisterUser(user.Email, user.Password, user.Nickname, user.Region, user.HighestScore);
                }
                await tx.CommitAsync();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<bool> AddUserToRegion(UserModel model)
        {
            if (_regionalUsers == null)
            {
                _regionalUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserModel>>(RegionalUsersDictionary);
            }
            using (var tx = StateManager.CreateTransaction())
            {
                var userExists = await _regionalUsers.ContainsKeyAsync(tx, model.Email);
                if (userExists)
                {
                    return false; 
                }
                await _regionalUsers.SetAsync(tx, model.Email, model);
                await tx.CommitAsync();
                return true;
            }
        }

        public async Task<bool> UpdateUserInRegion(UserModel model)
        {
            if (_regionalUsers == null)
            {
                _regionalUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserModel>>(RegionalUsersDictionary);
            }
            using (var tx = StateManager.CreateTransaction())
            {
                var userExists = await _regionalUsers.ContainsKeyAsync(tx, model.Email);
                if (!userExists)
                {
                    return false;
                }
                await _regionalUsers.SetAsync(tx, model.Email, model);
                await tx.CommitAsync();
                return true;
            }
        }

        public async Task<List<UserModel>> GetUsersInRegion()
        {
            var result = new List<UserModel>();
            if (_regionalUsers == null)
            {
                _regionalUsers = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserModel>>(RegionalUsersDictionary);
            }

            using (var tx = StateManager.CreateTransaction())
            {
                var enumerator = (await _regionalUsers.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    result.Add(enumerator.Current.Value);
                }
            }
            return result;
        }

        public async Task<bool> IsUserInRegion(string email)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                return await _regionalUsers.ContainsKeyAsync(tx, email);
            }
        }
    }
}

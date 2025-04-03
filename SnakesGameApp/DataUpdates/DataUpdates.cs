using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataUpdates.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using RegionalUserService.Interfaces;
using SharedModels;
using UserManagement.Interfaces;

namespace DataUpdates
{
    public class DataUpdates : StatelessService, IDataUpdates
    {
        private int _updateCount = 0;
        public DataUpdates(StatelessServiceContext context)
            : base(context) {  }

        private IRegionalUserService GetRegionalUserServiceProxy(string region)
        {
            return ServiceProxy.Create<IRegionalUserService>(new Uri("fabric:/SnakesGameApp/RegionaluserService"),
                new ServicePartitionKey(region));
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await DefineDescription();
            await base.OnOpenAsync(cancellationToken);
        }

        private async Task DefineDescription()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new();

            DefineCustomMetrics(updateDescription);
            DefineAutoScaling(updateDescription);

            await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void DefineCustomMetrics(StatelessServiceUpdateDescription updateDescription)
        {
            var leaderboardUpdatesMetric = new StatelessServiceLoadMetricDescription
            {
                Name = "LeaderboardUpdateCount",
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            updateDescription.Metrics ??= new MetricsCollection();
            updateDescription.Metrics.Add(leaderboardUpdatesMetric);
        }

        private void DefineAutoScaling(StatelessServiceUpdateDescription updateDescription)
        {
            PartitionInstanceCountScaleMechanism partitionInstanceCountScaleMechanism = new PartitionInstanceCountScaleMechanism
            {
                MinInstanceCount = 1,
                MaxInstanceCount = 5,
                ScaleIncrement = 2
            };

            AveragePartitionLoadScalingTrigger averagePartitionLoadScalingTrigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = "LeaderboardUpdateCount",
                LowerLoadThreshold = 2,
                UpperLoadThreshold = 5,
                ScaleInterval = TimeSpan.FromSeconds(30)
            };
            ScalingPolicyDescription scalingPolicyDescription = new ScalingPolicyDescription(partitionInstanceCountScaleMechanism, averagePartitionLoadScalingTrigger);
            updateDescription.ScalingPolicies ??= new List<ScalingPolicyDescription>();
            updateDescription.ScalingPolicies.Add(scalingPolicyDescription);
        }

        private void ReportCustomMetric(int loadValue)
        {
            var loadMetrics = new List<LoadMetric>
            {
                new LoadMetric("LeaderboardUpdateCount", loadValue)
            };
            ServiceEventSource.Current.ServiceMessage(this.Context, $"Reported custom metric: LeaderboardUpdateCount with value {loadValue}");
            Partition.ReportLoad(loadMetrics);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            var allPlayers = new List<UserModel>();
            var regions = new[] { "NA", "SA", "EU", "AS", "AF" };

            foreach (var region in regions)
            {
                var regionalUserServiceProxy = GetRegionalUserServiceProxy(region);
                var usersInRegion = await regionalUserServiceProxy.GetUsersInRegion();
                allPlayers.AddRange(usersInRegion);
            }

            return allPlayers;
        }

        public async Task<LeaderboardData> UpdateLeaderboardsAsync()
        {
            var players = await GetAllUsersAsync(); 
            var leaderboardData = new LeaderboardData
            {
                RegionalScores = players
                    .GroupBy(p => p.Region)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.HighestScore).Take(5).ToList()),

                GlobalScores = players
                    .OrderByDescending(p => p.HighestScore)
                    .Take(5)
                    .ToList(),

                PlayerCounts = players
                    .GroupBy(p => p.Region)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
            Console.WriteLine("Leaderboards updated.");
            _updateCount++;
            Console.WriteLine(_updateCount);
            ReportCustomMetric(_updateCount);
            return leaderboardData;
        }

    }
}

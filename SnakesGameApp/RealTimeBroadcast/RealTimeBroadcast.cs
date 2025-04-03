using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using static System.Net.WebRequestMethods;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using UserManagement.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using DataUpdates.Interfaces;
using System.Text.Json;
using DataUpdates;
using Microsoft.AspNetCore.Mvc;
using SharedModels;
using RegionalUserService.Interfaces;

namespace RealTimeBroadcast
{
    public sealed class RealTimeBroadcast : StatelessService, IRealTimeBroadcast
    {
        private IHubContext<GameHub> _gameHub;

        public RealTimeBroadcast(StatelessServiceContext context) : base(context) { }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            while (_gameHub == null)
            {
                await Task.Delay(100); 
            }
            await _gameHub.Clients.All.SendAsync("ConnectionEstablished", "Service is ready.");
            await base.OnOpenAsync(cancellationToken);
        }

        private IUserManagement GetUserManagementProxy()
        {
            return ServiceProxy.Create<IUserManagement>(
                new Uri("fabric:/SnakesGameApp/UserManagementService"));
        }

        private IDataUpdates GetDataUpdatesProxy()
        {
            return ServiceProxy.Create<IDataUpdates>(
                new Uri("fabric:/SnakesGameApp/DataUpdates"));
        }


        public async Task InitializeDashboard(string connectionId)
        {
            if (_gameHub == null)
            {
                throw new InvalidOperationException("SignalR Hub context is not initialized.");
            }

            var _dataUpdatesProxy = GetDataUpdatesProxy();
            var leaderboardData = await _dataUpdatesProxy.UpdateLeaderboardsAsync();

            if (_gameHub != null)
            {
                var leaderboardScore = leaderboardData.RegionalScores;
                await _gameHub.Clients.Client(connectionId).SendAsync("InitializeDashboard", new
                {
                    RegionalLeaderboard = leaderboardData.RegionalScores != null
                        ? JsonSerializer.Serialize(leaderboardData.RegionalScores) : "[]",
                    GlobalLeaderboard = leaderboardData.GlobalScores != null
                        ? JsonSerializer.Serialize(leaderboardData.GlobalScores) : "[]",
                    PlayerCount = leaderboardData.PlayerCounts != null
                    ? JsonSerializer.Serialize(leaderboardData.PlayerCounts) : "{}"
                });
            }
            else
            {
                Console.Error.WriteLine("Error: _gameHub is null. SignalR context not yet established.");
            }
        }

        public async Task UpdateDashboard(string connectionId)
        {
            var _dataUpdatesProxy = GetDataUpdatesProxy();
            var leaderboardData = await _dataUpdatesProxy.UpdateLeaderboardsAsync();

            await _gameHub.Clients.All.SendAsync("UpdateDashboard", new
            {
                RegionalLeaderboard = leaderboardData.RegionalScores != null
                        ? JsonSerializer.Serialize(leaderboardData.RegionalScores) : "[]",
                GlobalLeaderboard = leaderboardData.GlobalScores != null
                        ? JsonSerializer.Serialize(leaderboardData.GlobalScores) : "[]",
                PlayerCount = leaderboardData.PlayerCounts != null
                    ? JsonSerializer.Serialize(leaderboardData.PlayerCounts) : "{}"
            });
        }

        public async Task UpdateScore(string connectionId, ScoreUpdateModel model)
        {
            var _userManagementProxy = GetUserManagementProxy();
            await _userManagementProxy.UpdateScore(model);
            await UpdateDashboard(connectionId);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new FabricTransportServiceRemotingListener(serviceContext, this,
                    new FabricTransportRemotingListenerSettings() { EndpointResourceName = "ServiceEndpoint" }),
                    "ServiceEndpoint"),

                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "SignalREndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("AllowAll", policy =>
                            {
                                policy.WithOrigins("http://localhost:3000", "http://localhost:19081")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials();
                            });
                        });

                        builder.Services.AddSignalR();
                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);

                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        var app = builder.Build();

                        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.UseCors("AllowAll");

                        app.UseStaticFiles();
                        app.UseRouting();
                        app.UseWebSockets();
                        app.MapHub<GameHub>("/gameHub");

                        _gameHub = app.Services.GetService<IHubContext<GameHub>>()
                            ?? throw new InvalidOperationException("SignalR Hub context could not be resolved.");
                        return app;
                    }), "SignalREndpoint")
            };
        }
    }
}

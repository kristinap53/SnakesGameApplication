using Microsoft.AspNetCore.SignalR;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using SharedModels;

namespace RealTimeBroadcast
{
    public class GameHub : Hub
    {
        private ServiceProxyFactory _proxyFactory;
        private static readonly HashSet<string> _connectedClients = new();

        public GameHub()
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
        }

        private IRealTimeBroadcast GetBroadcastProxy()
        {
            return _proxyFactory.CreateServiceProxy<IRealTimeBroadcast>(
                new Uri("fabric:/SnakesGameApp/RealTimeBroadcast"), listenerName: "ServiceEndpoint");
        }

        public override async Task OnConnectedAsync()
        {
            if (_connectedClients.Contains(Context.ConnectionId))
                return; 

            _connectedClients.Add(Context.ConnectionId);
            var broadcastProxy = GetBroadcastProxy();
            var connectionId = Context.ConnectionId;
            await broadcastProxy.InitializeDashboard(connectionId);
            await base.OnConnectedAsync();
        }

        public async Task UpdateScore(string connectionId, ScoreUpdateModel model)
        {
            var broadcastProxy = GetBroadcastProxy();
            await broadcastProxy.UpdateScore(connectionId, model);
        }
    }
}


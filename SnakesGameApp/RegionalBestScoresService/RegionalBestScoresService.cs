using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using RegionalBestScoresService.Interfaces;
using UserLogic.Models;

namespace RegionalBestScoresService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class RegionalBestScoresService : StatelessService, IRegionalBestScoresService
    {
        private readonly Dictionary<string, UserModel> playerScores = new();

        public RegionalBestScoresService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }

        public async Task AddPlayerAsync(UserModel playerInfo)
        {
            playerScores[playerInfo.Email] = playerInfo;
            await Task.CompletedTask;
        }

        public async Task UpdatePlayerNicknameAsync(string email, string newNickname)
        {
            if (playerScores.TryGetValue(email, out var playerInfo))
            {
                playerInfo.Nickname = newNickname;
            }
            await Task.CompletedTask;
        }

        public async Task<UserModel[]> GetTop5PlayersAsync()
        {
            return playerScores
                .Values
                .OrderByDescending(p => p.HighestScore)
                .Take(5)
                .ToArray();
        }
    }
}

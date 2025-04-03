using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    [DataContract]
    public class LeaderboardData
    {
        [DataMember]
        public Dictionary<string, List<UserModel>> RegionalScores { get; set; }
        [DataMember]
        public List<UserModel> GlobalScores { get; set; }
        [DataMember]
        public Dictionary<string, int> PlayerCounts { get; set; }
    }
}

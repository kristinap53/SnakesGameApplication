using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    [DataContract]
    public class ClientModel
    {
        public ClientModel()
        {
        }

        public ClientModel(string email, string nickname, string region, int highestScore)
        {
            Email = email;
            Nickname = nickname;
            Region = region;
            HighestScore = highestScore;
        }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Nickname { get; set; }
        [DataMember]
        public string Region { get; set; }
        [DataMember]
        public int HighestScore { get; set; }
    }
}

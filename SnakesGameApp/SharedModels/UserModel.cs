using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    [DataContract]
    public class UserModel
    {
        public UserModel()
        {
        }

        public UserModel(string email, string password, string nickname, string region, int highestScore)
        {
            Email = email;
            Password = password;
            Nickname = nickname;
            Region = region;
            HighestScore = highestScore;
        }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Nickname { get; set; }
        [DataMember]
        public string Region { get; set; }
        [DataMember]
        public int HighestScore { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    [DataContract]
    public class ScoreUpdateModel
    {
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int NewScore { get; set; }
        [DataMember]
        public string Region { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    [DataContract]
    public class ResetPasswordModel
    {
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string newPassword { get; set; }
    }
}

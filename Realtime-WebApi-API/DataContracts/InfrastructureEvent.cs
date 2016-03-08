using System;
using System.Runtime.Serialization;

namespace Realtime_WebApi_API.DataContracts
{
    [DataContract]
    public class InfrastructureEvent
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public Guid Id;

        [DataMember]
        public string message;
    }
}
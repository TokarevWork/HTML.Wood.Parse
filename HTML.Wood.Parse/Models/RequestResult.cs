using System.Runtime.Serialization;

namespace HTML.Wood.Parse.Models
{
    [DataContract]
    public class RequestResult
    {
        [DataMember(Name = "data")]
        public Data Data { get; set; }
    }
}
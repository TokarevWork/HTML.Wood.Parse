using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HTML.Wood.Parse.Models
{
    [DataContract]
    public class SearchReportWoodDeal
    {
        [DataMember(Name = "content")]
        public List<Content> Content { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }

        [DataMember(Name = "__typename")]
        public string Typename { get; set; }
    }
}
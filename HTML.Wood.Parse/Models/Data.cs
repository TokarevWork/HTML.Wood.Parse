using System.Runtime.Serialization;

namespace HTML.Wood.Parse.Models
{
    [DataContract]
    public class Data
    {
        [DataMember(Name = "searchReportWoodDeal")]
        public SearchReportWoodDeal SearchReportWoodDeal { get; set; }
    }
}
using System.Runtime.Serialization;

namespace HTML.Wood.Parse.Models
{

    [DataContract]
    public class Content
    {
        [DataMember(Name = "sellerName")]
        public string SellerName { get; set; }

        [DataMember(Name = "sellerInn")]
        public string SellerInn { get; set; }

        [DataMember(Name = "buyerName")]
        public string BuyerName { get; set; }

        [DataMember(Name = "buyerInn")]
        public string BuyerInn { get; set; }

        [DataMember(Name = "woodVolumeBuyer")]
        public double WoodVolumeBuyer { get; set; }

        [DataMember(Name = "woodVolumeSeller")]
        public double WoodVolumeSeller { get; set; }

        [DataMember(Name = "dealDate")]
        public string DealDate { get; set; }

        [DataMember(Name = "dealNumber")]
        public string DealNumber { get; set; }

        [DataMember(Name = "__typename")]
        public string Typename { get; set; }
    }
}

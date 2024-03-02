namespace AuctionPortal.Business.Models
{
    public class AuctionModel
    {
        public string Id { get; set; }
        public string ItemName { get; set; }
        public double StartingAmount { get; set; }
		public string CreatedByClientId { get; set; }
		public BidModel HighestBid { get; set; }
    }
}

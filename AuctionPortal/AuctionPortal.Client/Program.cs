using AuctionPortal.Business;
using Grpc.Core;

namespace AuctionPortal.Client
{
	class Program
	{
		private readonly static string ClientId = Guid.NewGuid().ToString();
		static async Task Main(string[] args)
		{
			var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
			var client = new AuctionPortalClient(new AuctionPortalService.AuctionPortalServiceClient(channel));

			client.SubscribeToInitiatedAuctions(HandleInitiatedAuctionEvent);
			client.SubscribeToBids(HandleBidEvent);
			client.SubscribeToClosedAuctions(HandleClosedAuctionEvent);

			while (true)
			{
				Console.WriteLine(@"
Please type the option that you want to do:
1) Initiate auction
2) Bid auction
3) Close auction
4) Exit");

				var optionSelected = Console.ReadLine();

				switch (optionSelected)
				{
					case "1":
						await client.InitiateAuctionAsync(new InitiateAuctionRequest { ItemName = "test1", StartingPrice = 111 });
						Console.WriteLine("Auction created!");
						break;
					case "2":
						await client.BidAuctionAsync(new BidRequest { AuctionId = "adasd" });
						Console.WriteLine("Bid sent!");
						break;
					case "3":
						await client.CloseAuctionAsync(new CloseAuctionRequest { AuctionId = "asddsaasdda" });
						Console.WriteLine("Auction closed!");
						break;
					case "4":
						return;
					default:
						Console.Clear();
						break;
				}
			}
		}

		public static async void HandleInitiatedAuctionEvent(AuctionEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received new auction: " + @event.AuctionId);
		}

		public static async void HandleBidEvent(BidEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received new bid: " + @event.AuctionId);
		}

		public static async void HandleClosedAuctionEvent(AuctionEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received closed auction: " + @event.AuctionId);
		}
	}
}

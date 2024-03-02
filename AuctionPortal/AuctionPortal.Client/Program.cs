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
						Console.WriteLine("Type name of the item to sell:");
						var itemName = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(itemName))
						{
							Console.WriteLine("Invalid item name provided");
							continue;
						}

						Console.WriteLine("Type starting amount to sell the item:");
						var itemAmountStr = Console.ReadLine();

						if (!double.TryParse(itemAmountStr, out double itemAmount))
						{
							Console.WriteLine("Invalid starting amount provided");
							continue;
						}

						var newAuction = await client.InitiateAuctionAsync(new InitiateAuctionRequest { ItemName = itemName, StartingAmount = itemAmount });
						Console.WriteLine($"Auction {newAuction.AuctionId} created!");
						break;
					case "2":
						Console.WriteLine("Type the id of the auction:");
						var auctionIdToBid = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(auctionIdToBid))
						{
							Console.WriteLine("Invalid auction id provided");
							continue;
						}

						Console.WriteLine("Type amount to bid:");
						var bidAmountStr = Console.ReadLine();

						if (!double.TryParse(bidAmountStr, out double bidAmount))
						{
							Console.WriteLine("Invalid bid amount provided");
							continue;
						}

						var bidResponse = await client.BidAuctionAsync(new BidRequest { AuctionId = auctionIdToBid, Amount = bidAmount });
						Console.WriteLine(bidResponse.Message);
						break;
					case "3":
						Console.WriteLine("Type the id of the auction:");
						var auctionIdToClose = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(auctionIdToClose))
						{
							Console.WriteLine("Invalid auction id provided");
							continue;
						}

						var auctionResponse = await client.CloseAuctionAsync(new CloseAuctionRequest { AuctionId = auctionIdToClose });
						Console.WriteLine(auctionResponse.Message);
						break;
					case "4":
						return;
					default:
						Console.Clear();
						break;
				}
			}
		}

		#region Event Handlers
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
		#endregion
	}
}

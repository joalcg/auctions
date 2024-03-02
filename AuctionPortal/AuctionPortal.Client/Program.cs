using AuctionPortal.Business;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;

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
						await InitiateAuction(client);
						break;
					case "2":
						await BidAuction(client);
						break;
					case "3":
						await CloseAuction(client);
						break;
					case "4":
						return;
					default:
						Console.Clear();
						break;
				}
			}
		}

		private static async Task InitiateAuction(AuctionPortalClient client)
		{
			var isValid = false;

			string itemName = null;
			double itemAmount = 0;

			while (!isValid)
			{
				Console.WriteLine("Type name of the item to sell:");
				itemName = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(itemName))
				{
					Console.WriteLine("Invalid item name provided");
					continue;
				}

				Console.WriteLine("Type starting amount to sell the item:");
				var itemAmountStr = Console.ReadLine();

				if (!double.TryParse(itemAmountStr, out itemAmount))
				{
					Console.WriteLine("Invalid starting amount provided");
					continue;
				}

				isValid = true;
			}

			var newAuction = await client.InitiateAuctionAsync(new InitiateAuctionRequest { ItemName = itemName, StartingAmount = itemAmount });
			Console.WriteLine($"Auction {newAuction.AuctionId} created!");
		}

		private static async Task BidAuction(AuctionPortalClient client)
		{
			var isValid = false;

			string auctionIdToBid = null;
			double bidAmount = 0;

			while (!isValid)
			{
				Console.WriteLine("Type the id of the auction:");
				auctionIdToBid = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(auctionIdToBid))
				{
					Console.WriteLine("Invalid auction id provided");
					continue;
				}

				Console.WriteLine("Type amount to bid:");
				var bidAmountStr = Console.ReadLine();

				if (!double.TryParse(bidAmountStr, out bidAmount))
				{
					Console.WriteLine("Invalid bid amount provided");
					continue;
				}

				isValid = true;
			}			

			var bidResponse = await client.BidAuctionAsync(new BidRequest { AuctionId = auctionIdToBid, Amount = bidAmount, ClientId = ClientId });
			Console.WriteLine(bidResponse.Message);
		}

		private static async Task CloseAuction(AuctionPortalClient client)
		{
			var isValid = false;

			string auctionIdToClose = null;

			while (!isValid)
			{
				Console.WriteLine("Type the id of the auction:");
				auctionIdToClose = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(auctionIdToClose))
				{
					Console.WriteLine("Invalid auction id provided");
					continue;
				}

				isValid = true;
			}

			var auctionResponse = await client.CloseAuctionAsync(new CloseAuctionRequest { AuctionId = auctionIdToClose });
			Console.WriteLine(auctionResponse.Message);
		}

		#region Event Handlers
		public static async void HandleInitiatedAuctionEvent(AuctionEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received new auction: {@event.AuctionId} - {@event.ItemName}");
		}

		public static async void HandleBidEvent(BidEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received new bid for auction: {@event.AuctionId} - {@event.Amount}");
		}

		public static async void HandleClosedAuctionEvent(AuctionEvent @event)
		{
			Console.WriteLine($"Client {ClientId} received closed auction: {@event.AuctionId} - {@event.ItemName}");
		}
		#endregion
	}
}

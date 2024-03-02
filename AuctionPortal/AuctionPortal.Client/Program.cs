using AuctionPortal.Business;
using Grpc.Core;

namespace AuctionPortal.Client
{
	class Program
	{
		const int Port = 50051;

		static async Task Main(string[] args)
		{
			//Channel channel = new Channel("127.0.0.1:" + Port, ChannelCredentials.Insecure);
			//var client = new Auctions.AuctionsClient(channel);

			//// Example usage
			//var initiateResponse = client.InitiateAuction(new InitiateAuctionRequest { ItemName = "Item 1", StartingPrice = 100 });
			//Console.WriteLine("Auction ID: " + initiateResponse.AuctionId);

			//var bidResponse = client.BidOnAuction(new InitiateBidRequest { AuctionId = initiateResponse.AuctionId, Amount = 150 });
			//Console.WriteLine("Bid response: " + bidResponse.Message);

			//var closeResponse = client.CloseAuction(new CloseAuctionRequest { AuctionId = initiateResponse.AuctionId });
			//Console.WriteLine("Close response: " + closeResponse.Message);

			//channel.ShutdownAsync().Wait();
			//Console.WriteLine("Press any key to exit...");
			//Console.ReadKey();

			using (var client = new AuctionClient("localhost", 50051))
			{
				await client.InitiateAuction("Item1", 100);
				await client.BidOnAuction("Item1", 120);
				await client.CloseAuction("Item1");
			}

			using (var client = new AuctionClient("localhost", 50052))
			{
				await client.InitiateAuction("Item1", 100);
				await client.BidOnAuction("Item1", 120);
				await client.CloseAuction("Item1");
			}


			Console.ReadLine();
		}
	}
}
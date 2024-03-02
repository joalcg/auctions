using AuctionPortal.Business;
using Grpc.Core;

namespace AuctionPortal
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var server = new Server
			{
				Services = { AuctionPortalService.BindService(new AuctionPortal.Business.Services.AuctionPortalService()) },
				Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) }
			};
			server.Start();

			var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
			var client1 = new AuctionPortalClient(new AuctionPortalService.AuctionPortalServiceClient(channel));
			var client2 = new AuctionPortalClient(new AuctionPortalService.AuctionPortalServiceClient(channel));
			var client3 = new AuctionPortalClient(new AuctionPortalService.AuctionPortalServiceClient(channel));

			client1.SubscribeToInitiatedAuctions((@event) =>
			{
				Console.WriteLine("Client 1 received new auction: " + @event.AuctionId);
			});
			client2.SubscribeToInitiatedAuctions((@event) =>
			{
				Console.WriteLine("Client 2 received new auction: " + @event.AuctionId);
			});
			client3.SubscribeToInitiatedAuctions((@event) =>
			{
				Console.WriteLine("Client 3 received new auction: " + @event.AuctionId);
			});

			client1.SubscribeToBids((@event) =>
			{
				Console.WriteLine("Client 1 received new bid: " + @event.AuctionId);
			});
			client2.SubscribeToBids((@event) =>
			{
				Console.WriteLine("Client 2 received new bid: " + @event.AuctionId);
			});
			client3.SubscribeToBids((@event) =>
			{
				Console.WriteLine("Client 3 received new bid: " + @event.AuctionId);
			});

			client1.SubscribeToClosedAuctions((@event) =>
			{
				Console.WriteLine("Client 1 received closed auction: " + @event.AuctionId);
			});
			client2.SubscribeToInitiatedAuctions((@event) =>
			{
				Console.WriteLine("Client 2 received closed auction: " + @event.AuctionId);
			});
			client3.SubscribeToInitiatedAuctions((@event) =>
			{
				Console.WriteLine("Client 3 received closed auction: " + @event.AuctionId);
			});

			await Task.Delay(500); // Wait for clients to subscribe before publishing

			while (true)
			{
				await client1.InitiateAuctionAsync(new InitiateAuctionRequest { ItemName = "test1", StartingPrice = 111 });
				await Task.Delay(1000); // Wait for clients to subscribe before publishing
			}

			Console.ReadLine();
		}
	}
}

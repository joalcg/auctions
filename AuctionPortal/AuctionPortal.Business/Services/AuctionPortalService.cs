using AuctionPortal.Business.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionPortal.Business.Services
{
	public class AuctionPortalService : global::AuctionPortalService.AuctionPortalServiceBase
	{
		private readonly Dictionary<string, AuctionModel> auctions = new();
		private readonly List<IServerStreamWriter<AuctionEvent>> initiatedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();
		private readonly List<IServerStreamWriter<BidEvent>> bidSubscribers = new List<IServerStreamWriter<BidEvent>>();
		private readonly List<IServerStreamWriter<AuctionEvent>> closedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();

		public override Task<InitiateAuctionResponse> InitiateAuction(InitiateAuctionRequest request, ServerCallContext context)
		{
			string auctionId = Guid.NewGuid().ToString();
			auctions.Add(auctionId, new AuctionModel { Id = auctionId, ItemName = request.ItemName, StartingAmount = request.StartingAmount });
			return Task.FromResult(new InitiateAuctionResponse { AuctionId = auctionId });
		}

		public override Task<BidResponse> BidAuction(BidRequest request, ServerCallContext context)
		{
			if (auctions.TryGetValue(request.AuctionId, out AuctionModel auction))
			{
				return Task.FromResult(new BidResponse { IsSuccess = true, Message = $"Bid successfully sent for auction {auction.Id}: {auction.ItemName}" });
			}
			else
			{
				return Task.FromResult(new BidResponse { IsSuccess = false, Message = "Auction not found" });
			}
		}

		public override Task<CloseAuctionResponse> CloseAuction(CloseAuctionRequest request, ServerCallContext context)
		{
			if (auctions.TryGetValue(request.AuctionId, out AuctionModel auction))
			{
				auctions.Remove(request.AuctionId);
				return Task.FromResult(new CloseAuctionResponse { AuctionId = auction.Id, ItemName = auction.ItemName, IsSuccess = true, Message = $"Auction {auction.Id} closed: {auction.ItemName}" });
			}
			else
			{
				return Task.FromResult(new CloseAuctionResponse { IsSuccess = false, Message = "Auction not found" });
			}
		}

		public override async Task SubscribeToInitiatedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
		{
			initiatedAuctionSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				await Task.Delay(100);
			}

			await Task.CompletedTask;
		}

		public override async Task SubscribeToBids(EmptyRequest request, IServerStreamWriter<BidEvent> responseStream, ServerCallContext context)
		{
			bidSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				await Task.Delay(100);
			}

			await Task.CompletedTask;
		}

		public override async Task SubscribeToClosedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
		{
			closedAuctionSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				await Task.Delay(100);
			}

			await Task.CompletedTask;
		}

		public override async Task PublishInitiatedAuction(AuctionEvent request, IServerStreamWriter<EmptyRequest> responseStream, ServerCallContext context)
		{
			foreach (var subscriber in initiatedAuctionSubscribers)
			{
				await subscriber.WriteAsync(request);
			}
			await Task.CompletedTask;
		}

		public override async Task PublishBid(BidEvent request, IServerStreamWriter<EmptyRequest> responseStream, ServerCallContext context)
		{
			foreach (var subscriber in bidSubscribers)
			{
				await subscriber.WriteAsync(request);
			}
			await Task.CompletedTask;
		}

		public override async Task PublishClosedAuction(AuctionEvent request, IServerStreamWriter<EmptyRequest> responseStream, ServerCallContext context)
		{
			foreach (var subscriber in closedAuctionSubscribers)
			{
				await subscriber.WriteAsync(request);
			}
			await Task.CompletedTask;
		}
	}
}

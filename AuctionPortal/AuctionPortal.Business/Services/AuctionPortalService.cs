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
		private Dictionary<string, AuctionInfo> _auctions = new Dictionary<string, AuctionInfo>();
		private readonly List<IServerStreamWriter<AuctionEvent>> initiatedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();
		private readonly List<IServerStreamWriter<BidEvent>> bidSubscribers = new List<IServerStreamWriter<BidEvent>>();
		private readonly List<IServerStreamWriter<AuctionEvent>> closedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();

		public override Task<InitiateAuctionResponse> InitiateAuction(InitiateAuctionRequest request, ServerCallContext context)
		{
			string auctionId = Guid.NewGuid().ToString();
			_auctions.Add(auctionId, new AuctionInfo { ItemName = request.ItemName, StartingPrice = request.StartingPrice });
			return Task.FromResult(new InitiateAuctionResponse { AuctionId = auctionId });
		}

		public override Task<BidResponse> BidAuction(BidRequest request, ServerCallContext context)
		{
			if (_auctions.TryGetValue(request.AuctionId, out AuctionInfo auction))
			{
				// Implement bidding logic
				//NotifyBidders(request.AuctionId, request.Amount, "New bid submitted");
				return Task.FromResult(new BidResponse { Success = true, Message = "Bid successful" });
			}
			else
			{
				return Task.FromResult(new BidResponse { Success = false, Message = "Auction not found" });
			}
		}

		public override Task<CloseAuctionResponse> CloseAuction(CloseAuctionRequest request, ServerCallContext context)
		{
			if (_auctions.TryGetValue(request.AuctionId, out AuctionInfo auction))
			{
				_auctions.Remove(request.AuctionId);
				return Task.FromResult(new CloseAuctionResponse { Success = true, Message = "Auction closed" });
			}
			else
			{
				return Task.FromResult(new CloseAuctionResponse { Success = false, Message = "Auction not found" });
			}
		}

		public override async Task SubscribeToInitiatedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
		{
			initiatedAuctionSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				// Avoid pegging CPU
				await Task.Delay(100);
			}

			await Task.CompletedTask;
		}

		public override async Task SubscribeToBids(EmptyRequest request, IServerStreamWriter<BidEvent> responseStream, ServerCallContext context)
		{
			bidSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				// Avoid pegging CPU
				await Task.Delay(100);
			}

			await Task.CompletedTask;
		}

		public override async Task SubscribeToClosedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
		{
			closedAuctionSubscribers.Add(responseStream);

			while (!context.CancellationToken.IsCancellationRequested)
			{
				// Avoid pegging CPU
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

	public class AuctionInfo
	{
		public string ItemName { get; set; }
		public double StartingPrice { get; set; }
	}
}

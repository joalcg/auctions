using AuctionPortal.Business.Models;
using Grpc.Core;

namespace AuctionPortal.Business.Services
{
    public class AuctionPortalService : global::AuctionPortalService.AuctionPortalServiceBase
    {
        //TODO: move auctions from a variable in memory to a database
        private readonly Dictionary<string, AuctionModel> auctions = new();
        private readonly List<IServerStreamWriter<AuctionEvent>> initiatedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();
        private readonly List<IServerStreamWriter<BidEvent>> bidSubscribers = new List<IServerStreamWriter<BidEvent>>();
        private readonly List<IServerStreamWriter<AuctionEvent>> closedAuctionSubscribers = new List<IServerStreamWriter<AuctionEvent>>();

        public override Task<InitiateAuctionResponse> InitiateAuction(InitiateAuctionRequest request, ServerCallContext context)
        {
            string auctionId = Guid.NewGuid().ToString();
            auctions.Add(auctionId, new AuctionModel 
            { 
                Id = auctionId, 
                ItemName = request.ItemName, 
                StartingAmount = request.StartingAmount,
                CreatedByClientId = request.CreatedByClientId
            });
            return Task.FromResult(new InitiateAuctionResponse { AuctionId = auctionId });
        }

        public override Task<BidResponse> BidAuction(BidRequest request, ServerCallContext context)
        {
            if (auctions.TryGetValue(request.AuctionId, out AuctionModel auction))
            {
                if (auction.StartingAmount > request.Amount || (auction.HighestBid is not null && auction.HighestBid.Amount > request.Amount))
                {
                    return Task.FromResult(new BidResponse
                    {
                        IsSuccess = false,
                        Message = $"Higher amount must be sent for auction {auction.Id}: {auction.ItemName}"
                    });
                }

                auction.HighestBid = new BidModel
                {
                    Amount = request.Amount,
                    ClientId = request.ClientId
                };

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
                if (auction.CreatedByClientId != request.ClosedByClientId)
					return Task.FromResult(new CloseAuctionResponse { IsSuccess = false, Message = "Client does not have permissions to close auction" });

				var winnerText = auction.HighestBid is null ? "N/A" : $"Highest bid: {auction.HighestBid.Amount} - {auction.HighestBid.ClientId}";

                auctions.Remove(request.AuctionId);
                return Task.FromResult(new CloseAuctionResponse
                {
                    AuctionId = auction.Id,
                    ItemName = auction.ItemName,
                    IsSuccess = true,
                    Message = $"Auction {auction.Id} closed: {auction.ItemName}. Winner: {winnerText}",
                    WonByClientId = auction.HighestBid?.ClientId ?? string.Empty // TODO: handle null values in a better way
                });
            }
            else
            {
                return Task.FromResult(new CloseAuctionResponse { IsSuccess = false, Message = "Auction not found" });
            }
        }

        // TODO: explore options to reuse logic on subscribe methods
        public override async Task SubscribeToInitiatedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
        {
            initiatedAuctionSubscribers.Add(responseStream);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            await Task.CompletedTask;
        }

		// TODO: explore options to reuse logic on subscribe methods
		public override async Task SubscribeToBids(EmptyRequest request, IServerStreamWriter<BidEvent> responseStream, ServerCallContext context)
        {
            bidSubscribers.Add(responseStream);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            await Task.CompletedTask;
        }

		// TODO: explore options to reuse logic on subscribe methods
		public override async Task SubscribeToClosedAuctions(EmptyRequest request, IServerStreamWriter<AuctionEvent> responseStream, ServerCallContext context)
        {
            closedAuctionSubscribers.Add(responseStream);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }

            await Task.CompletedTask;
        }

		// TODO: explore options to reuse logic on publish methods
		public override async Task PublishInitiatedAuction(AuctionEvent request, IServerStreamWriter<EmptyRequest> responseStream, ServerCallContext context)
        {
            foreach (var subscriber in initiatedAuctionSubscribers)
            {
                await subscriber.WriteAsync(request);
            }
            await Task.CompletedTask;
        }

		// TODO: explore options to reuse logic on publish methods
		public override async Task PublishBid(BidEvent request, IServerStreamWriter<EmptyRequest> responseStream, ServerCallContext context)
        {
            foreach (var subscriber in bidSubscribers)
            {
                await subscriber.WriteAsync(request);
            }
            await Task.CompletedTask;
        }

		// TODO: explore options to reuse logic on publish methods
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

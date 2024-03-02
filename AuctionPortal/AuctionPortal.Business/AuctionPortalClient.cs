namespace AuctionPortal.Business
{
    public class AuctionPortalClient
    {
        private readonly AuctionPortalService.AuctionPortalServiceClient client;
        private readonly CancellationTokenSource cancellationTokenSource;

        public AuctionPortalClient(AuctionPortalService.AuctionPortalServiceClient client)
        {
            this.client = client;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<InitiateAuctionResponse> InitiateAuctionAsync(InitiateAuctionRequest initiateAuctionRequest)
        {
            var auctionResponse = await client.InitiateAuctionAsync(initiateAuctionRequest);
            client.PublishInitiatedAuction(new AuctionEvent
            {
                AuctionId = auctionResponse.AuctionId,
                ItemName = auctionResponse.ItemName
            });
            return auctionResponse;
        }

        public async Task<BidResponse> BidAuctionAsync(BidRequest initiateBidRequest)
        {
            var bidResponse = await client.BidAuctionAsync(initiateBidRequest);

            if (bidResponse.IsSuccess)
                client.PublishBid(new BidEvent
                {
                    AuctionId = initiateBidRequest.AuctionId,
                    Amount = initiateBidRequest.Amount
                });

            return bidResponse;
        }

        public async Task<CloseAuctionResponse> CloseAuctionAsync(CloseAuctionRequest closeAuctionRequest)
        {
            var auctionResponse = await client.CloseAuctionAsync(closeAuctionRequest);

            if (auctionResponse.IsSuccess)
                client.PublishClosedAuction(new AuctionEvent
                {
                    AuctionId = auctionResponse.AuctionId,
                    ItemName = auctionResponse.ItemName,
                });

            return auctionResponse;
        }

		// TODO: explore options to reuse logic on subscribe methods
		public async Task SubscribeToInitiatedAuctions(Action<AuctionEvent> onEventReceived)
        {
            using (var call = client.SubscribeToInitiatedAuctions(new EmptyRequest(), cancellationToken: cancellationTokenSource.Token))
            {
                var responseStream = call.ResponseStream;
                while (await responseStream.MoveNext(cancellationTokenSource.Token))
                {
                    var @event = responseStream.Current;
                    onEventReceived(@event);
                }
            }
        }

		// TODO: explore options to reuse logic on subscribe methods
		public async Task SubscribeToBids(Action<BidEvent> onEventReceived)
        {
            using (var call = client.SubscribeToBids(new EmptyRequest(), cancellationToken: cancellationTokenSource.Token))
            {
                var responseStream = call.ResponseStream;
                while (await responseStream.MoveNext(cancellationTokenSource.Token))
                {
                    var @event = responseStream.Current;
                    onEventReceived(@event);
                }
            }
        }

		// TODO: explore options to reuse logic on subscribe methods
		public async Task SubscribeToClosedAuctions(Action<AuctionEvent> onEventReceived)
        {
            using (var call = client.SubscribeToClosedAuctions(new EmptyRequest(), cancellationToken: cancellationTokenSource.Token))
            {
                var responseStream = call.ResponseStream;
                while (await responseStream.MoveNext(cancellationTokenSource.Token))
                {
                    var @event = responseStream.Current;
                    onEventReceived(@event);
                }
            }
        }
    }
}

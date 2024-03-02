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

		public async Task InitiateAuctionAsync(InitiateAuctionRequest initiateAuctionRequest)
		{
			await client.InitiateAuctionAsync(initiateAuctionRequest);
			client.PublishInitiatedAuction(new AuctionEvent
			{
				AuctionId = Guid.NewGuid().ToString()
			});
		}

		public async Task BidAuctionAsync(BidRequest initiateBidRequest)
		{
			await client.BidAuctionAsync(initiateBidRequest);
			client.PublishBid(new BidEvent
			{
				AuctionId = initiateBidRequest.AuctionId,
				Price = initiateBidRequest.Amount
			});
		}

		public async Task CloseAuctionAsync(CloseAuctionRequest closeAuctionRequest)
		{
			await client.CloseAuctionAsync(closeAuctionRequest);
			client.PublishClosedAuction(new AuctionEvent
			{
				AuctionId = closeAuctionRequest.AuctionId
			});
		}

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

		//public async Task PublishEvent(Event @event)
		//{
		//	client.Publish(@event);
		//}

		public void Stop()
		{
			cancellationTokenSource.Cancel();
		}
	}
}

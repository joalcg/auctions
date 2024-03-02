using Grpc.Core;

namespace AuctionPortal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new Server
            {
                Services = { AuctionPortalService.BindService(new Business.Services.AuctionPortalService()) },
                Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) } //TODO: move host and port to configuration file
            };
            server.Start();

            Console.ReadLine();
        }
    }
}

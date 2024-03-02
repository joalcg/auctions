# Auctions Portal

## Project Mission Statement

Simple Peer-to-Peer (P2P) auction system that utilizes Remote Procedure Calls (RPC) as its primary form of communication.

## Details of the tech stack

This application was built using C# .Net 7 with RPC.

## Process of building your development environment

Application can be built by running a simple build in Visual Studio or the console.

## Dependencies

Google.Protobuf
Grpc.AspNetCore
Grpc.Core
Grpc.Tools
Microsoft.Extensions.Hosting

## Details of operation

This application allows multiple clients to connect into the same channel to create auctions, submit bids, and close auctions.

## Details of testing

Main steps:

1. Make sure you set up both AuctionPortal and AuctionPortal.Client as start up projects.
2. Run more instances of AuctionPortal.Client if you want to see multiple nodes interacting in the network.

For reference: AuctionPortal sets up the server, and the AuctionPortal.Client creates a client. That said, there should be a single instance of AuctionPortal but there can be 1 or more instances of AuctionPortal.Client.

## Work pending & future improvements

The code has some TODO comments with ideas to improve but here are more details that could not be explained in the code itself

1. **Move the host and port to a configuration file**: This is important so values can be changed easily if needed.
2. **Explore some options and ideas to reuse logic for subscribe/publish operations**: They perform different tasks but their code and logic is too similar. Reusing would be great here.
3. **Create unit tests**: Install XUnit and create unit tests for logic like the ones that decide which is the highest bid on an auction.
4. **Save state into a database**: Currently, everything is in memory which meets the requirements but the data is lost if the application is closed. Ideally, the auctions can be stored in the database instead of in an object in memory. That way, data will remain alive after restarting the application. This adds an additional complexity which is tracking the id of each client which is currently set up on the application start, that would have to be saved in a configuration file or embedded in the executable itself.
5. **Improve distributed transaction handling**: Currently, testing everything manually it is pretty much impossible to get multiple requests coming at the same time. However, in production the current program is not robust enough. A better way to ensure the operations are handled in the correct order, and that everyone gets the correct responses only is key for a good production system.

## Contact Details

This application was developed by Jose Cerdas (joalcg@gmail.com)

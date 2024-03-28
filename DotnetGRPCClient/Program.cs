using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using DotnetGRPC;

Console.WriteLine("Hello World!");

using var channel = GrpcChannel.ForAddress("https://comp1640api.azurewebsites.net");
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(
    new HelloRequest { Name = "GreeterClient" });
Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
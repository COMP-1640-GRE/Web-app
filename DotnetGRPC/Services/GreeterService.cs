using Grpc.Core;
using DotnetGRPC;

namespace DotnetGRPC.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name == "Token" ? DotnetGRPC.GlobalVariables.Database.BackupToken : request.Name
        });
    }
}

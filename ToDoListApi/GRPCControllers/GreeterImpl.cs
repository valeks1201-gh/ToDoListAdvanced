using Grpc.Core;
using Greet;
using System.Threading.Tasks;



public class GreeterImpl : Greeter.GreeterBase
{
    private readonly ILogger<GreeterImpl> _logger;
    public GreeterImpl(ILogger<GreeterImpl> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name + " " + DateTime.UtcNow.ToString()
        });
    }
}
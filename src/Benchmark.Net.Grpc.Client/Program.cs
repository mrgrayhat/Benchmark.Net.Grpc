#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

using System.Diagnostics;
using System.Net.Http.Json;

using Benchmark.Net.Grpc.Shared;
using Benchmark.Net6.Grpc.Shared;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Net.Client;

const int BenchmarkRate = 5000;
const string BaseUrl = "https://localhost:5001";
// for worker services with di
//Host.CreateDefaultBuilder(args)
//               .ConfigureServices((hostContext, services) =>
//               {
//                   //services.AddHostedService<Worker>();
//                   services.AddGrpcClient<ChatServiceGrpc.ChatServiceGrpcClient>(options =>
//                   {
//                       options.Address = new Uri(BaseUrl);
//                   });
//               }).Build().Run();

// console app
Console.WriteLine("Hello, World! Have Fun :)");
// a grpc socket channel is expensive object and must be created once / using a client factory | di, to prevent server side overflow.
// here we created once ( you may inject in real app or implement an static / use factory builder)
using GrpcChannel channel = GrpcChannel.ForAddress(BaseUrl);
// simulate high load and multiple tests continuously.
// http rest api requests are short lived, but socket communications are long lived usually. however, i didn't create http client per request, we think we have a http client for all requests. and we create grpc client like that (it's not expensive like channels)

// Ignore Test Ssl. NB: You should make this more robust by actually checking the certificate:
using var httpHandler = new HttpClientHandler();
httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

while (true)
{
    var client = new ChatServiceGrpc.ChatServiceGrpcClient(channel);

    using var httpclint = new HttpClient(httpHandler, false)
    {
        BaseAddress = new Uri(BaseUrl)
    };

    Stopwatch stopwatch = new Stopwatch();

    Console.WriteLine("Press any key to start mutiple grpc calls...");
    Console.ReadKey();
    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Watch Started Measurring...");
    stopwatch.Start();

    Console.ForegroundColor = ConsoleColor.Green;
    UnaryCallExample(client);
    Console.ResetColor();

    stopwatch.Stop();
    var grpcWatchElapsed = stopwatch.Elapsed;
    Console.WriteLine($"{DateTime.Now} Server Sync Call Example Took {stopwatch.Elapsed}");

    //Console.WriteLine("Press any key to start stream...");
    //Console.ReadKey();
    //stopwatch.Restart();
    //await ServerStreamingCallExample(client);
    //stopwatch.Stop();
    //Console.WriteLine($"{DateTime.Now} Server Streaming Example Took {stopwatch.Elapsed}");
    //await ClientStreamingCallExample(client);

    Console.WriteLine("Grpc Finished.");
    Console.WriteLine();
    Console.WriteLine("Press any key to start http rest apis...");
    Console.ReadKey();

    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Watch Started Measurring...");
    stopwatch.Restart();

    Console.ForegroundColor = ConsoleColor.Green;
    await HttpApiCallExample(httpclint);
    Console.ResetColor();

    stopwatch.Stop();
    var restWatchElapsed = stopwatch.Elapsed;
    Console.WriteLine($"{DateTime.Now} Server HTTP API Example Took {stopwatch.Elapsed}");

    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine();
    Console.WriteLine($"Sampling Result: Grpc Channel {grpcWatchElapsed} and HTTP Rest API {restWatchElapsed} for total {BenchmarkRate} for each one (+ serialize / deserialize (json and binary) in both client server, large response and logging).");
    Console.WriteLine($"Sampling Result Diff: Grpc vs Rest => {grpcWatchElapsed - restWatchElapsed} .");
    Console.WriteLine();

    Console.WriteLine("Press any key to run again (try multiple times to see more real world diffs), or Q / ctrl+c to exit...");
    Console.ResetColor();

    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Q)
        break;
}
Console.WriteLine("Shutting down");

#region local methods

async Task HttpApiCallExample(HttpClient client)
{
    try
    {
        for (int i = 0; i <= BenchmarkRate; i++)
        {
            var restResponse = await client.GetAsync("api/chats/");
            if (restResponse.IsSuccessStatusCode)
            {
                var deserialized = await restResponse.Content.ReadFromJsonAsync<MessageContract<List<ChatMessageDto>>>();
                if (deserialized != null)
                {
                    Console.WriteLine($"total {deserialized.Data.Count} messages recieved from server.");
                    //foreach (var msg in deserialized)
                    //{
                    //    Console.WriteLine($"[{msg.Id}] => {msg.User}: {msg.Message}");
                    //}
                }
                else
                {
                    Console.WriteLine($"response in null !");
                }
            }
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"http client request pip exception raised: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"http client exception raised: {ex.Message}");
    }
}
void UnaryCallExample(ChatServiceGrpc.ChatServiceGrpcClient client)
{
    try
    {
        for (int i = 0; i <= BenchmarkRate; i++)
        {
            GetMessagesGrpcResponse syncMessages = client.GetMessages(new Empty());
            if (!syncMessages.Message.IsSuccess)
                Console.WriteLine($"server message: {syncMessages.Message.Message} .");

            Console.WriteLine($"total {syncMessages.Message.Paging.Total} messages recieved from server.");
            // if you want log / use responses data
            //for (int index = 0; index < syncMessages.Data.Count; index++)
            //{
            //    var msg = syncMessages.Data[index];
            //    Console.WriteLine($"[{msg.Id}] => {msg.User}: {msg.Message}");
            //}
        }
    }
    catch (RpcException ex)
    {
        Console.WriteLine($"grpc exception raised: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"unhandled exception raised: {ex.Message}");
    }
}
async Task ServerStreamingCallExample(ChatServiceGrpc.ChatServiceGrpcClient client)
{
    try
    {
        using var asyncStreamMessages = client.GetStreamMessages(new Empty());

        await foreach (var message in asyncStreamMessages.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"[{message.Id}] => {message.User}: {message.Message}");
        }
    }
    catch (RpcException ex)
    {
        Console.WriteLine($"grpc exception raised: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"unhandled exception raised: {ex.Message}");
    }
}

#endregion
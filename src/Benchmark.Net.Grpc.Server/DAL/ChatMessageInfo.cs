namespace Benchmark.Net.Grpc.Server.DAL
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class ChatMessageInfo
    {
        public int Id { get; set; } = Random.Shared.Next(100, 100000);
        public string User { get; set; }
        public string Message { get; set; }
    }
}

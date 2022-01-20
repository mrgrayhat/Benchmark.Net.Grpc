using System.Linq;

namespace Benchmark.Net.Grpc.Server.DAL
{
    public class DbContext
    {
        public static void Seed(int count = 200)
        {
            Chats = Enumerable.Range(0, count).Select(x => new ChatMessageInfo()
            {
                Id = x,
                Message = $"Message {x}",
                User = $"User {x}"
            }).ToList();
        }

        public static List<ChatMessageInfo> Chats = new List<ChatMessageInfo>();

    }
}

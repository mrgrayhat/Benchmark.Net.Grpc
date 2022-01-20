
using Benchmark.Net.Grpc.Server.DAL;
using Benchmark.Net.Grpc.Shared;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

namespace Benchmark.Net.Grpc.Server.Services
{
    public class ChatServiceGrpc : Benchmark.Net.Grpc.Shared.ChatServiceGrpc.ChatServiceGrpcBase
    {
        private readonly ILogger<ChatServiceGrpc> _logger;


        public ChatServiceGrpc(ILogger<ChatServiceGrpc> logger)
        {
            _logger = logger;
        }

        public override Task<MessageContractGrpc> SendMessage(SendMessageGrpcRequest request, ServerCallContext context)
        {
            return Task.FromResult(new MessageContractGrpc
            {
                IsSuccess = true,
                Message = "You'r Message has been sent!"
            });
        }

        /// <summary>
        /// sync fetch and return data
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessagesGrpcResponse> GetMessages(Empty request, ServerCallContext context)
        {
            List<ChatMessageDtoGrpc>? chats = null;
            chats = DbContext.Chats
                .Select(chat => new ChatMessageDtoGrpc()
                {
                    Id = chat.Id,
                    Message = chat.Message,
                    User = chat.User
                })
                .ToList();

            var res = new GetMessagesGrpcResponse
            {
                Data = { chats },
                Message = new MessageContractGrpc()
                {
                    IsSuccess = true,
                    Paging = new PagingResultGrpc()
                    {
                        Page = 1,
                        PageSize = chats.Count,
                        Total = chats.Count
                    }
                }
            };
            return Task.FromResult(res);
        }

        /// <summary>
        /// async get and stream return
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetStreamMessages(Empty request, IServerStreamWriter<ChatMessageDtoGrpc> responseStream, ServerCallContext context)
        {
            var chats = DbContext.Chats
                           .ToList();

            for (int i = 0; i <= chats.Count; i++)
            {
                var chat = chats[i];
                await responseStream.WriteAsync(new ChatMessageDtoGrpc()
                {
                    Id = chat.Id,
                    Message = chat.Message,
                    User = chat.User
                });
                //await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
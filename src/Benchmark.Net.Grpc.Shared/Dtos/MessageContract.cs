namespace Benchmark.Net6.Grpc.Shared
{
    public class MessageContract
    {
        public MessageContract()
        {

        }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public bool IsSuccess { get; set; }

    }
    public class MessageContract<T> : MessageContract
    {
        public MessageContract()
        {

        }

        public MessageContract(T data, PagingResult paging = null)
        {
            Data = data;
            Paging = paging;
            IsSuccess = true;
        }


        public T Data { get; set; }
        public PagingResult? Paging { get; set; } = null;
    }
}

namespace Benchmark.Net6.Grpc.Shared
{
    public class PagingResult
    {
        public PagingResult()
        {

        }
        public PagingResult(long total, int page = 1, int pageSize = 10)
        {
            Page = page;
            PageSize = pageSize;
            Total = total;
        }
        public long Total { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
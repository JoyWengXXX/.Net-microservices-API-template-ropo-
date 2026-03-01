
namespace Service.Common.Models
{
    public class PagedList<T>
    {
        public IEnumerable<T> items { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public int totalPages => (int)Math.Ceiling(totalCount / (double)pageSize);
        public bool hasPreviousPage => pageIndex > 1;
        public bool hasNextPage => pageIndex < totalPages;
    }
}


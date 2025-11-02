namespace PetHotelManager.Common
{
    public class Pagination
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);

        public Pagination() { }

        public Pagination(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber <= 0 ? 1 : pageNumber;
            PageSize = pageSize <= 0 ? 10 : pageSize;
        }

        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
    }
}

using System;
using System.Collections.Generic;

namespace PetHotelManager.Pages.Invoices
{
    // ViewModel cho danh sách hóa đơn
    public class InvoiceViewModel
    {
        public int      Id           { get; set; }
        public DateTime InvoiceDate  { get; set; }
        public decimal  TotalAmount  { get; set; }
        public string   Status       { get; set; } = string.Empty;
        public string   CustomerName { get; set; } = string.Empty;
        public string   CustomerId   { get; set; } = string.Empty;
    }

    // ViewModel cho chi tiết hóa đơn
    public class InvoiceDetailViewModel
    {
        public int                       Id          { get; set; }
        public DateTime                  InvoiceDate { get; set; }
        public decimal                   TotalAmount { get; set; }
        public string                    Status      { get; set; } = string.Empty;
        public CustomerViewModel         Customer    { get; set; } = new();
        public List<ItemDetailViewModel> Details     { get; set; } = new();
    }

    public class CustomerViewModel
    {
        public string  Id    { get; set; } = string.Empty;
        public string  Name  { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class ItemDetailViewModel
    {
        public string  Description { get; set; } = string.Empty;
        public int     Quantity    { get; set; }
        public decimal UnitPrice   { get; set; }
        public decimal SubTotal    { get; set; }
        public string  ItemName    { get; set; } = string.Empty;
    }

    // Phân trang tối thiểu
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex  { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex  = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage     => PageIndex < TotalPages;
    }
}
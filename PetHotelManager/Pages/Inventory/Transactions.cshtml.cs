using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Inventory
{
    [Authorize(Roles = "Admin,Staff")]
    public class TransactionsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public TransactionsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public TransactionResponse TransactionData { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ProductId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TransactionType { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 20;

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies["ApiToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task OnGetAsync()
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            var queryParts = new List<string> 
            { 
                $"pageNumber={PageNumber}", 
                $"pageSize={PageSize}" 
            };
            
            if (ProductId.HasValue) queryParts.Add($"productId={ProductId}");
            if (!string.IsNullOrEmpty(TransactionType)) queryParts.Add($"transactionType={TransactionType}");
            if (StartDate.HasValue) queryParts.Add($"startDate={StartDate:yyyy-MM-dd}");
            if (EndDate.HasValue) queryParts.Add($"endDate={EndDate:yyyy-MM-dd}");

            var queryString = "?" + string.Join("&", queryParts);
            TransactionData = await client.GetFromJsonAsync<TransactionResponse>($"{baseUrl}/api/inventory/transactions{queryString}")
                              ?? new TransactionResponse();
        }

        public class TransactionResponse
        {
            public int TotalRecords { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public List<TransactionItem> Transactions { get; set; } = new List<TransactionItem>();
        }

        public class TransactionItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public int ChangeQuantity { get; set; }
            public string TransactionType { get; set; } = "";
            public string? ReferenceType { get; set; }
            public int? ReferenceId { get; set; }
            public DateTime TransactionDate { get; set; }
            public string? Supplier { get; set; }
            public decimal? UnitPrice { get; set; }
            public string? Notes { get; set; }
            public string? CreatedByUserName { get; set; }
        }
    }
}

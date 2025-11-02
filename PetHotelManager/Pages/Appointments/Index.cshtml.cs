using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Appointments;
using System.Text.Json;

namespace PetHotelManager.Pages.Appointments
{
    [Authorize(Roles = "Admin,Staff,Doctor")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public PaginatedAppointmentList PaginatedList { get; set; } = new PaginatedAppointmentList();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var requestUrl = $"{baseUrl}/api/appointments?pageNumber={CurrentPage}&pageSize=10";
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                requestUrl += $"&search={SearchTerm}";
            }

            var response = await client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                PaginatedList = await JsonSerializer.DeserializeAsync<PaginatedAppointmentList>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
    }

    public class PaginatedAppointmentList
    {
        public List<AppointmentListDto> Data { get; set; } = new List<AppointmentListDto>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }

    public class PaginationInfo
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
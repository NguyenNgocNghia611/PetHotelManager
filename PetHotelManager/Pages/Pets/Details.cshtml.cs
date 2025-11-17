using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetHotelManager.DTOs.Appointments;
using PetHotelManager.DTOs.MedicalRecord;
using PetHotelManager.DTOs.Pets;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetHotelManager.Pages.Pets
{
    [Authorize(Roles = "Admin,Staff,Doctor,Veterinarian")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DetailsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public PetDetailDto Pet { get; set; }
        public List<MedicalRecordDto> MedicalRecords { get; set; } = new List<MedicalRecordDto>();
        public List<AppointmentListDto> Appointments { get; set; } = new List<AppointmentListDto>();

        private HttpClient GetAuthenticatedClient()
        {
            var client = _clientFactory.CreateClient("ApiClient");
            var token = HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            if (token != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Identity.Application={token}");
            }
            return client;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = GetAuthenticatedClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var petDetailTask = client.GetFromJsonAsync<PetDetailDto>($"{baseUrl}/api/pets/{id}");
            var medicalRecordsTask = client.GetFromJsonAsync<List<MedicalRecordDto>>($"{baseUrl}/api/medicalrecords/pet/{id}");
            var appointmentsTask = client.GetFromJsonAsync<PaginatedAppointmentList>($"{baseUrl}/api/appointments?search={""}");

            await Task.WhenAll(petDetailTask, medicalRecordsTask, appointmentsTask);

            Pet = await petDetailTask;
            if (Pet == null)
            {
                return NotFound();
            }

            MedicalRecords = (await medicalRecordsTask) ?? new List<MedicalRecordDto>();

            var paginatedAppointments = await appointmentsTask;
            if (paginatedAppointments?.Data != null)
            {
                Appointments = paginatedAppointments.Data.Where(a => a.PetName == Pet.Name).ToList();
            }

            return Page();
        }

        public class PaginatedAppointmentList
        {
            public List<AppointmentListDto> Data { get; set; }
        }
    }
}
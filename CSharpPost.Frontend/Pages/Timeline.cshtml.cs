using MicrobloggingApp.Frontend.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicrobloggingApp.Frontend.Pages
{
    public class TimelineModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TimelineModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<PostDto>? Posts { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? Search { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync(int page = 1, string? search = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            CurrentPage = page;
            Search = search;
            StartDate = startDate;
            EndDate = endDate;

            var httpClient = _httpClientFactory.CreateClient("MicrobloggingAPI");
            var query = $"api/posts?page={page}&search={search}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await httpClient.GetFromJsonAsync<PaginatedResponse>(query);

            Posts = response?.Posts;
            TotalPages = response?.TotalPages ?? 0;
        }
    }
}

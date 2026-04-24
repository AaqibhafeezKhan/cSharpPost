using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CSharpPost.Frontend.DTOs;

namespace CSharpPost.Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<PostDto>? Posts { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? Search { get; set; }

    public async Task OnGetAsync(int page = 1, string? search = null, string? sortBy = "latest")
    {
        CurrentPage = page;
        Search = search;

        var httpClient = _httpClientFactory.CreateClient("MicrobloggingAPI");
        var query = $"api/posts?page={page}&search={search}&sortBy={sortBy}";
        var response = await httpClient.GetFromJsonAsync<PaginatedResponse>(query);

        Posts = response?.Posts;
        TotalPages = response?.TotalPages ?? 0;
    }

    public async Task<IActionResult> OnPostAsync(string text)
    {
        if (!string.IsNullOrEmpty(text) && text.Length <= 140)
        {
            var httpClient = _httpClientFactory.CreateClient("MicrobloggingAPI");
            
            var formData = new Dictionary<string, string>
            {
                { "text", text }
            };
            
            using var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync("api/posts", content);
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(new { page = CurrentPage, search = Search });
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var httpClient = _httpClientFactory.CreateClient("MicrobloggingAPI");
        var response = await httpClient.DeleteAsync($"api/posts/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage(new { page = CurrentPage, search = Search });
        }

        return Page();
    }
}

public class PaginatedResponse
{
    public List<PostDto>? Posts { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

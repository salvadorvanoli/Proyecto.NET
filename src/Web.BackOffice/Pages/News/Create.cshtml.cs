using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.BackOffice.Models;
using Web.BackOffice.Services;

namespace Web.BackOffice.Pages.News;

public class CreateModel : PageModel
{
    private readonly INewsApiService _newsApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(INewsApiService newsApiService, ILogger<CreateModel> logger)
    {
        _newsApiService = newsApiService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(5000, ErrorMessage = "El contenido no puede exceder 5000 caracteres")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de publicación es requerida")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string? ImageUrl { get; set; }
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var createNewsDto = new CreateNewsDto
            {
                Title = Input.Title,
                Content = Input.Content,
                PublishDate = Input.PublishDate,
                ImageUrl = Input.ImageUrl
            };

            var createdNews = await _newsApiService.CreateNewsAsync(createNewsDto);

            TempData["SuccessMessage"] = $"Noticia '{createdNews.Title}' creada exitosamente.";
            return RedirectToPage("/News/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            ErrorMessage = "Error al crear la noticia. Por favor, intente nuevamente.";
            return Page();
        }
    }
}


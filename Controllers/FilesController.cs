using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v{version:apiVersion}/files")]
public class FilesController : ControllerBase
{
  private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

  public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
  {
    _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
      ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
  }

  [HttpGet("{fileId}")]
  [ApiVersion(0.1, Deprecated = true)]
  public IActionResult GetFile(string fileId)
  {
    // FileContentResult
    // FileStreamResult
    // PhysicalFileResult
    // VirtualFileResult
    string pathToFile = "some-file.pdf";
    if (!System.IO.File.Exists(pathToFile))
    {
      return NotFound();
    }

    var bytes = System.IO.File.ReadAllBytes(pathToFile);
    return File(bytes, "text/plain", Path.GetFileName(pathToFile));
  }

  [HttpPost]
  public async Task<ActionResult> CreateFile(IFormFile file)
  {
    if (file.Length == 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
    {
      return BadRequest("No file or an invalid one has been inputted");
    }

    // Create the file path. Avoid using the file.FileName, as an attacker can provide a
    // malicious one, including full paths or relative paths.
    var path = Path.Combine(
      Directory.GetCurrentDirectory(),
      $"uploaded_file_{Guid.NewGuid()}.pdf");

    using (var stream = new FileStream(path, FileMode.Create))
    {
      await file.CopyToAsync(stream);
    }

    return Ok("Your file has been uploaded successfully");
  }
}
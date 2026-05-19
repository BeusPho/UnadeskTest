using Microsoft.AspNetCore.Mvc;
using UnadeskTestApiGateway.Producers;
using UnadeskTestCommon.Entities;
using UnadeskTestCommon.Repository;

namespace UnadeskTestApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class PdfController(MqLoadProducer loadProducer, PdfRepository repository, MqGetContentRequestResponse contentRequestResponse) : ControllerBase
{
    [HttpPost("/loadPdf")]
    public async Task<IActionResult> LoadPdf(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Файл не выбран");
        }

        if (Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return BadRequest("Только PDF файлы");
        }

        byte[] fileContent;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileContent = memoryStream.ToArray();
        }

        var id = await repository.InitialWritePdfAsync(new PdfRecord
        {
            Name = file.Name,
            Status = "New",
            UploadDate = DateTime.UtcNow,
        });

        if (id is null)
        {
            return BadRequest("Failed to write");
        }

        await loadProducer.LoadPdfAsync(id.Value, file.FileName, fileContent);

        return Accepted();
    }

    [HttpGet("/getList")]
    public async Task<IActionResult> GetList()
    {
        return Ok(await repository.GetListAsync());
    }

    [HttpGet("/getContent")]
    public async Task<IActionResult> GetContent(int id)
    {
        return Ok(await repository.GetPdfTextContentByIdAsync(id));
    }

    [HttpGet("/getContentByMq")]
    public async Task<IActionResult> GetContentByMq(int id)
    {
        try
        {
            return Ok(await contentRequestResponse.GetPdfContentAsync(id));
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status408RequestTimeout);
        }
    }
}

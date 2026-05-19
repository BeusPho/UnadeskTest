using UglyToad.PdfPig;
using UnadeskTestCommon.Entities;
using UnadeskTestCommon.Repository;

namespace UnadeskTestApp.Services;

public class PdfProcessingService(PdfRepository repository)
{
    public async Task ProcessAsync(MqLoadPdfDto record)
    {
        string text;
        using (PdfDocument document = PdfDocument.Open(record.FileContent))
        {
            text = string.Join(Environment.NewLine, document.GetPages().Select(p => p.Text));
        }

        var entity = await repository.GetRecordByIdAsync(record.Id);
        if (entity is null)
        {
            return;
        }

        entity.Status = "Updated";
        entity.TextContent = text;

        await repository.UpdatePdfAsync(entity);
    }
}

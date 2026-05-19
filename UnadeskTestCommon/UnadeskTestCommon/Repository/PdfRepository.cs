using Microsoft.EntityFrameworkCore;
using UnadeskTestCommon.Entities;
using UnadeskTestCommon.Infrastructure;

namespace UnadeskTestCommon.Repository;

public class PdfRepository(ApplicationContext context)
{
    public async Task<int?> InitialWritePdfAsync(PdfRecord record)
    {
        try
        {
            var entity = await context.Records.AddAsync(record);

            await context.SaveChangesAsync();
            return entity.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return null;
        }
    }

    public async Task UpdatePdfAsync(PdfRecord record)
    {
        context.Update(record);

        await context.SaveChangesAsync();
    }

    public async Task<string?> GetPdfTextContentByIdAsync(int id)
    {
        return (await context.Records.SingleOrDefaultAsync(context => context.Id == id))?.TextContent;
    }

    public async Task<PdfRecord?> GetRecordByIdAsync(int id)
    {
        return await context.Records.SingleOrDefaultAsync(context => context.Id == id);
    }

    public async Task<List<PdfRecord>> GetListAsync()
    {
        return await context.Records.ToListAsync();
    }
}
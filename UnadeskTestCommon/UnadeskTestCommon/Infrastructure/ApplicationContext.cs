using Microsoft.EntityFrameworkCore;
using UnadeskTestCommon.Entities;

namespace UnadeskTestCommon.Infrastructure;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<PdfRecord> Records => Set<PdfRecord>();
}

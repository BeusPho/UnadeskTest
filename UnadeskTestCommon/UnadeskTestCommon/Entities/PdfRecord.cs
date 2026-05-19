namespace UnadeskTestCommon.Entities;

public record PdfRecord
{
    public int Id { get; set; }
    public required string Name { get; init; }
    public DateTime UploadDate { get; init; }
    public required string Status { get; set; }
    public string? TextContent { get; set; }
}

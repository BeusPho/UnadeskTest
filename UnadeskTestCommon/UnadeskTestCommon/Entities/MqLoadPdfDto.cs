namespace UnadeskTestCommon.Entities;

public class MqLoadPdfDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required byte[] FileContent { get; set; }
}
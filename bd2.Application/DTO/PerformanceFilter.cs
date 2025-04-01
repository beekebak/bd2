namespace bd2.Application.DTO;

public class PerformanceFilter
{
    public DateTime? StartDateTimeFrom { get; set; }
    public DateTime? StartDateTimeTo { get; set; }
    public string? OriginName { get; set; }
    public string? ComposerName { get; set; }
    public string? WriterName { get; set; }
    public string? StagingComposerName { get; set; }
    public string? DirectorName { get; set; }
    public string? ArtistName { get; set; }
}
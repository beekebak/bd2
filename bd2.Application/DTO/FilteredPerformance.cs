namespace bd2.Application.DTO;

public class FilteredPerformance
{
    public int Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public string OriginName { get; set; }
    public string? ComposerName { get; set; }
    public string? WriterName { get; set; }
    public string? StagingComposerName { get; set; }
    public string? DirectorName { get; set; }
    public List<string> ArtistNames { get; set; }
}
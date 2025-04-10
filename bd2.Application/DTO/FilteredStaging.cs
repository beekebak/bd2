namespace bd2.Application.DTO;

public class FilteredStaging
{
    public int Id { get; set; }
    public string OriginName { get; set; }
    public string ComposerOriginName { get; set; }
    public string WriterName { get; set; }
    public string ComposerName { get; set; }
    public string DirectorName { get; set; }
    public TimeSpan Duration { get; set; }
}
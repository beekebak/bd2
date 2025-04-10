namespace bd2.Core.StagingAggregate;

public record Author(int Id, string AuthorName, List<AuthorSpecialty> Specialties);
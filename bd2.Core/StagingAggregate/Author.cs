namespace bd2.Core.StagingAggregate;

public record Author(int id, string AuthorName, List<AuthorSpecialty> Specialties);
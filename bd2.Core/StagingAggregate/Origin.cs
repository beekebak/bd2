namespace bd2.Core.StagingAggregate;

public record Origin(TimeOnly Duration, Author Writer, Author OriginComposer);
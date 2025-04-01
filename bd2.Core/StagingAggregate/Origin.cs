namespace bd2.Core.StagingAggregate;

public record Origin(int OriginId, string OriginName, Author Writer, Author OriginComposer);
namespace bd2.Core.StagingAggregate;

public class Staging(
    int id,
    TimeOnly duration,
    int directorId,
    int stagingComposerId,
    Origin origin,
    List<Role> roles,
    Dictionary<int, int> inventory)
{
    public int Id { get; private set; } = id;
    public TimeOnly Duration { get; private set; } = duration;
    public int DirectorId { get; private set; } = directorId;
    public int StagingComposerId { get; private set; } = stagingComposerId;
    public Origin Origin { get; private set; } = origin;
    public List<Role> Roles { get; private set; } = roles;

    public Dictionary<int, int> Inventory { get; private set; } = inventory;
}
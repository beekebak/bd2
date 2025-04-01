namespace bd2.Core.StagingAggregate;

public class Staging(
    int id,
    TimeOnly duration,
    Worker.Worker director, 
    Worker.Worker stagingComposer,
    int originId,
    List<Role> roles,
    Dictionary<Inventory, int> inventory)
{
    public int Id { get; private set; } = id;
    public TimeOnly Duration { get; private set; } = duration;
    public Worker.Worker Director { get; private set; } = director;
    public Worker.Worker StagingComposer { get; private set; } = stagingComposer;
    public int OriginId { get; private set; } = originId;
    public List<Role> Roles { get; private set; } = roles;
    public Dictionary<Inventory, int> Inventory { get; private set; } = inventory;
}
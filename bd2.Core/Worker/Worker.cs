namespace bd2.Core.Worker;

public class Worker(int id, string name, string specialty)
{
    public int Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public string Specialty { get; private set; } = specialty;
}
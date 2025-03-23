namespace bd2.Core.Worker;

public class Artist(int id, string name, string specialty, string grade) : Worker(id, name, specialty)
{
    public string Grade { get; private set; } = grade;
}
namespace DNET.Backend.Api.Models;

public class Table
{
    public Table() { }
    
    public Table(int id, int capacity)
    {
        Id = id;
        Capacity = capacity;
    }
    
    public int Id { get; set; }
    public int Capacity { get; set; }
}

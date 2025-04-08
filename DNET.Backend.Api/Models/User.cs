using DNET.Backend.DataAccess.Domain;

namespace DNET.Backend.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = String.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; }
    
    public User() { }
    
    public User(UserEntity user)
    {
        Id = user.Id;
        Username = user.Username;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Role = user.Role;
    }
}
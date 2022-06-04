namespace P2.Model;

public enum Role
{
    Customer,
    Manager
}

public class User : Entity
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Role Role { get; set; }

    public string FullName => FirstName + " " + LastName;
}
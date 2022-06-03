namespace P2.Model;

public class User : Entity
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string FullName => FirstName + " " + LastName;
}
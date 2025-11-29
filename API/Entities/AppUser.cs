namespace API.Entities;

// AppUser Entity for EF

// required keyword is to ensure that a property must be supplied 
// meaning it cant be null
// as strings are a reference type - variable contains a reference to memory

public class AppUser
{
    public string Id {get; set;} = Guid.NewGuid().ToString();
    public required string? DisplayName {get; set;}
    public required string Email {get; set;}

}

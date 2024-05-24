namespace CheckMade.Common.Tests.Unit.MonadicWrappers.Composition;

internal class User
{
    internal string? Username { get; init; }
    internal string? Email { get; init; }
    internal string? Password { get; init; }
}

internal class RegisteredUser
{
    public int Id { get; set; }
    public string? Username { get; set; }
}

internal static class UserService
{
    public static Attempt<User> CreateUser(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Attempt<User>.Fail(new ArgumentException("Invalid user details"));
        }
        return Attempt<User>.Succeed(new User { Username = username, Email = email, Password = password });
    }

    public static Validation<User> ValidateUser(User user)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            errors.Add("Username is required");
        }
        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
        {
            errors.Add("Valid email is required");
        }
        if (user.Password?.Length < 6)
        {
            errors.Add("Password must be at least 6 characters long");
        }

        return errors.Count == 0 ? Validation<User>.Valid(user) : Validation<User>.Invalid(errors);
    }

    public static async Task<Result<RegisteredUser>> RegisterUserAsync(User user)
    {
        await Task.Delay(50); // Simulate async work
        
        return user.Username == "existingUser" 
            ? Result<RegisteredUser>.FromError(Ui("Username already exists")) 
            : Result<RegisteredUser>.FromSuccess(new RegisteredUser { Id = new Random().Next(1, 1000), Username = user.Username });
    }

    public static Option<int> GenerateWelcomeEmail(RegisteredUser? user)
    {
        return user == null 
            ? Option<int>.None() 
            : Option<int>.Some(new Random().Next(1000, 9999)); // Simulate email generation
    }
}

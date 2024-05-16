using FluentAssertions;

namespace CheckMade.Common.Tests.MonadicWrappers.Composition;

public class MonadicWrappersTest
{
    [Fact]
    public async Task NestedMatch_CanComposeVariousMonads_ToGenerateWelcomeEmail_ForValidUserData()
    {
        const string username = "newUser";
        const string email = "newUser@example.com";
        const string password = "password";

        var userAttempt = UserService.CreateUser(username, email, password);

        await userAttempt.Match(
            async user =>
            {
                var userValidation = UserService.ValidateUser(user);

                await userValidation.Match(
                    async validUser =>
                    {
                        var registerUserResult = await UserService.RegisterUserAsync(validUser);
                        registerUserResult.Success.Should().BeTrue();

                        var welcomeEmail = UserService.GenerateWelcomeEmail(registerUserResult.GetValueOrDefault());
                        welcomeEmail.IsSome.Should().BeTrue();
                    },
                    errors => Task.FromException(new Exception(errors[0]))
                    );
            },
            exception => Task.FromException(new Exception("User creation failed", exception)));
    }
    
    [Fact]
    public async Task NestedMatch_CanComposeVariousMonads_ToFailWelcomeEmail_ForInvalidUserData()
    {
        const string username = "newUser";
        const string email = "newUser@example.com";
        const string password = "";

        var userAttempt = UserService.CreateUser(username, email, password);

        var act = async () =>
        {
            await userAttempt.Match(
                async user =>
                {
                    var userValidation = UserService.ValidateUser(user);

                    await userValidation.Match(
                        async validUser =>
                        {
                            var registerUserResult = await UserService.RegisterUserAsync(validUser);
                            registerUserResult.Success.Should().BeTrue();

                            var welcomeEmail = UserService.GenerateWelcomeEmail(registerUserResult.GetValueOrDefault());
                            welcomeEmail.IsSome.Should().BeTrue();
                        },
                        errors => Task.FromException(new Exception(errors[0]))
                    );
                },
                exception => Task.FromException(new Exception("User creation failed", exception)));
        };

        await act.Should().ThrowAsync<Exception>().WithMessage("User creation failed");
    }

    [Fact]
    public async Task Imperative_CanComposeVariousMonads_ToGenerateWelcomeEmail_ForValidUserData()
    {
        const string username = "newUser";
        const string email = "newUser@example.com";
        const string password = "password";

        var userAttempt = UserService.CreateUser(username, email, password);
    
        if (userAttempt.IsSuccess)
        {
            var userValidation = UserService.ValidateUser(userAttempt.Value!);
            if (userValidation.IsValid)
            {
                var registerUserResult = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserResult.Success.Should().BeTrue();

                var welcomeEmail = UserService.GenerateWelcomeEmail(registerUserResult.GetValueOrDefault());
                welcomeEmail.IsSome.Should().BeTrue();
            }
            else
            {
                Assert.Fail("User validation failed");
            }
        }
        else
        {
            Assert.Fail("User creation failed");
        }
    }
    
    [Fact]
    public async Task Imperative_CanComposeVariousMonads_ToFailWelcomeEmail_ForInvalidUserData()
    {
        const string username = "newUser";
        const string email = "newUser@example.com";
        const string password = "";

        var userAttempt = UserService.CreateUser(username, email, password);

        var act = async () =>
        {
            if (userAttempt.IsSuccess)
            {
                var userValidation = UserService.ValidateUser(userAttempt.Value!);
                if (userValidation.IsValid)
                {
                    var registerUserResult = await UserService.RegisterUserAsync(userValidation.Value!);
                    registerUserResult.Success.Should().BeFalse();
                }
                else
                {
                    throw new Exception(userValidation.Errors[0]);
                }
            }
            else
            {
                throw new Exception("User creation failed");
            }
        };

        await act.Should().ThrowAsync<Exception>().WithMessage("User creation failed");
    }
    
    [Fact]
    public async Task UserValidation_GeneratesTwoErrors_ForInvalidEmailAndPsw()
    {
        const string username = "newUser";
        const string email = "no-ApfelStrudel";
        const string password = "short";

        var userAttempt = UserService.CreateUser(username, email, password);
        
        if (userAttempt.IsSuccess)
        {
            var userValidation = UserService.ValidateUser(userAttempt.Value!);
            if (userValidation.IsValid)
            {
                var registerUserResult = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserResult.Success.Should().BeFalse();
            }
            else
            {
                userValidation.Errors.Count.Should().Be(2);
            }
        }
        else
        {
            throw new Exception("User creation failed");
        }
    }
    
    [Fact]
    public async Task UserValidation_GeneratesErrorMessageInCorrectOrder_ForInvalidEmailAndPsw()
    {
        const string username = "newUser";
        const string email = "no-ApfelStrudel";
        const string password = "short";

        var userAttempt = UserService.CreateUser(username, email, password);
        
        if (userAttempt.IsSuccess)
        {
            var userValidation = UserService.ValidateUser(userAttempt.Value!);
            if (userValidation.IsValid)
            {
                var registerUserResult = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserResult.Success.Should().BeFalse();
            }
            else
            {
                userValidation.Errors[0].Should().Be("Valid email is required");
                userValidation.Errors[1].Should().Be("Password must be at least 6 characters long");
            }
        }
        else
        {
            throw new Exception("User creation failed");
        }
    }
}

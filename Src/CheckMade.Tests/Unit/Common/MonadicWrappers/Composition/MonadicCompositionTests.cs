using FluentAssertions;

namespace CheckMade.Tests.Unit.Common.MonadicWrappers.Composition;

public class MonadicCompositionTests
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
                        var registerUserAttempt = await UserService.RegisterUserAsync(validUser);
                        registerUserAttempt.IsSuccess.Should().BeTrue();

                        var welcomeEmail = UserService
                            .GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
                        welcomeEmail.IsSome.Should().BeTrue();
                    },
                    errors => Task.FromException(new Exception(errors[0].RawEnglishText))
                    );
            },
            failure => Task.FromException(new Exception("User creation failed", failure.Exception)));
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
                            var registerUserAttempt = await UserService.RegisterUserAsync(validUser);
                            registerUserAttempt.IsSuccess.Should().BeTrue();

                            var welcomeEmail = UserService
                                .GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
                            welcomeEmail.IsSome.Should().BeTrue();
                        },
                        errors => Task
                            .FromException(new Exception(errors[0].RawEnglishText))
                    );
                },
                failure => Task.FromException(new Exception("User creation failed", failure.Exception)));
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
                var registerUserAttempt = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserAttempt.IsSuccess.Should().BeTrue();

                var welcomeEmail = UserService.GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
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

        var errorMessage = string.Empty;
        
        if (userAttempt.IsSuccess)
        {
            var userValidation = UserService.ValidateUser(userAttempt.Value!);
            if (userValidation.IsValid)
            {
                var registerUserAttempt = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserAttempt.IsSuccess.Should().BeFalse();
            }
            else
            {
                errorMessage = userValidation.Errors[0].RawEnglishText;
            }
        }
        else
        {
            errorMessage = "User creation failed";
        }

        errorMessage.Should().Be("User creation failed");
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
                var registerUserAttempt = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserAttempt.IsSuccess.Should().BeFalse();
            }
            else
            {
                userValidation.Errors.Count.Should().Be(2);
            }
        }
        else
        {
            throw new Exception("User creation failed - but this test shouldn't visit this line ever");
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
                var registerUserAttempt = await UserService.RegisterUserAsync(userValidation.Value!);
                registerUserAttempt.IsSuccess.Should().BeFalse();
            }
            else
            {
                userValidation.Errors[0].GetFormattedEnglish().Should().Be("Valid email is required");
                userValidation.Errors[1].GetFormattedEnglish().Should().Be(
                    "Password must be at least 6 characters long");
            }
        }
        else
        {
            throw new Exception("User creation failed - but this test shouldn't visit this line ever");
        }
    }
}

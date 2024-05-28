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
                        Assert.True(registerUserAttempt.IsSuccess);

                        var welcomeEmail = UserService
                            .GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
                        Assert.True(welcomeEmail.IsSome);
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
                            Assert.True(registerUserAttempt.IsSuccess);

                            var welcomeEmail = UserService
                                .GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
                            Assert.True(welcomeEmail.IsSome);
                        },
                        errors => Task
                            .FromException(new Exception(errors[0].RawEnglishText))
                    );
                },
                failure => Task.FromException(new Exception("User creation failed", failure.Exception)));
        };

        var ex = await Assert.ThrowsAsync<Exception>(act);
        Assert.Equal("User creation failed", ex.Message);
        // await act.Should().ThrowAsync<Exception>().WithMessage("User creation failed");
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
                Assert.True(registerUserAttempt.IsSuccess);

                var welcomeEmail = UserService.GenerateWelcomeEmail(registerUserAttempt.GetValueOrDefault());
                Assert.True(welcomeEmail.IsSome);
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
                Assert.True(registerUserAttempt.IsFailure);
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

        Assert.Equal("User creation failed", errorMessage);
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
                Assert.True(registerUserAttempt.IsFailure);
            }
            else
            {
                Assert.Equal(2, userValidation.Errors.Count);
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
                Assert.True(registerUserAttempt.IsFailure);
            }
            else
            {
                Assert.Equal(
                    "Valid email is required",
                    userValidation.Errors[0].GetFormattedEnglish());
                Assert.Equal(
                    "Password must be at least 6 characters long",
                    userValidation.Errors[1].GetFormattedEnglish());
            }
        }
        else
        {
            throw new Exception("User creation failed - but this test shouldn't visit this line ever");
        }
    }
}

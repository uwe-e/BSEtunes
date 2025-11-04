// Plan (pseudocode):
// 1. Create mocks for UserManager<ApplicationUser> and IRefreshTokenRepository.
// 2. Provide an IConfiguration with Jwt settings.
// 3. Use reflection to call the private static CreateAndSaveTokensAsync method on IdentityApiEndpointRouteBuilderExtensions.
// 4. Await the returned Task and extract the private TokenBuildResult via reflection.
// 5. Assert expected outcomes:
//    - Successful case: IsProblem == false, TokenDto contains non-empty access and refresh tokens, SaveAsync was called and saved refresh token has correct UserId.
//    - Error case (missing Jwt:Key): IsProblem == true and ProblemDetail matches the configured message.
// 6. Keep tests minimal and focused, using xUnit and Moq.

using BSEtunes.Identity.Extensions;
using BSEtunes.Identity.Models;
using BSEtunes.Identity.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Reflection;

namespace BSEtunes.Identity.Tests
{
    public class IdentityApiLoginTests
    {
        [Fact]
        public async Task Login_CreatesTokensAndSavesRefreshToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                storeMock.Object, null, null, null, null, null, null, null, null);

            userManagerMock
                .Setup(um => um.FindByNameAsync(user.Email))
                .ReturnsAsync(user);

            userManagerMock
                .Setup(um => um.CheckPasswordAsync(user, "P@ssw0rd"))
                .ReturnsAsync(true);

            userManagerMock
                .Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            RefreshToken? savedToken = null;
            var tokenRepoMock = new Mock<IRefreshTokenRepository>();
            tokenRepoMock
                .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => savedToken = t)
                .Returns(Task.CompletedTask);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "ThisIsATestJwtKeyForUnitTests-ReplaceInProd",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                })
                .Build();

            // Act: call private static CreateAndSaveTokensAsync via reflection
            var method = typeof(IdentityApiEndpointRouteBuilderExtensions)
                .GetMethod("CreateAndSaveTokensAsync", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(method);

            var task = (Task)method!.Invoke(null, new object?[] { user, userManagerMock.Object, tokenRepoMock.Object, config })!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(resultProperty);

            var tokenBuildResult = resultProperty!.GetValue(task);
            Assert.NotNull(tokenBuildResult);

            var resultType = tokenBuildResult.GetType();
            var isProblem = (bool)resultType.GetProperty("IsProblem")!.GetValue(tokenBuildResult)!;
            Assert.False(isProblem);

            var tokenDto = resultType.GetProperty("TokenDto")!.GetValue(tokenBuildResult);
            Assert.NotNull(tokenDto);

            var dtoType = tokenDto.GetType();
            var accessToken = (string)dtoType.GetProperty("AccessToken")!.GetValue(tokenDto)!;
            var refreshToken = (string)dtoType.GetProperty("RefreshToken")!.GetValue(tokenDto)!;
            var expires = (int)dtoType.GetProperty("Expires")!.GetValue(tokenDto)!;

            Assert.False(string.IsNullOrWhiteSpace(accessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshToken));
            Assert.True(expires > 0);

            // Verify refresh token was saved and associated with the correct user
            tokenRepoMock.Verify(r => r.SaveAsync(It.IsAny<RefreshToken>()), Times.Once);
            Assert.NotNull(savedToken);
            Assert.Equal(user.Id, savedToken!.UserId);
            Assert.False(savedToken.IsRevoked);
            Assert.True(savedToken.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task CreateTokens_WithoutJwtKey_ReturnsProblem()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test2@example.com",
                Email = "test2@example.com"
            };

            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                storeMock.Object, null, null, null, null, null, null, null, null);

            userManagerMock
                .Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var tokenRepoMock = new Mock<IRefreshTokenRepository>();

            // Missing Jwt:Key
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var method = typeof(IdentityApiEndpointRouteBuilderExtensions)
                .GetMethod("CreateAndSaveTokensAsync", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(method);

            var task = (Task)method!.Invoke(null, new object?[] { user, userManagerMock.Object, tokenRepoMock.Object, config })!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(resultProperty);

            var tokenBuildResult = resultProperty!.GetValue(task);
            Assert.NotNull(tokenBuildResult);

            var resultType = tokenBuildResult.GetType();
            var isProblem = (bool)resultType.GetProperty("IsProblem")!.GetValue(tokenBuildResult)!;
            var problemDetail = (string?)resultType.GetProperty("ProblemDetail")!.GetValue(tokenBuildResult);

            Assert.True(isProblem);
            Assert.Equal("JWT key is not configured.", problemDetail);
        }
    }
}
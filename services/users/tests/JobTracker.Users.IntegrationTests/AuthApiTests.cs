using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.Users.IntegrationTests;

/// <summary>
/// End-to-end auth tests over real HTTP against the API backed by a Testcontainers PostgreSQL.
/// Exercises register/login and the JWT-protected /me, plus the failure paths. The table is
/// reset before each test for isolation.
/// </summary>
public sealed class AuthApiTests : IClassFixture<UsersApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly UsersApiFactory _factory;
    private readonly HttpClient _client;

    public AuthApiTests(UsersApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private sealed record UserDto(Guid Id, string Email, string DisplayName, DateTimeOffset CreatedAt);

    private sealed record LoginDto(string Token, DateTimeOffset ExpiresAt);

    private static object RegisterPayload(string email = "ada@example.com") => new
    {
        email,
        password = "Sup3rSecret!",
        displayName = "Ada Lovelace"
    };

    [Fact]
    public async Task Register_returns_201_and_normalizes_email()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload("Ada@Example.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        Assert.Equal("ada@example.com", user!.Email);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_409()
    {
        (await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload())).EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload());

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_invalid_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new { email = "bad", password = "short", displayName = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("Email", out _));
    }

    [Fact]
    public async Task Login_returns_token()
    {
        (await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload())).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "ada@example.com", password = "Sup3rSecret!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginDto>(Json);
        Assert.False(string.IsNullOrWhiteSpace(login!.Token));
    }

    [Fact]
    public async Task Login_wrong_password_returns_401()
    {
        (await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload())).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email = "ada@example.com", password = "wrongpass" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_token_returns_current_user()
    {
        var token = await RegisterAndLoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        Assert.Equal("ada@example.com", me!.Email);
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> RegisterAndLoginAsync(string email = "ada@example.com")
    {
        (await _client.PostAsJsonAsync("/api/auth/register", RegisterPayload(email))).EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login", new { email, password = "Sup3rSecret!" });
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginDto>(Json);
        return login!.Token;
    }
}

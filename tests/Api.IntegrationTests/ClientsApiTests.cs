using System.Net;
using System.Net.Http.Json;
using Api.DTOs;
using Clients.Core.Entities;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class ClientsApiTests : IntegrationTestBase
{
    public ClientsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClients_ReturnsEmptyList_WhenNoClientsExist()
    {
        var response = await Client.GetAsync("/api/v1/clients");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ClientDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateClient_ReturnsCreatedClient()
    {
        var request = new CreateClientRequest
        {
            Name = "Test Client",
            Code = "TEST001",
            ContactName = "John Doe",
            ContactEmail = "john@test.com",
        };

        var response = await Client.PostAsJsonAsync("/api/v1/clients", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("Test Client");
        result.Data.Code.Should().Be("TEST001");
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetClient_ReturnsClient_WhenExists()
    {
        var client = new Client
        {
            Name = "Test Client",
            Code = "GET001",
            ContactEmail = "test@test.com"
        };
        DbContext.Clients.Add(client);
        await DbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/v1/clients/{client.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(client.Id);
        result.Data.Code.Should().Be("GET001");
    }

    [Fact]
    public async Task GetClient_ReturnsNotFound_WhenNotExists()
    {
        var response = await Client.GetAsync($"/api/v1/clients/{Guid.NewGuid()}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateClient_UpdatesAndReturnsClient()
    {
        var client = new Client
        {
            Name = "Original Name",
            Code = "UPD001",
        };
        DbContext.Clients.Add(client);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateClientRequest
        {
            Name = "Updated Name",
            Code = "UPD001",
            ContactName = "Updated Contact",
        };

        var response = await Client.PutAsJsonAsync($"/api/v1/clients/{client.Id}", updateRequest);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClientDto>>();
        result.Should().NotBeNull();
        result!.Data.Name.Should().Be("Updated Name");
        result.Data.ContactName.Should().Be("Updated Contact");
    }

    [Fact]
    public async Task DeleteClient_RemovesClient()
    {
        var client = new Client
        {
            Name = "To Delete",
            Code = "DEL001",
        };
        DbContext.Clients.Add(client);
        await DbContext.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/v1/clients/{client.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/clients/{client.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateClient_ReturnsBadRequest_WhenInvalid()
    {
        var request = new CreateClientRequest
        {
            Name = "",
            Code = "",
        };

        var response = await Client.PostAsJsonAsync("/api/v1/clients", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

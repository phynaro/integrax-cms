using System.Net;
using System.Net.Http.Json;
using Api.DTOs;
using Clients.Core.Entities;
using Portfolios.Core.Entities;
using Portfolios.Core.Enums;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class PortfoliosApiTests : IntegrationTestBase
{
    public PortfoliosApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Client> CreateTestClient()
    {
        var client = new Client
        {
            Name = "Test Client",
            Code = $"TC{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
        };
        DbContext.Clients.Add(client);
        await DbContext.SaveChangesAsync();
        return client;
    }

    [Fact]
    public async Task GetPortfolios_ReturnsEmptyList_WhenNoPortfoliosExist()
    {
        var response = await Client.GetAsync("/api/v1/portfolios");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PortfolioListDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePortfolio_ReturnsCreatedPortfolio()
    {
        var client = await CreateTestClient();
        
        var request = new CreatePortfolioRequest
        {
            Name = "Test Portfolio",
            Code = "TP001",
            ClientId = client.Id,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/portfolios", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PortfolioDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("Test Portfolio");
        result.Data.Code.Should().Be("TP001");
        result.Data.ClientId.Should().Be(client.Id);
        result.Data.Status.Should().Be(PortfolioStatus.Draft);
    }

    [Fact]
    public async Task GetPortfolio_ReturnsPortfolio_WhenExists()
    {
        var client = await CreateTestClient();
        var portfolio = new Portfolio
        {
            Name = "Test Portfolio",
            Code = "GP001",
            ClientId = client.Id,
            Status = PortfolioStatus.Active,
        };
        DbContext.Portfolios.Add(portfolio);
        await DbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/v1/portfolios/{portfolio.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PortfolioDto>>();
        result.Should().NotBeNull();
        result!.Data.Id.Should().Be(portfolio.Id);
        result.Data.Code.Should().Be("GP001");
        result.Data.Status.Should().Be(PortfolioStatus.Active);
    }

    [Fact]
    public async Task GetPortfolio_ReturnsNotFound_WhenNotExists()
    {
        var response = await Client.GetAsync($"/api/v1/portfolios/{Guid.NewGuid()}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePortfolioStatus_UpdatesStatus()
    {
        var client = await CreateTestClient();
        var portfolio = new Portfolio
        {
            Name = "Status Test",
            Code = "ST001",
            ClientId = client.Id,
            Status = PortfolioStatus.Draft,
        };
        DbContext.Portfolios.Add(portfolio);
        await DbContext.SaveChangesAsync();

        var request = new ChangePortfolioStatusRequest { Status = PortfolioStatus.Active };
        var response = await Client.PutAsJsonAsync($"/api/v1/portfolios/{portfolio.Id}/status", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PortfolioDto>>();
        result.Should().NotBeNull();
        result!.Data.Status.Should().Be(PortfolioStatus.Active);
    }

    [Fact]
    public async Task DeletePortfolio_RemovesPortfolio()
    {
        var client = await CreateTestClient();
        var portfolio = new Portfolio
        {
            Name = "To Delete",
            Code = "DEL001",
            ClientId = client.Id,
        };
        DbContext.Portfolios.Add(portfolio);
        await DbContext.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/v1/portfolios/{portfolio.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/portfolios/{portfolio.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClientPortfolios_ReturnsPortfoliosForClient()
    {
        var client = await CreateTestClient();
        var portfolio1 = new Portfolio { Name = "P1", Code = "P001", ClientId = client.Id };
        var portfolio2 = new Portfolio { Name = "P2", Code = "P002", ClientId = client.Id };
        DbContext.Portfolios.AddRange(portfolio1, portfolio2);
        await DbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/v1/clients/{client.Id}/portfolios");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<PortfolioListDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
    }
}

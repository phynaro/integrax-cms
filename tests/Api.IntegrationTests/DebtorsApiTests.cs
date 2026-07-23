using System.Net;
using System.Net.Http.Json;
using Api.DTOs;
using Debtors.Core.Entities;
using Debtors.Core.Enums;
using FluentAssertions;
using Xunit;

namespace Api.IntegrationTests;

public class DebtorsApiTests : IntegrationTestBase
{
    public DebtorsApiTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDebtors_ReturnsEmptyList_WhenNoDebtorsExist()
    {
        var response = await Client.GetAsync("/api/v1/debtors");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<DebtorListDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDebtor_ReturnsCreatedDebtor_ForIndividual()
    {
        var request = new CreateDebtorRequest
        {
            ExternalId = "EXT-001",
            DebtorType = DebtorType.Individual,
            FirstName = "John",
            LastName = "Doe",
            TaxId = "123-45-6789",
        };

        var response = await Client.PostAsJsonAsync("/api/v1/debtors", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.ExternalId.Should().Be("EXT-001");
        result.Data.DisplayName.Should().Be("John Doe");
        result.Data.DebtorType.Should().Be(DebtorType.Individual);
        result.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateDebtor_ReturnsCreatedDebtor_ForCompany()
    {
        var request = new CreateDebtorRequest
        {
            ExternalId = "EXT-002",
            DebtorType = DebtorType.Company,
            CompanyName = "Acme Corp",
            TaxId = "98-7654321",
        };

        var response = await Client.PostAsJsonAsync("/api/v1/debtors", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.DisplayName.Should().Be("Acme Corp");
        result.Data.DebtorType.Should().Be(DebtorType.Company);
    }

    [Fact]
    public async Task GetDebtor_ReturnsDebtor_WhenExists()
    {
        var debtor = new Debtor
        {
            ExternalId = "EXT-003",
            DebtorType = DebtorType.Individual,
            FirstName = "Jane",
            LastName = "Smith",
            DisplayName = "Jane Smith",
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/v1/debtors/{debtor.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(debtor.Id);
        result.Data.ExternalId.Should().Be("EXT-003");
    }

    [Fact]
    public async Task GetDebtor_ReturnsNotFound_WhenNotExists()
    {
        var response = await Client.GetAsync($"/api/v1/debtors/{Guid.NewGuid()}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateDebtor_ReturnsConflict_WhenExternalIdExists()
    {
        var debtor = new Debtor
        {
            ExternalId = "DUPLICATE-001",
            DebtorType = DebtorType.Individual,
            FirstName = "First",
            LastName = "Person",
            DisplayName = "First Person",
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var request = new CreateDebtorRequest
        {
            ExternalId = "DUPLICATE-001",
            DebtorType = DebtorType.Individual,
            FirstName = "Second",
            LastName = "Person",
        };

        var response = await Client.PostAsJsonAsync("/api/v1/debtors", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateDebtor_UpdatesAndReturnsDebtor()
    {
        var debtor = new Debtor
        {
            ExternalId = "UPD-001",
            DebtorType = DebtorType.Individual,
            FirstName = "Original",
            LastName = "Name",
            DisplayName = "Original Name",
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var updateRequest = new UpdateDebtorRequest
        {
            ExternalId = "UPD-001",
            DebtorType = DebtorType.Individual,
            FirstName = "Updated",
            LastName = "Person",
            Notes = "Updated via test",
        };

        var response = await Client.PutAsJsonAsync($"/api/v1/debtors/{debtor.Id}", updateRequest);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorDto>>();
        result.Should().NotBeNull();
        result!.Data.DisplayName.Should().Be("Updated Person");
        result.Data.Notes.Should().Be("Updated via test");
    }

    [Fact]
    public async Task DeleteDebtor_DeactivatesDebtor()
    {
        var debtor = new Debtor
        {
            ExternalId = "DEL-001",
            DebtorType = DebtorType.Individual,
            FirstName = "To",
            LastName = "Delete",
            DisplayName = "To Delete",
            IsActive = true,
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/v1/debtors/{debtor.Id}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        DbContext.ChangeTracker.Clear();
        var deletedDebtor = await DbContext.Debtors.FindAsync(debtor.Id);
        deletedDebtor.Should().NotBeNull();
        deletedDebtor!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AddContact_AddsContactToDebtor()
    {
        var debtor = new Debtor
        {
            ExternalId = "CON-001",
            DebtorType = DebtorType.Individual,
            FirstName = "Contact",
            LastName = "Test",
            DisplayName = "Contact Test",
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var request = new CreateContactRequest
        {
            Type = ContactType.Email,
            Value = "contact@test.com",
            IsPrimary = true,
        };

        var response = await Client.PostAsJsonAsync($"/api/v1/debtors/{debtor.Id}/contacts", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorContactDto>>();
        result.Should().NotBeNull();
        result!.Data.Value.Should().Be("contact@test.com");
        result.Data.Type.Should().Be(ContactType.Email);
        result.Data.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task AddAddress_AddsAddressToDebtor()
    {
        var debtor = new Debtor
        {
            ExternalId = "ADDR-001",
            DebtorType = DebtorType.Individual,
            FirstName = "Address",
            LastName = "Test",
            DisplayName = "Address Test",
        };
        DbContext.Debtors.Add(debtor);
        await DbContext.SaveChangesAsync();

        var request = new CreateAddressRequest
        {
            Label = AddressLabel.Home,
            AddressLine1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            IsPrimary = true,
        };

        var response = await Client.PostAsJsonAsync($"/api/v1/debtors/{debtor.Id}/addresses", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DebtorAddressDto>>();
        result.Should().NotBeNull();
        result!.Data.AddressLine1.Should().Be("123 Main St");
        result.Data.City.Should().Be("New York");
        result.Data.IsPrimary.Should().BeTrue();
    }
}

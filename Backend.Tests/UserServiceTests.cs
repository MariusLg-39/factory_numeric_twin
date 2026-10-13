using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProjetFullstack.DTO.Users;
using ProjetFullstack.Models;
using ProjetFullstack.Repositories.Interfaces;
using ProjetFullstack.Services;
using Xunit;

namespace Backend.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();

        var storeMock = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    private UserService CreateService()
    {
        return new UserService(
            _repositoryMock.Object,
            _userManagerMock.Object);
    }

    private static ApplicationUser CreateTestUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Jean",
            LastName = "Dupont",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CreateUserRequest CreateRequest()
    {
        return new CreateUserRequest
        {
            Email = "nouveau@example.com",
            UserName = "nouveauuser",
            Password = "MotDePasse123!",
            FirstName = "Marie",
            LastName = "Martin"
        };
    }

    private static UpdateUserRequest CreateUpdateRequest()
    {
        return new UpdateUserRequest
        {
            Email = "modifie@example.com",
            UserName = "utilisateurmodifie",
            FirstName = "Pierre",
            LastName = "Martin",
            IsActive = true
        };
    }

    // --------------------------------------------------
    // GET ALL
    // --------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            CreateTestUser(),
            CreateTestUser()
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync((IReadOnlyList<ApplicationUser>)users);

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);

        _repositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoUsers_ReturnsEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync((IReadOnlyList<ApplicationUser>)
                new List<ApplicationUser>());

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    // --------------------------------------------------
    // GET BY ID
    // --------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    // --------------------------------------------------
    // GET BY EMAIL
    // --------------------------------------------------

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser();

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetByEmailAsync(user.Email!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string email = "inconnu@example.com";

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateService();

        // Act
        var result = await service.GetByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    // --------------------------------------------------
    // CREATE
    // --------------------------------------------------

    [Fact]
    public async Task CreateAsync_WhenCreationSucceeds_ReturnsSuccess()
    {
        // Arrange
        var request = CreateRequest();

        _userManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.NotNull(result.User);
        Assert.Equal(request.Email, result.User.Email);
        Assert.Equal(request.UserName, result.User.UserName);
        Assert.Equal(request.FirstName, result.User.FirstName);
        Assert.Equal(request.LastName, result.User.LastName);
        Assert.True(result.User.IsActive);

        _userManagerMock.Verify(
            m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenIdentityRejectsUser_ReturnsFailure()
    {
        // Arrange
        var request = CreateRequest();

        var identityError = new IdentityError
        {
            Description = "Email déjà utilisé."
        };

        _userManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Email déjà utilisé.", result.Error);
        Assert.Null(result.User);
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    [Fact]
    public async Task UpdateAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = CreateUpdateRequest();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(id.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Utilisateur introuvable.", result.Error);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailBelongsToAnotherUser_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var anotherUser = CreateTestUser();
        var request = CreateUpdateRequest();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(anotherUser);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Cet email est déjà utilisé.", result.Error);
        Assert.Null(result.User);

        _userManagerMock.Verify(
            m => m.UpdateAsync(It.IsAny<ApplicationUser>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSucceeds_ReturnsUpdatedUser()
    {
        // Arrange
        var user = CreateTestUser();
        var request = CreateUpdateRequest();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(user.Id, request);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.NotNull(result.User);
        Assert.Equal(request.Email, result.User.Email);
        Assert.Equal(request.UserName, result.User.UserName);
        Assert.Equal(request.FirstName, result.User.FirstName);
        Assert.Equal(request.LastName, result.User.LastName);
        Assert.Equal(request.IsActive, result.User.IsActive);

        _userManagerMock.Verify(
            m => m.UpdateAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdentityRejectsUpdate_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var request = CreateUpdateRequest();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Erreur de mise à jour."
                }));

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(user.Id, request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Erreur de mise à jour.", result.Error);
        Assert.Null(result.User);
    }

    // --------------------------------------------------
    // DELETE
    // --------------------------------------------------

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(id.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Utilisateur introuvable.", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeletionSucceeds_ReturnsSuccess()
    {
        // Arrange
        var user = CreateTestUser();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);

        _userManagerMock.Verify(
            m => m.DeleteAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenIdentityRejectsDeletion_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();

        _userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Suppression impossible."
                }));

        var service = CreateService();

        // Act
        var result = await service.DeleteAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Suppression impossible.", result.Error);
    }
}

using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Moq;
using SeaBattle.Application.Exceptions;
using SeaBattle.Application.Interfaces;
using SeaBattle.Application.Services;
using SeaBattle.Domain.Exceptions;
using SeaBattle.Domain.Models;
using SeaBattle.Domain.Models.Dto;
using SeaBattle.Domain.Models.Errors;
using SeaBattle.Domain.Models.Results;

namespace SeaBattle.UnitTests.Services;

public class GameServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork = new();
    private Mock<IRepository<Game>> _mockGameRepository= new();
    private Mock<IUserGameService> _mockUserGameService = new();
    private Mock<IAppUserService> _mockUserService = new();
    private Mock<IGameInvitationService> _mockInvitationService = new();
    private Mock<IShipService> _mockShipService = new();
    private Mock<ICoordinateService> _mockCoordinateService = new();
    private Mock<IShipCoordinateService> _mockShipCoordinateService = new();
    private Mock<IGameFieldService> _mockGameFieldService = new();
    
    [Theory]
    [InlineData("AddShipToField")]
    [InlineData("Delete")]
    [InlineData("GetById")]
    [InlineData("Join")]
    [InlineData("InviteUser")]
    [InlineData("AcceptInvitation")]
    [InlineData("UpdateUserStatusOnReady")]
    [InlineData("UpdatePlayerTurn")]
    public void Method_WhenGameDoesNotExist_ReturnsFailureResult(string methodName)
    {
        var id = 1000;
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Game)null!);
        
        var result = GetResultFromActSection(methodName, id, gameDto);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ServiceErrors.GameServiceExceptions.NonExistentGame.Code, result.Error.Code);
    }
    
    [Theory]
    [InlineData("Insert")]
    [InlineData("Join")]
    [InlineData("InviteUser")]
    [InlineData("AcceptInvitation")]
    public void Method_WhenUserDoesNotExist_ReturnFailureResult(string methodName)
    {
        var failureResult = Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser);
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        var game = new Game("Game-1", 1) { GameId = 1 };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(failureResult);
        
        var result = GetResultFromActSection(methodName, game.GameId, gameDto);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureResult.Error.Code);
    }
    
    [Theory]
    [InlineData("AddShipToField")]
    [InlineData("Delete")]
    [InlineData("Insert")]
    [InlineData("Join")]
    [InlineData("InviteUser")]
    [InlineData("Update")]
    [InlineData("UpdateUserStatusOnReady")]
    [InlineData("UpdateCoordinateType")]
    [InlineData("UpdatePlayerTurn")]
    public void Method_WhenImpossibleCommitChanges_ReturnFailureResult(string methodName)
    {
        var id = SetArrangeData(methodName);
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        
        var result = GetResultFromActSection(methodName, id, gameDto);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.UnitOfWorkExceptions.ImpossibleCommitChanges.Code);
    }
    
    [Theory]
    [InlineData("AddShipToField")]
    [InlineData("UpdateCoordinateType")]
    [InlineData("UpdatePlayerTurn")]
    public void AddShipToField_WhenCoordinateDoesNotExist_ReturnFailureResult(string methodName)
    {
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        var successShipResult = Result.Success(new WarShip() as BaseShip);
        var failureCoordinateResult = Result.Failure<Coordinate>(ServiceErrors.CoordinateServiceExceptions.FailedCreateCoordinate);
        var game = new Game("Game-1", 1) { GameId = 1 };
        var id = game.GameId;
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(failureCoordinateResult);
        
        var result = GetResultFromActSection(methodName, id, gameDto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureCoordinateResult.Error.Code);
    }
    
    [Fact]
    public void GetAll_WhenGamesExist_ReturnGames()
    {
        var games = new List<Game>()
        {
            new("Game-1", 1),
            new("Game-2", 1)
        };
        
        _mockGameRepository.Setup(repo => repo.GetAll(It.IsAny<int>(), It.IsAny<int>())).Returns(games);
        
        var gameService = CreateInstance();

        var result = gameService.GetAll();
        
        result.IsFailure.Should().BeFalse();
        result.Value.Should().HaveCountGreaterThan(0);
    }
    
    [Fact]
    public void GetAll_WhenThereAreNoGames_ReturnEmptyList()
    {
        var games = new List<Game>();
        
        _mockGameRepository.Setup(repo => repo.GetAll(It.IsAny<int>(), It.IsAny<int>())).Returns(games);
        
        var gameService = CreateInstance();

        var result = gameService.GetAll();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.GameServiceExceptions.NonExistentGames.Code);
    }
    
    [Fact]
    public void GetById_WhenGameExists_ReturnGame()
    {
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        
        var gameService = CreateInstance();

        var result = gameService.GetById(game.GameId);
        
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public void Insert_WhenInvalidUserId_ReturnFailureResult()
    {
        var gameDto = new GameDto() { CreatorId = 0 };
        var failureResult = Result.Failure<AppUser>(ServiceErrors.AppUserServiceExceptions.NonExistentUser);
        
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(failureResult);
        
        var gameService = CreateInstance();

        var result = gameService.Insert(gameDto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureResult.Error.Code);
    }
    
    [Fact]
    public void Insert_WhenCannotCreateNewGame_ReturnFailureResult()
    {
        var successResult = Result.Success(new AppUser());
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successResult);
        _mockGameRepository.Setup(repo => repo.Insert(It.IsAny<Game>())).Returns((Game)null!);
        
        var gameService = CreateInstance();

        var result = gameService.Insert(gameDto);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.GameServiceExceptions.FailedCreateGame.Code);
    }
 
    [Fact]
    public void Insert_WhenCreatedGame_ReturnNewGame()
    {
        var game = new Game("Game-1", 1);
        var successUserResult = Result.Success(new AppUser());
        var gameDto = new GameDto() { Name = "Game-1", CreatorId = 1 };
        var successUserGameResult = Result.Success(new UserGames());
        var saveChangesResult = true;
        
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockGameRepository.Setup(repo => repo.Insert(It.IsAny<Game>())).Returns(game);
        _mockUserGameService.Setup(ugs => ugs.Insert(It.IsAny<Game>(), successUserResult.Value)).Returns(successUserGameResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        var gameService = CreateInstance();

        var result = gameService.Insert(gameDto);

        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Update_WhenUpdateGame_ReturnSuccessResult()
    {
        var gameId = 1;
        var saveChangesResult = true;
        
        var gameDto = new GameDto()
        {
            Name = "Game-1",
            CreatorId = 1
        };
        
        _mockGameRepository.Setup(repo => repo.Update(It.IsAny<Game>()));
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);

        var gameService = CreateInstance();

        var result = gameService.Update(gameId, gameDto);
        
        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void Delete_WhenDeleteGame_ReturnSuccessResult()
    {
        var saveChangesResult = true;
        var invitations = Result.Success(new List<GameInvitation>());
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockInvitationService.Setup(inv => inv.GetAll(It.IsAny<int>(), It.IsAny<int>())).Returns(invitations);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        var gameService = CreateInstance();

        var result = gameService.Delete(game.GameId);

        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void Join_WhenUserHasAlreadyJoinedThisGame_ReturnFailureResult()
    {
        var id = 1000;

        var gameUsers = new List<UserGames>()
        {
            new() { AppUserId = id }
        };

        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = gameUsers
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        
        var gameService = CreateInstance();

        var result = gameService.Join(game.GameId, id);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.GameServiceExceptions.UserHasAlreadyJoinedThisGame.Code);
    }

    [Fact]
    public void Join_WhenGameHasAlreadyStarted_ReturnFailureResult()
    {
        var id = 1000;

        var gameUsers = new List<UserGames>()
        {
            new() { AppUserId = id + 1 },
            new() { AppUserId = id + 2 },
            new() { AppUserId = id + 3 }
        };

        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = gameUsers
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        
        var gameService = CreateInstance();

        var result = gameService.Join(game.GameId, id);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.GameServiceExceptions.GameHasAlreadyStarted.Code);
    }

    [Fact]
    public void Join_WhenCannotCreateUserGame_ReturnFailureResult()
    {
        var successUserResult = Result.Success(new AppUser());
        var failureUserGameResult = Result.Failure<UserGames>(ServiceErrors.GameFieldServiceExceptions.FailedCreateGameField);
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockUserGameService.Setup(ug => ug.Insert(game, successUserResult.Value)).Returns(failureUserGameResult);
        
        var gameService = CreateInstance();

        var result = gameService.Join(game.GameId, game.CreatorId);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureUserGameResult.Error.Code);
    }
    
    [Fact]
    public void Join_WhenJoinUserToGame_ReturnGame()
    {
        var saveChangesResult = true;
        var successUserResult = Result.Success(new AppUser());
        var successUserGameResult = Result.Success(new UserGames());
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockUserGameService.Setup(ug => ug.Insert(game, successUserResult.Value)).Returns(successUserGameResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);

        var gameService = CreateInstance();

        var result = gameService.Join(game.GameId, game.CreatorId);
        
        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void InviteUser_WhenCannotCrateNewInvitation_ReturnFailureResult()
    {
        var successUserResult = Result.Success(new AppUser());
        var failureInvitationResult = Result.Failure<GameInvitation>(ServiceErrors.GameInvitationServiceExceptions.InvitationAlreadyExists);
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockInvitationService.Setup(ug => ug.CreateNewInvitation(game, successUserResult.Value)).Returns(failureInvitationResult);
        
        var gameService = CreateInstance();

        var result = gameService.InviteUser(game.GameId, game.CreatorId);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureInvitationResult.Error.Code);
    }
    
    [Fact]
    public void InviteUser_WhenCreateInvitation_ReturnInvitation()
    {
        var saveChangesResult = true;
        var successUserResult = Result.Success(new AppUser());
        var successInvitationResult = Result.Success(new GameInvitation());
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockInvitationService.Setup(ug => ug.CreateNewInvitation(game, successUserResult.Value)).Returns(successInvitationResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult); 
        
        var gameService = CreateInstance();

        var result = gameService.InviteUser(game.GameId, game.CreatorId);
        
        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void AcceptInvitation_WhenInvitationHasAlreadyExist_ReturnFailureResult()
    {
        var successUserResult = Result.Success(new AppUser());
        var failureInvitationResult = Result.Failure(ServiceErrors.GameInvitationServiceExceptions.InvitationAlreadyExists);
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };

        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockInvitationService.Setup(inv => inv.IsInvitationExist(game, successUserResult.Value)).Returns(failureInvitationResult);
        
        var gameService = CreateInstance();

        var result = gameService.AcceptInvitation(game.GameId, game.CreatorId);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureInvitationResult.Error.Code);
    }
    
    [Fact]
    public void AddShipToField_WhenCannotCreateShip_ReturnFailureResult()
    {
        var failureShipResult = Result.Failure<BaseShip>(ServiceErrors.ShipServiceExceptions.FailedCreateShip);
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(failureShipResult);
        
        var gameService = CreateInstance();

        var result = gameService.AddShipToField(new ShipDto(), game.CreatorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureShipResult.Error.Code);
    }

    [Fact]
    public void AddShipToField_WhenGameFieldDoesNotExist_ReturnFailureResult()
    {
        var successShipResult = Result.Success(new WarShip() as BaseShip);
        var successCoordinateResult = Result.Success(new Coordinate());
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        
        var gameService = CreateInstance();

        var result = gameService.AddShipToField(new ShipDto(), game.CreatorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame.Code);
    }
    
    [Fact]
    public void AddShipToField_WhenShipDoesNotExist_ReturnFailureResult()
    {
        var successShipResult = Result.Success((BaseShip)null!);
        var successCoordinateResult = Result.Success(new Coordinate());
        
        var userGame = new List<UserGames> {
            new() { GameField = new GameField(), AppUserId = 1 }
        };
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = userGame
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        
        var gameService = CreateInstance();

        var result = gameService.AddShipToField(new ShipDto(), game.CreatorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.ShipServiceExceptions.NonExistentShip.Code);
    }

    [Fact]
    public void AddShipToField_WhenCannotAddShipToField_ReturnFailureResult()
    {
        var successShipResult = Result.Success(new WarShip() as BaseShip);
        var successCoordinateResult = Result.Success(new Coordinate());
        var failureAddShipToFieldResult = Result.Failure(DomainErrors.GameFieldServiceException.InvalidShipPlacement);
        
        var userGame = new List<UserGames> {
            new() { GameField = new GameField(), AppUserId = 1 }
        };
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = userGame
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockShipCoordinateService.Setup(gf => gf.AddShipToGameField(It.IsAny<BaseShip>(), It.IsAny<GameField>(),
            It.IsAny<Coordinate>(), It.IsAny<ShipDto>())).Returns(failureAddShipToFieldResult);

        var gameService = CreateInstance();

        var result = gameService.AddShipToField(new ShipDto(), game.CreatorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(failureAddShipToFieldResult.Error.Code);
    }
    
    [Fact]
    public void AddShipToField_WhenAddShipToField_ReturnGame()
    {
        var saveChangesResult = true;
        var successShipResult = Result.Success(new WarShip() as BaseShip);
        var successCoordinateResult = Result.Success(new Coordinate());
        var successAddShipToFieldResult = Result.Success();
        
        var userGame = new List<UserGames> {
            new() { GameField = new GameField(), AppUserId = 1 }
        };
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = userGame
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockShipCoordinateService.Setup(gf => gf.AddShipToGameField(It.IsAny<BaseShip>(), It.IsAny<GameField>(),
            It.IsAny<Coordinate>(), It.IsAny<ShipDto>())).Returns(successAddShipToFieldResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);

        var gameService = CreateInstance();

        var result = gameService.AddShipToField(new ShipDto(), game.CreatorId);

        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }

    [Fact]
    public void UpdateUserStatusOnReady_WhenReadyPropertyDoesNotExist_ReturnFailureResult()
    {
        var successCoordinateResult = Result.Success(new Coordinate());
        var game = new Game("Game-1", 1) { GameId = 1 };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        
        var gameService = CreateInstance();

        var result = gameService.UpdateUserStatusOnReady(game.GameId, game.CreatorId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ServiceErrors.UserGameFieldServiceExceptions.NonExistentUserGame.Code);
    }
    
    [Fact]
    public void UpdateUserStatusOnReady_WhenSetUserStatusAsReady_ReturnGame()
    {
        var saveChangesResult = true;
        var successCoordinateResult = Result.Success(new Coordinate());
        
        var userGame = new List<UserGames> {
            new() { GameField = new GameField(), AppUserId = 1 },
            new() { GameField = new GameField(), AppUserId = 2 }
        };
        
        var game = new Game("Game-1", 1)
        {
            GameId = 1,
            GameUsers = userGame
        };
        
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        var gameService = CreateInstance();

        var result = gameService.UpdateUserStatusOnReady(game.GameId, game.CreatorId);

        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCoordinateType_WhenUpdateType_ReturnCoordinate()
    {
        var saveChangesResult = true;
        var successCoordinateResult = Result.Success(new Coordinate() { CoordinateId = 10, CoordinateType = new CoordinateType() { Type = "Destroyed"}});
        
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        var gameService = CreateInstance();

        var result = gameService.UpdateCoordinateType(successCoordinateResult.Value.CoordinateId);

        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePlayerTurn_WhenUpdateTurn_ReturnSuccessResult()
    {
        var saveChangesResult = true;
        var userGames = new List<UserGames> { new() { GameField = new GameField(), AppUserId = 1 }, new() { GameField = new GameField(), AppUserId = 2 } };
        var game = new Game("Game-1", 1) { GameId = 1, GameUsers = userGames };
        var successCoordinateResult = Result.Success(new Coordinate() { CoordinateId = 1, CoordinateType = new CoordinateType() { Type = "Empty"}});
        var id = successCoordinateResult.Value.CoordinateId;

        _mockGameRepository.Setup(g => g.GetById(It.IsAny<int>())).Returns(game);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        var gameService = CreateInstance();

        var result = gameService.UpdatePlayerTurn(id, id, id);

        result.IsFailure.Should().BeFalse();
        result.Should().NotBeNull();
    }

    private int SetArrangeData(string methodName)
    {
        var saveChangesResult = false;
        var successShipResult = Result.Success(new WarShip() as BaseShip);
        var successCoordinateResult = Result.Success(new Coordinate() { CoordinateType = new CoordinateType() { Type = "Empty"}});
        var successAddShipToFieldResult = Result.Success();
        var invitations = Result.Success(new List<GameInvitation>());
        var successUserResult = Result.Success(new AppUser());
        var successUserGameResult = Result.Success(new UserGames());
        var successInvitationsResult = Result.Success(new GameInvitation());
        var userGames = new List<UserGames> { new() { GameField = new GameField(), AppUserId = 1 }, new() { GameField = new GameField(), AppUserId = 2 } };
        var game = new Game("Game-1", 1) { GameId = 1, GameUsers = userGames };
       
        _mockUserService.Setup(us => us.GetUserById(It.IsAny<int>())).Returns(successUserResult);
        _mockUserGameService.Setup(ugs => ugs.Insert(It.IsAny<Game>(), It.IsAny<AppUser>())).Returns(successUserGameResult);
        _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        _mockGameRepository.Setup(repo => repo.Insert(It.IsAny<Game>())).Returns(game);
        _mockShipService.Setup(sh => sh.Insert(It.IsAny<ShipDto>())).Returns(successShipResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockShipCoordinateService.Setup(gf => gf.AddShipToGameField(It.IsAny<BaseShip>(), It.IsAny<GameField>(),
            It.IsAny<Coordinate>(), It.IsAny<ShipDto>())).Returns(successAddShipToFieldResult);
        _mockInvitationService.Setup(inv => inv.GetAll(It.IsAny<int>(), It.IsAny<int>())).Returns(invitations);
        _mockInvitationService.Setup(ug => ug.CreateNewInvitation(It.IsAny<Game>(), It.IsAny<AppUser>())).Returns(successInvitationsResult);
        _mockCoordinateService.Setup(c => c.GetById(It.IsAny<int>())).Returns(successCoordinateResult);
        _mockUnitOfWork.Setup(uow => uow.SaveChanges()).Returns(saveChangesResult);
        
        if (methodName.Equals("Join"))
        {
            game.GameUsers.RemoveAt(1);
            _mockGameRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(game);
        }

        return game.GameId;
    }

    private Result GetResultFromActSection(string methodName, int id, GameDto gameDto = null!)
    {
        var gameService = CreateInstance();
        
        Func<Result> methodCall = methodName switch
        {
            "AddShipToField" => () => gameService.AddShipToField(new ShipDto(), id),
            "Insert" => () => gameService.Insert(gameDto),
            "GetById" => () => gameService.GetById(id),
            "Delete" => () => gameService.Delete(id),
            "Update" => () => gameService.Update(id, gameDto),
            "Join" => () => gameService.Join(id, id + 10),
            "InviteUser" => () => gameService.InviteUser(id, id),
            "AcceptInvitation" => () => gameService.AcceptInvitation(id, id),
            "UpdateUserStatusOnReady" => () => gameService.UpdateUserStatusOnReady(id, id),
            "UpdateCoordinateType" => () => gameService.UpdateCoordinateType(id),
            "UpdatePlayerTurn" => () => gameService.UpdatePlayerTurn(id, id, id),
            _ => throw new ArgumentException("Invalid method name", nameof(methodName)),
        };
        
        return methodCall();
    }
    
    private GameService CreateInstance()
    {
        return new GameService(
            _mockGameRepository.Object,
            _mockUnitOfWork.Object,
            _mockUserGameService.Object,
            _mockUserService.Object,
            _mockInvitationService.Object,
            _mockShipService.Object,
            _mockCoordinateService.Object,
            _mockShipCoordinateService.Object,
            _mockGameFieldService.Object
        );
    }
}
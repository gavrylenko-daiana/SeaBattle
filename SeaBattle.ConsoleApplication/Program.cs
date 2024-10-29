using SeaBattle.Domain.Models;
using Microsoft.Extensions.Configuration;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Interfaces;
using SeaBattle.Domain.Services;
using SeaBattle.Infrastructure;
using SeaBattle.Infrastructure.Interfaces;
using SeaBattle.Infrastructure.Services;
using CoordinateService = SeaBattle.Domain.Services.CoordinateService;
using GameFieldService = SeaBattle.Domain.Services.GameFieldService;
using ICoordinateService = SeaBattle.Domain.Interfaces.ICoordinateService;
using IGameFieldService = SeaBattle.Domain.Interfaces.IGameFieldService;

namespace SeaBattle.ConsoleApplication;

class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        #region SetOrmServices

        IMappingService mappingService = new MappingService();
        ISqlExecutorService executorService = new SqlExecutorService(connectionString);
        ICrudOperationsService<GameField> crudOperationsGameFieldService = new CrudOperationsService<GameField>(mappingService, executorService);
        ICrudOperationsService<Coordinate> crudOperationsCoordinateService = new CrudOperationsService<Coordinate>(mappingService, executorService);
        ICrudOperationsService<BaseShip> crudOperationsShipService = new CrudOperationsService<BaseShip>(mappingService, executorService);
        ICrudOperationsService<CoordinateType> crudOperationsCoordinateTypeService = new CrudOperationsService<CoordinateType>(mappingService, executorService);
        ICrudOperationsService<ShipCoordinate> crudOperationsShipCoordinateService = new CrudOperationsService<ShipCoordinate>(mappingService, executorService);
        ICrudOperationsService<Point> crudOperationsPointService = new CrudOperationsService<Point>(mappingService, executorService);
        ICrudOperationsService<UserGames> crudOperationsUserGamesService = new CrudOperationsService<UserGames>(mappingService, executorService);
        ICrudOperationsService<Game> crudOperationsGamesService = new CrudOperationsService<Game>(mappingService, executorService);

        var uow = new UnitOfWork(executorService);

        #endregion

        #region SetRepository

        IRepository<GameField> repositoryGameField = new Repository<GameField>(executorService, crudOperationsGameFieldService, mappingService);
        IRepository<Coordinate> repositoryCoordinate = new Repository<Coordinate>(executorService, crudOperationsCoordinateService, mappingService);
        IRepository<BaseShip> repositoryShip = new Repository<BaseShip>(executorService, crudOperationsShipService, mappingService);
        IRepository<CoordinateType> repositoryCoordinateType = new Repository<CoordinateType>(executorService, crudOperationsCoordinateTypeService, mappingService);
        IRepository<ShipCoordinate> repositoryShipCoordinate = new Repository<ShipCoordinate>(executorService, crudOperationsShipCoordinateService, mappingService);
        IRepository<Point> repositoryPoint = new Repository<Point>(executorService, crudOperationsPointService, mappingService);
        IRepository<UserGames> repositoryUserGames = new Repository<UserGames>(executorService, crudOperationsUserGamesService, mappingService);
        IRepository<Game> repositoryGames = new Repository<Game>(executorService, crudOperationsGamesService, mappingService);

        #endregion

        #region SetDomainServices

        IValidationService validationService = new ValidationService();
        IComputeCoordinateService computeCoordinateService = new ComputeCoordinateService(validationService);
        ICoordinateService coordinateService = new CoordinateService(validationService, computeCoordinateService);
        IGameFieldService gameFieldService = new GameFieldService(coordinateService);

        #endregion

        #region ExistedTypes
        
        // var coordinateType = CoordinateType.GetCoordinateTypeByTypeName();
        //
        // if (!crudOperationsCoordinateService.ExistsInDatabase(coordinateType))
        // {
        //     repositoryCoordinateType.Insert(coordinateType);
        // }
        //
        // var shipType = ShipType.GetShipTypeByTypeName(nameof(HybridShip));
        //
        // if (!crudOperationsShipService.ExistsInDatabase(shipType))
        // {
        //     repositoryCoordinateType.Insert(coordinateType);
        // }
        //
        // uow.SaveChanges();

        #endregion

        #region InsertGameField/Coordinate/Point/Ship

        // var gameFieldFirstResult = GameField.CreateGameField(60);
        // gameFieldFirstResult.Value.SetCoordinatesToGameField();
        // var gameFieldInsert = repositoryGameField.Insert(gameFieldFirstResult.Value);
        //
        // foreach (var coordinate in gameFieldFirstResult.Value.Coordinates)
        // {
        //     var newPoint = repositoryPoint.Insert(coordinate.Point);
        //
        //     coordinate.PointId = newPoint.PointId;
        //     coordinate.GameFieldId = gameFieldInsert.GameFieldId;
        //
        //     repositoryCoordinate.Insert(coordinate);
        // }
        //
        // var shipResult = BaseShip.CreateShip(10, 2, Direction.Right, nameof(WarShip));
        //
        // if (shipResult.IsFailure)
        // {
        //     Console.WriteLine(shipResult.Error.Message);
        //     return;
        // }
        //
        // repositoryShip.Insert(shipResult.Value);
        //
        // uow.SaveChanges();

        #endregion

        #region AddShipToField(Insert/Update)

        // var getShip = repositoryShip.GetById(80);
        // var getGameField = repositoryGameField.GetById(80);
        //
        // if (getShip is not null && getGameField is not null)
        // {
        //     var addShipToFieldResult = gameFieldService.AddShipToField(getShip,
        //         getGameField.Coordinates.Find(c => c.Point.X == -30 && c.Point.Y == 5),
        //         getShip.Direction, getGameField);
        //
        //     if (!addShipToFieldResult.IsFailure)
        //     {
        //         foreach (var shipCoordinate in getShip.ShipCoordinates)
        //         {
        //             repositoryShipCoordinate.Insert(shipCoordinate);
        //             repositoryCoordinate.Update(shipCoordinate.Coordinate);
        //         }
        //         
        //         repositoryShip.Update(getShip);
        //         repositoryGameField.Update(getGameField);
        //         
        //         uow.SaveChanges();
        //     }
        //     else
        //     {
        //         Console.WriteLine(addShipToFieldResult.Error.Message);
        //     }
        // }
        // else
        // {
        //     Console.WriteLine("Entity with such Id does not existed");
        // }

        #endregion

        #region GetData

        // var ship = repositoryShip.GetById(shipResult.Value.ShipId);
        // var ships = repositoryShip.GetAll(2);
        // var coordinates = repositoryCoordinate.GetAll(1, 10000);
        // var coordinate = repositoryCoordinate.GetById(3);
        // var gameField = repositoryGameField.GetById(8);
        // var getShip = repositoryShip.GetById(19);
        // var shipCoordinate = repositoryShipCoordinate.GetById(10);

        #endregion

        #region Delete

        // repositoryGameField.Delete(1);
        // uow.SaveChanges();

        #endregion


        var game = repositoryGames.GetById(1);
        var gameInvitation = game.GameInvitations;
    }
}
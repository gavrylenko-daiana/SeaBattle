using SeaBattle.Persistence;
using SeaBattle.Application.Interfaces;
using SeaBattle.Domain.Models;

namespace SeaBattle.Persistence.Repository;

public class UnitOfWork : IUnitOfWork
{
    private ApplicationContext _context;
    private Repository<AppUser> _userRepository;
    private Repository<Coordinate> _coordinateRepository;
    private Repository<CoordinateType> _coordinateTypeRepository;
    private Repository<Game> _gameRepository;
    private Repository<GameField> _gameFieldRepository;
    private Repository<GameInvitation> _gameInvitationRepository;
    private Repository<Point> _pointRepository;
    private Repository<ShipCoordinate> _shipCoordinateRepository;
    private Repository<UserGames> _userGamesRepository;
    private Repository<WarShip> _warShipRepository;
    private Repository<SupportShip> _supportShipRepository;
    private Repository<HybridShip> _hybridShipRepository;
    
    public UnitOfWork(ApplicationContext context)
    {
        _context = context;
    }

    public Repository<AppUser> UserRepository
    {
        get
        {
            if (_userRepository == null)
            {
                _userRepository = new Repository<AppUser>(_context);
            }

            return _userRepository;
        }
    }

    public Repository<Coordinate> CoordinateRepository
    {
        get
        {
            if (_coordinateRepository == null)
            {
                _coordinateRepository = new Repository<Coordinate>(_context);
            }

            return _coordinateRepository;
        }
    }

    public Repository<CoordinateType> CoordinateTypeRepository
    {
        get
        {
            if (_coordinateTypeRepository == null)
            {
                _coordinateTypeRepository = new Repository<CoordinateType>(_context);
            }

            return _coordinateTypeRepository;
        }
    }

    public Repository<Game> GameRepository
    {
        get
        {
            if (_gameRepository == null)
            {
                _gameRepository = new Repository<Game>(_context);
            }

            return _gameRepository;
        }
    }

    public Repository<GameField> GameFieldRepository
    {
        get
        {
            if (_gameFieldRepository == null)
            {
                _gameFieldRepository = new Repository<GameField>(_context);
            }

            return _gameFieldRepository;
        }
    }

    public Repository<GameInvitation> GameInvitationRepository
    {
        get
        {
            if (_gameInvitationRepository == null)
            {
                _gameInvitationRepository = new Repository<GameInvitation>(_context);
            }

            return _gameInvitationRepository;
        }
    }

    public Repository<Point> PointRepository
    {
        get
        {
            if (_pointRepository == null)
            {
                _pointRepository = new Repository<Point>(_context);
            }

            return _pointRepository;
        }
    }

    public Repository<ShipCoordinate> ShipCoordinateRepository
    {
        get
        {
            if (_shipCoordinateRepository == null)
            {
                _shipCoordinateRepository = new Repository<ShipCoordinate>(_context);
            }

            return _shipCoordinateRepository;
        }
    }

    public Repository<UserGames> UserGamesRepository
    {
        get
        {
            if (_userGamesRepository == null)
            {
                _userGamesRepository = new Repository<UserGames>(_context);
            }

            return _userGamesRepository;
        }
    }
    
    public Repository<WarShip> WarShipRepository
    {
        get
        {
            if (_warShipRepository == null)
            {
                _warShipRepository = new Repository<WarShip>(_context);
            }

            return _warShipRepository;
        }
    }

    public Repository<SupportShip> SupportShipRepository
    {
        get
        {
            if (_supportShipRepository == null)
            {
                _supportShipRepository = new Repository<SupportShip>(_context);
            }

            return _supportShipRepository;
        }
    }

    public Repository<HybridShip> HybridShipRepository
    {
        get
        {
            if (_hybridShipRepository == null)
            {
                _hybridShipRepository = new Repository<HybridShip>(_context);
            }

            return _hybridShipRepository;
        }
    }

    public async Task<bool> SaveChanges()
    {
        try
        {
            var recordsCount = await _context.SaveChangesAsync();

            return recordsCount > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Fail to save changes to the database: {ex.Message}");
        }
    }

    private bool disposed = false;

    protected virtual async Task Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                await _context.DisposeAsync();
            }
        }

        this.disposed = true;
    }

    public async void Dispose()
    {
        await Dispose(true);
        GC.SuppressFinalize(this);
    }
}
using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;

namespace SeaBattle.Domain.Models;

public class HybridShip : BaseShip, IShootable, IRepairable
{
    public HybridShip() : base() { }
    
    public HybridShip(int speed, int size, Direction direction, ShipType shipType) : base(speed, size, direction, shipType)
    {
    }

    public bool Shoot()
    {
        throw new NotImplementedException();
    }

    public bool Repair()
    {
        throw new NotImplementedException();
    }
}
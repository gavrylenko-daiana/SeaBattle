using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;

namespace SeaBattle.Domain.Models;

public class WarShip : BaseShip, IShootable
{
    public WarShip() : base() { }
    
    public WarShip(int speed, int size, Direction direction, ShipType shipType) : base(speed, size, direction, shipType)
    {
    }

    public bool Shoot()
    {
        throw new NotImplementedException();
    }
}
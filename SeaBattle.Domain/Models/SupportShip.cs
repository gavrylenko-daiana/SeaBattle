using SeaBattle.Domain.Enums;
using SeaBattle.Domain.Interfaces;

namespace SeaBattle.Domain.Models;

public class SupportShip : BaseShip, IRepairable
{
    public SupportShip() : base() { }
    
    public SupportShip(int speed, int size, Direction direction, ShipType shipType) : base(speed, size, direction, shipType)
    {
    }

    public bool Repair()
    {
        throw new NotImplementedException();
    }
}
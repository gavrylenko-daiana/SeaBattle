using SeaBattle.Domain.Models.Errors;

namespace SeaBattle.Domain.Exceptions;

public static class DomainErrors
{
    public static class BaseShipExceptions
    {
        public static Error LessThan0ShipSize => new Error(
            "BaseShipExceptions.LessThan0ShipSize",
            "The size of ship cannot be less than 0.");

        public static Error MoreThan4ShipSize => new Error(
            "BaseShipExceptions.MoreThan4ShipSize",
            "The size of ship cannot be more than 4.");

        public static Error LessThan0ShipSpeed => new Error(
            "BaseShipExceptions.LessThan0ShipSpeed",
            "Ship speed cannot be less than 0.");

        public static Error NotExistShip => new Error(
            "BaseShipExceptions.NotExistShip",
            "Such a ship doesn't exist.");

        public static Error NotExistTypeOfShip => new Error(
            "BaseShipExceptions.NotExistTypeOfShip",
            "This type of ship doesn't exist.");
    }

    public static class GameFieldExceptions
    {
        public static Error NoneExistShipOnThisCoordinate => new Error(
            "GameFieldExceptions.NoneExistShipOnThisCoordinate",
            "");

        public static Error NotEvenFieldSize => new Error(
            "GameFieldExceptions.NotEvenFieldSize",
            "The field size must be an even number.");

        public static Error LessThan10FieldSize => new Error(
            "GameFieldExceptions.LessThan10FieldSize",
            "The field size cannot be less than 10.");

        public static Error MoreThan100FieldSize => new Error(
            "GameFieldExceptions.MoreThan100FieldSize",
            "The field size cannot be more than 100.");

        public static Error CoordinateBeyondTheField => new Error(
            "GameFieldExceptions.CoordinateBeyondTheField",
            "The coordinate is out of the field.");
    }

    public static class CoordinateExceptions
    {
        public static Error CoordinateOutsideTheQuadrant => new Error(
            "CoordinateExceptions.LessThan0ShipSize",
            "The coordinate cannot be outside the quadrant.");
    }

    public static class GameFieldServiceException
    {
        public static Error InvalidShipPlacement => new Error(
            "GameFieldServiceException.InvalidShipPlacement",
            "The ship can't be at these coordinates.");
        
        public static Error OccupiedCoordinate => new Error(
            "GameFieldServiceException.OccupiedCoordinate",
            "This coordinate is already occupied.");
    }
}
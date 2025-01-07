CREATE TABLE [dbo].[ShipCoordinates]
(
	ShipCoordinateId INT PRIMARY KEY NOT NULL,
    CoordinateId INT NOT NULL,
    ShipId INT NOT NULL,
    FOREIGN KEY (CoordinateId) REFERENCES Coordinates(CoordinateId),
    FOREIGN KEY (ShipId) REFERENCES Ships(ShipId)
)

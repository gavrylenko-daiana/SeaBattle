CREATE TABLE [dbo].[Ships]
(
	ShipId INT PRIMARY KEY NOT NULL,
    Range INT NOT NULL,
    Size INT NOT NULL,
    Speed INT NOT NULL,
    Direction INT NOT NULL,
    ShipTypeId INT NOT NULL,
    FOREIGN KEY (ShipTypeId) REFERENCES ShipTypes(ShipTypeId)
)

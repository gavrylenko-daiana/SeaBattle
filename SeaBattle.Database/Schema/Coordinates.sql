CREATE TABLE [dbo].[Coordinates]
(
	CoordinateId INT PRIMARY KEY NOT NULL,
    Quadrant INT NOT NULL,
    GameFieldId INT NOT NULL,
    PointId INT NOT NULL,
    CoordinateTypeId INT NOT NULL,
    FOREIGN KEY (GameFieldId) REFERENCES GameFields(GameFieldId),
    FOREIGN KEY (PointId) REFERENCES Points(PointId),
    FOREIGN KEY (CoordinateTypeId) REFERENCES CoordinateTypes(CoordinateTypeId)
)
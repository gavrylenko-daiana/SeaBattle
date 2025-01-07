CREATE TABLE [dbo].[UserGames]
(
    [UserGamesId] INT PRIMARY KEY NOT NULL,
    [GameId] INT NOT NULL,
    [AppUserId] INT NOT NULL,
    [GameFieldId] INT NOT NULL,
    FOREIGN KEY ([GameId]) REFERENCES [Games]([GameId]),
    FOREIGN KEY ([AppUserId]) REFERENCES [Users]([AppUserId]),
    FOREIGN KEY ([GameFieldId]) REFERENCES [GameFields]([GameFieldId])
)
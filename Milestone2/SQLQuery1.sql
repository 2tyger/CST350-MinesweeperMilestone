SELECT * FROM Users;

-- Create saved games table for Milestone 4
-- Id (int identity), UserId (int), DateSaved (datetime), GameData (nvarchar(max))
IF OBJECT_ID('Games', 'U') IS NULL
BEGIN
    CREATE TABLE Games (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        DateSaved DATETIME NOT NULL,
        GameData NVARCHAR(MAX) NOT NULL
    );
END

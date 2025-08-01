DROP TABLE IF EXISTS StudySessions;
DROP TABLE IF EXISTS Flashcards;
DROP TABLE IF EXISTS Stacks;

CREATE TABLE Stacks (
    Id INT IDENTITY(1, 1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Flashcards (
    Id INT IDENTITY(1, 1) PRIMARY KEY,
    StackId INT,
    Front NVARCHAR(50) NOT NULL,
    Back NVARCHAR(50) NOT NULL,
    FOREIGN KEY (StackId) REFERENCES Stacks(Id)
    ON DELETE CASCADE
);

CREATE TABLE StudySessions (
    Id INT IDENTITY(1, 1) PRIMARY KEY,
    StackId INT,
    Date DATETIME NOT NULL,
    Score INT NOT NULL,
    FOREIGN KEY (StackId) REFERENCES Stacks(Id)
    ON DELETE CASCADE
);
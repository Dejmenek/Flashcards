DROP TABLE IF EXISTS StudySessions;
DROP TABLE IF EXISTS Cards;
DROP TABLE IF EXISTS Stacks;

CREATE TABLE Stacks (
    Id INT IDENTITY(1, 1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Cards (
    Id INT IDENTITY(1, 1) PRIMARY KEY,
    StackId INT,
    Front NVARCHAR(50) NULL,
    Back NVARCHAR(50) NULL,
    Question NVARCHAR(max) NULL,
    Choices NVARCHAR(max) NULL,
    ClozeText NVARCHAR(max) NULL,
    FillInText NVARCHAR(max) NULL,
    Answer NVARCHAR(max) NULL,
    CardType NVARCHAR(30) NOT NULL,
    CHECK (CardType IN ('Flashcard', 'Cloze', 'MultipleChoice', 'FillIn')),
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
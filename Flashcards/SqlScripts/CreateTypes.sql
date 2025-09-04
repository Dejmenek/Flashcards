DROP TYPE IF EXISTS CardProgressTVP;

CREATE TYPE CardProgressTVP AS TABLE
(
    CardId INT,
    Box INT,
    NextReviewDate DATETIME2
);
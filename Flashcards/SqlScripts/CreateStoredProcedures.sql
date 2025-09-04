CREATE PROCEDURE UpdateCardProgressBulk
    @CardProgressTVP CardProgressTVP READONLY
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE c
    SET
        c.Box = u.Box,
        c.NextReviewDate = u.NextReviewDate
    FROM Cards c
    INNER JOIN @CardProgressTVP u ON c.Id = u.CardId;
END;
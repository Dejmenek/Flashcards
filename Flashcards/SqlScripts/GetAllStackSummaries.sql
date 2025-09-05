SELECT
    s.Name,
    COUNT(
        CASE WHEN c.NextReviewDate <= CAST(GETDATE() AS DATE) THEN 1 END
    ) AS DueCards,
    COUNT(c.Id) AS TotalCards
FROM Stacks s
    JOIN Cards c ON s.Id = c.StackId
GROUP BY s.Name;
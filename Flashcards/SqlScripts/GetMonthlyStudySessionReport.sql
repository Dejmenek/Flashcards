SELECT
    Name AS StackName,
    ISNULL([January], 0) AS JanuaryNumber,
    ISNULL([February], 0) AS FebruaryNumber,
    ISNULL([March], 0) AS MarchNumber,
    ISNULL([April], 0) AS AprilNumber,
    ISNULL([May], 0) AS MayNumber,
    ISNULL([June], 0) AS JuneNumber,
    ISNULL([July], 0) AS JulyNumber,
    ISNULL([August], 0) AS AugustNumber,
    ISNULL([September], 0) AS SeptemberNumber,
    ISNULL([October], 0) AS OctoberNumber,
    ISNULL([November], 0) AS NovemberNumber,
    ISNULL([December], 0) AS DecemberNumber
FROM (
    SELECT Name, DATENAME(month, Date) AS month, st.Id FROM StudySessions s
    JOIN Stacks st ON s.StackId = st.Id WHERE YEAR(Date) = @Year
) t
PIVOT (
    COUNT(t.Id)
    FOR month IN (
        [January], [February], [March],
        [April], [May], [June], [July],
        [August], [September], [October],
        [November], [December]
    )
) AS pivot_table
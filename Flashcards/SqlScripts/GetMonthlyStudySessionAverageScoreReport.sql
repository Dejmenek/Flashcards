SELECT
    Name AS StackName,
    ISNULL([January], 0) AS JanuaryAverageScore,
    ISNULL([February], 0) AS FebruaryAverageScore,
    ISNULL([March], 0) AS MarchAverageScore,
    ISNULL([April], 0) AS AprilAverageScore,
    ISNULL([May], 0) AS MayAverageScore,
    ISNULL([June], 0) AS JuneAverageScore,
    ISNULL([July], 0) AS JulyAverageScore,
    ISNULL([August], 0) AS AugustAverageScore,
    ISNULL([September], 0) AS SeptemberAverageScore,
    ISNULL([October], 0) AS OctoberAverageScore,
    ISNULL([November], 0) AS NovemberAverageScore,
    ISNULL([December], 0) AS DecemberAverageScore
FROM (
    SELECT Name, DATENAME(month, Date) AS month, ISNULL(Score, 0) AS Score FROM StudySessions s
    JOIN Stacks st ON s.StackId = st.Id WHERE YEAR(Date) = @Year
) t
PIVOT (
    AVG(t.Score)
    FOR month IN (
        [January], [February], [March],
        [April], [May], [June], [July],
        [August], [September], [October],
        [November], [December]
    )
) AS pivot_table
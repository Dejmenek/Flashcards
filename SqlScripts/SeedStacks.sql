IF NOT EXISTS (
    SELECT 1 FROM Stacks
)
BEGIN
    INSERT INTO Stacks (Name) VALUES
    ('Spanish'),
    ('German'),
    ('Polish');
END;
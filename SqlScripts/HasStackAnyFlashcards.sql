IF EXISTS (
    SELECT 1 FROM Stacks s JOIN Flashcards f ON s.Id = f.StackId WHERE s.Id = @Id
)
BEGIN
    SELECT 1;
END
ELSE
BEGIN
    SELECT 0;
END;
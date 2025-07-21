IF NOT EXISTS (
	SELECT * FROM sys.databases WHERE name = 'Flashcards'
)
BEGIN
	CREATE DATABASE Flashcards;
END;
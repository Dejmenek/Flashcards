IF NOT EXISTS (
	SELECT * FROM sys.databases WHERE name = 'Flashcards'
)
BEGIN
	CREATE DATABASE Flashcards COLLATE Latin1_General_100_CI_AS_SC_UTF8;
END;
IF NOT EXISTS (
	SELECT 1 FROM Flashcards
)
BEGIN
	INSERT INTO Flashcards (StackId, Front, Back) VALUES
	(1, 'Hola', 'Hello'),
	(1, '¿Cómo estás?', 'How are you?'),
	(1, 'Gracias', 'Thank you'),
	(2, 'Hallo', 'Hello'),
	(2, 'Wie geht es dir?', 'How are you?'),
	(2, 'Danke', 'Thank you'),
	(3, 'Dzień dobry', 'Good morning'),
	(3, 'Do widzenia', 'Goodbye'),
	(3, 'Proszę', 'Please')
END;
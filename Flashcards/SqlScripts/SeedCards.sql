INSERT INTO Cards (StackId, Front, Back, CardType) VALUES
	(1, 'Hola', 'Hello', 'Flashcard'),
	(1, '¿Cómo estás?', 'How are you?', 'Flashcard'),
	(1, 'Gracias', 'Thank you', 'Flashcard'),
	(2, 'Hallo', 'Hello', 'Flashcard'),
	(2, 'Wie geht es dir?', 'How are you?', 'Flashcard'),
	(2, 'Danke', 'Thank you', 'Flashcard'),
	(3, 'Dzień dobry', 'Good morning', 'Flashcard'),
	(3, 'Do widzenia', 'Goodbye', 'Flashcard'),
	(3, 'Proszę', 'Please', 'Flashcard');

	INSERT INTO Cards (StackId, Question, Choices, Answer, CardType) VALUES
	(3, 'Jakie jest największe miasto w Polsce?', 'Kraków;Warszawa;Gdańsk;Wrocław', 'Warszawa', 'MultipleChoice'),
	(3, 'Które z tych są polskimi rzekami?', 'Wisła;Odra;Ren;Dunaj;Warta', 'Wisła;Odra;Warta', 'MultipleChoice'),
	(3, 'Ile województw ma Polska?', 'Czternaście;Piętnaście;Szesnaście;Siedemnaście', 'Szesnaście', 'MultipleChoice');
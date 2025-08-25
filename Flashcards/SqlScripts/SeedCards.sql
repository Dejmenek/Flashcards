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

INSERT INTO Cards (StackId, ClozeText, CardType) VALUES
    (2, 'Guten {{c1::Morgen}}!', 'Cloze'),
    (2, 'Ich {{c1::heiße}} Anna.', 'Cloze'),
    (2, 'Wie {{c1::geht}} es {{c2::dir}}?', 'Cloze'),
    (2, 'Ich komme aus {{c1::Deutschland}}.', 'Cloze');
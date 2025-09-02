-- Flashcards: Assign Box values (1=daily, 2=every 3 days, 3=every 7 days)
INSERT INTO Cards (StackId, Front, Back, CardType, Box, NextReviewDate) VALUES
	(1, 'Hola', 'Hello', 'Flashcard', 1, GETDATE()),
	(1, '¿Cómo estás?', 'How are you?', 'Flashcard', 1, GETDATE()),
	(1, 'Gracias', 'Thank you', 'Flashcard', 2, DATEADD(day, 3, GETDATE())),
	(2, 'Hallo', 'Hello', 'Flashcard', 1, GETDATE()),
	(2, 'Wie geht es dir?', 'How are you?', 'Flashcard', 2, DATEADD(day, 3, GETDATE())),
	(2, 'Danke', 'Thank you', 'Flashcard', 3, DATEADD(day, 7, GETDATE())),
	(3, 'Dzień dobry', 'Good morning', 'Flashcard', 1, GETDATE()),
	(3, 'Do widzenia', 'Goodbye', 'Flashcard', 2, DATEADD(day, 3, GETDATE())),
	(3, 'Proszę', 'Please', 'Flashcard', 3, DATEADD(day, 7, GETDATE()));

INSERT INTO Cards (StackId, Question, Choices, Answer, CardType, Box, NextReviewDate) VALUES
    (3, 'Jakie jest największe miasto w Polsce?', 'Kraków;Warszawa;Gdańsk;Wrocław', 'Warszawa', 'MultipleChoice', 1, GETDATE()),
	(3, 'Które z tych są polskimi rzekami?', 'Wisła;Odra;Ren;Dunaj;Warta', 'Wisła;Odra;Warta', 'MultipleChoice', 2, DATEADD(day, 3, GETDATE())),
	(3, 'Ile województw ma Polska?', 'Czternaście;Piętnaście;Szesnaście;Siedemnaście', 'Szesnaście', 'MultipleChoice', 3, DATEADD(day, 7, GETDATE()));

INSERT INTO Cards (StackId, ClozeText, CardType, Box, NextReviewDate) VALUES
    (2, 'Guten {{c1::Morgen}}!', 'Cloze', 1, GETDATE()),
    (2, 'Ich {{c1::heiße}} Anna.', 'Cloze', 2, DATEADD(day, 3, GETDATE())),
    (2, 'Wie {{c1::geht}} es {{c2::dir}}?', 'Cloze', 3, DATEADD(day, 7, GETDATE())),
    (2, 'Ich komme aus {{c1::Deutschland}}.', 'Cloze', 1, GETDATE());
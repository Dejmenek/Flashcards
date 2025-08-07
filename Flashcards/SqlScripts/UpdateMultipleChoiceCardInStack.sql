UPDATE Cards
SET Question = @Question, Choices = @Choices, Answer = @Answer
WHERE Id = @Id AND StackId = @StackId
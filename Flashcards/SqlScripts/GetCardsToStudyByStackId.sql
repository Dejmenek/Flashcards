SELECT * FROM Cards
WHERE StackId = @StackId AND NextReviewDate <= GETDATE();
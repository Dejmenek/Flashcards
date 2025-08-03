UPDATE Cards
SET Front = @Front, Back = @Back
WHERE Id = @Id AND StackId = @StackId
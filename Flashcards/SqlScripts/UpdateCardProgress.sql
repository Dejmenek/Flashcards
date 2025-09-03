UPDATE Cards
SET Box = @Box, NextReviewDate = @NextReviewDate
WHERE Id = @Id;
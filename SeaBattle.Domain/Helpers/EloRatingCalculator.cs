namespace SeaBattle.Domain.Helpers;

public static class EloRatingCalculator
{
    public static (int PlayerOneNewRating, int PlayerTwoNewRating) CalculateElo(
        int playerOneRating, int playerTwoRating, bool playerOneWon)
    {
        const int kFactor = 32;

        double playerOneExpected = 1 / (1 + Math.Pow(10, (playerTwoRating - playerOneRating) / 400.0));
        double playerTwoExpected = 1 / (1 + Math.Pow(10, (playerOneRating - playerTwoRating) / 400.0));

        int playerOneScore = playerOneWon ? 1 : 0;
        int playerTwoScore = playerOneWon ? 0 : 1;

        int playerOneNewRating = (int)(playerOneRating + kFactor * (playerOneScore - playerOneExpected));
        int playerTwoNewRating = (int)(playerTwoRating + kFactor * (playerTwoScore - playerTwoExpected));

        return (playerOneNewRating, playerTwoNewRating);
    }
}
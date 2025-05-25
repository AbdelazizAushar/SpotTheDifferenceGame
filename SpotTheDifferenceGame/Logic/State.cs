using System;
namespace SpotTheDifferenceGame.Logic
{
    public class State
    {
        public int CurrentLevel { get; private set; } = 1;
        public int MaxLevels { get; private set; }
        public int TotalDifferences { get; private set; }
        public int FoundDifferences { get; private set; } = 0;
        public int MaxAttempts { get; private set; }
        public int RemainingAttempts { get; private set; }
        public int TotalScore { get; private set; } = 0;
        public bool IsGameOver { get; private set; } = false;
        public string GameMode { get; set; } = "Attempts";

        public State(int totalDifferences = 5, int maxAttempts = 10, int maxLevels = 6)
        {
            TotalDifferences = totalDifferences;
            MaxAttempts = maxAttempts;
            MaxLevels = maxLevels;
            ResetLevel();
        }

        // New method to set total differences dynamically
        public void SetTotalDifferences(int count)
        {
            TotalDifferences = Math.Max(1, count); // Ensure at least 1 difference
        }

        public void ResetLevel()
        {
            FoundDifferences = 0;
            RemainingAttempts = MaxAttempts;
            IsGameOver = false;
        }

        public bool RegisterCorrect()
        {
            FoundDifferences++;
            TotalScore += 10; // award points for each correct spot
            
            // Level complete when all differences are found
            if (FoundDifferences >= TotalDifferences)
                return true;
            return false;
        }

        public bool RegisterWrong()
        {
            if (GameMode == "Attempts")
            {
                RemainingAttempts--;
                if (RemainingAttempts <= 0)
                {
                    IsGameOver = true;
                    return true;
                }
            }
            return false;
        }

        public bool AdvanceLevel()
        {
            CurrentLevel++;
            if (CurrentLevel > MaxLevels)
            {
                IsGameOver = true;
                return false;
            }
            ResetLevel();
            return true;
        }
    }
}
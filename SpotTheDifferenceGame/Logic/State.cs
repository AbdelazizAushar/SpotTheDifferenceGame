using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotTheDifferenceGame.Logic
{
    class State
    {
        public int CurrentLevel { get; private set; } = 1;
        public int MaxLevels { get; private set; }
        public int TotalDifferences { get; private set; }

        public int FoundDifferences { get; private set; } = 0;

        public int MaxAttempts { get; private set; }
        public int RemainingAttempts { get; private set; }

        public State(int totalDifferences = 5, int maxAttempts = 10, int maxLevels = 6)
        {
            TotalDifferences = totalDifferences;
            MaxAttempts = maxAttempts;
            MaxLevels = maxLevels;
            ResetLevel();
        }

        public void ResetLevel()
        {
            FoundDifferences = 0;
            RemainingAttempts = MaxAttempts;
        }

        public bool RegisterCorrect()
        {
            FoundDifferences++;
            return FoundDifferences >= TotalDifferences;
        }

        public bool RegisterWrong()
        {
            RemainingAttempts--;
            return RemainingAttempts <= 0;
        }

        public bool AdvanceLevel()
        {
            CurrentLevel++;
            if (CurrentLevel > MaxLevels)
                return false;

            ResetLevel();
            return true;
        }
    }
}

using Protosweeper.Web.Exceptions;
using Protosweeper.Web.Models;

namespace Protosweeper.Web;

public static class Definitions
{
    private static readonly XyPair BeginnerDimensions = new(9, 9);
    private static readonly XyPair IntermediateDimensions = new(16, 16);
    private static readonly XyPair ExpertDimensions = new(30, 16);

    public static XyPair GetDimensions(Difficulty difficulty) =>
        difficulty switch
        {
            Difficulty.Beginner => BeginnerDimensions,
            Difficulty.Intermediate => IntermediateDimensions,
            Difficulty.Expert => ExpertDimensions,
            _ => throw new InvalidDifficultyException(difficulty),
        };

    private const int BeginnerMineCount = 10;
    private const int IntermediateMineCount = 40;
    private const int ExpertMineCount = 99;

    public static int GetMineCount(Difficulty difficulty) =>
        difficulty switch
        {
            Difficulty.Beginner => BeginnerMineCount,
            Difficulty.Intermediate => IntermediateMineCount,
            Difficulty.Expert => ExpertMineCount,
            _ => throw new InvalidDifficultyException(difficulty)
        };
    
    public static Difficulty ParseDifficulty(string difficulty) =>
        difficulty.ToLowerInvariant() switch
        {
            "beginner" => Difficulty.Beginner,
            "intermediate" => Difficulty.Intermediate,
            "expert" => Difficulty.Expert,
            _ => Difficulty.Beginner,
        };
}
using Protosweeper.Core.Models;

namespace Protosweeper.Core.Exceptions;

public class InvalidDifficultyException(Difficulty difficulty) : Exception(Enum.GetName(difficulty));

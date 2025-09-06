using Protosweeper.Api.Models;

namespace Protosweeper.Api.Exceptions;

public class InvalidDifficultyException(Difficulty difficulty) : Exception(Enum.GetName(difficulty));

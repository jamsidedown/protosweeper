using Protosweeper.Web.Models;

namespace Protosweeper.Web.Exceptions;

public class InvalidDifficultyException(Difficulty difficulty) : Exception(Enum.GetName(difficulty));

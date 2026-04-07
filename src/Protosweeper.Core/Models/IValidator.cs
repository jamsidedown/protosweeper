namespace Protosweeper.Core.Models;

public interface IValidator
{
    bool IsSolvable(string gameId);
}
using Protosweeper.Validator;

if (args.Length < 1)
{
    Console.WriteLine("Game ID required");
    return;
}

var gameId = args[^1];
var validator = new Validator();
var result = validator.IsSolvable(gameId) ? "yes" : "no";
Console.WriteLine(result);

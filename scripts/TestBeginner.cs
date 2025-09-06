#:property TargetFramework=net10.0
#:project ../src/Protosweeper.Web/Protosweeper.Web.csproj

using Protosweeper.Web.Models;

var game = GameBoard.Generate(Difficulty.Expert, new XyPair(4, 4));

GameBoard.Print(game.Cells);

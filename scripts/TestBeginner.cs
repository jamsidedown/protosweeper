#:property TargetFramework=net10.0
#:project ../src/Protosweeper.Api/Protosweeper.Api.csproj

using Protosweeper.Api.Models;

var game = GameBoard.Generate(Difficulty.Expert, new XyPair(4, 4));

GameBoard.Print(game.Cells);

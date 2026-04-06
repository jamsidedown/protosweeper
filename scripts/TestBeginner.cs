#:property TargetFramework=net10.0
#:project ../src/Protosweeper.Core/Protosweeper.Core.csproj

using Protosweeper.Core.Models;

var game = GameBoard.Generate(Difficulty.Expert, new XyPair(4, 4));

GameBoard.Print(game.Cells);


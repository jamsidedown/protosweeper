using System.Text;

namespace Protosweeper.Web.Models;

public class GameBoard
{
    public int[,] Cells { get; private set; } = new int[0, 0];

    public static GameBoard Generate(Difficulty difficulty, XyPair initialClick)
    {
        var dimensions = Definitions.GetDimensions(difficulty);
        var mineCount = Definitions.GetMineCount(difficulty);

        var coords = Enumerable.Range(0, dimensions.X)
            .SelectMany(x => Enumerable.Range(0, dimensions.Y).Select(y => new XyPair(x, y)))
            .ToHashSet();

        var safe = GetNeighbours(dimensions, initialClick);

        var mines = coords.Except(safe).Shuffle().Take(mineCount).ToArray();

        return new GameBoard
        {
            Cells = GetCells(dimensions, mines),
        };
    }

    private static HashSet<XyPair> GetNeighbours(XyPair dimensions, XyPair cell)
        => GetNeighbours(dimensions, cell.X, cell.Y);
    
    private static HashSet<XyPair> GetNeighbours(XyPair dimensions, int x, int y)
    {
        var neighbours = new HashSet<XyPair>();

        for (var dx = Math.Max(x - 1, 0); dx < Math.Min(x + 2, dimensions.X); dx++)
        for (var dy = Math.Max(y - 1, 0); dy < Math.Min(y + 2, dimensions.Y); dy++)
            neighbours.Add(new XyPair(dx, dy));

        return neighbours;
    }

    private static int[,] GetCells(XyPair dimensions, XyPair[] mines)
    {
        var cells = new int[dimensions.X, dimensions.Y];

        foreach (var mine in mines)
            cells[mine.X, mine.Y] = -1;

        for (var x = 0; x < dimensions.X; x++)
        {
            for (var y = 0; y < dimensions.Y; y++)
            {
                if (cells[x, y] == -1)
                    continue;

                var neighbours = GetNeighbours(dimensions, x, y);
                var count = neighbours.Count(neighbour => cells[neighbour.X, neighbour.Y] == -1);

                cells[x, y] = count;
            }
        }

        return cells;
    }

    public static void Print(int[,] cells)
    {

        for (var y = 0; y < cells.GetLength(1); y++)
        {
            var line = new StringBuilder();

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                var character = cells[x, y] switch
                {
                    -1 => "X",
                    0 => ".",
                    _ => $"{cells[x, y]}",
                };
                line.Append(character);
            }
            
            Console.WriteLine(line.ToString());
        }
    }
}
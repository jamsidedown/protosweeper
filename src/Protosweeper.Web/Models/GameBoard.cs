using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Protosweeper.Web.Models;

public class GameBoard
{
    public HashSet<XyPair> Mines { get; private set; }
    public HashSet<XyPair> Clear { get; private set; }
    public HashSet<XyPair> Flagged { get; private set; } = [];
    public HashSet<XyPair> Cleared { get; private set; } = [];
    public int[,] Cells { get; private set; } = new int[0, 0];
    public XyPair InitialClick { get; private set; }
    public XyPair Dimensions { get; private set; }
    public bool ReadOnly = false;

    public static GameBoard Generate(Difficulty difficulty, XyPair initialClick)
    {
        var dimensions = Definitions.GetDimensions(difficulty);
        var mineCount = Definitions.GetMineCount(difficulty);

        var coords = Enumerable.Range(0, dimensions.X)
            .SelectMany(x => Enumerable.Range(0, dimensions.Y).Select(y => new XyPair(x, y)))
            .ToHashSet();

        var safe = GetNeighbours(dimensions, initialClick);

        var mines = coords.Except(safe).Shuffle().Take(mineCount).ToHashSet();
        var clear = coords.Except(mines).ToHashSet();

        return new GameBoard
        {
            Cells = GetCells(dimensions, mines.ToArray()),
            Dimensions = dimensions,
            InitialClick = initialClick,
            Mines = mines,
            Clear = clear,
        };
    }

    public IEnumerable<IGameResponse> Click(GameRequestClick click)
    {
        if (ReadOnly)
            yield break;
        
        var x = click.X;
        var y = click.Y;
        var coord = new XyPair(x, y);

        if (click.Button.ToLower() == "left")
        {
            if (Flagged.Contains(coord))
                yield break;
            
            foreach (var response in Reveal(x, y))
                yield return response;
        }
        else if (click.Button.ToLower() == "right")
        {
            if (!Flagged.Contains(coord) && !Cleared.Contains(coord))
            {
                Flagged.Add(coord);
                yield return new FlagResponse { X = x, Y = y };
                yield return new ProgressResponse { Flagged = $"{Flagged.Count} / {Mines.Count}" };
            }
            else if (Flagged.Contains(coord))
            {
                Flagged.Remove(coord);
                yield return new UnflagResponse { X = x, Y = y };
                yield return new ProgressResponse { Flagged = $"{Flagged.Count} / {Mines.Count}" };
            }
        }
    }

    private IEnumerable<IGameResponse> Reveal(int x, int y)
    {
        var cell = Cells[x, y];
        var coord = new XyPair(x, y);

        if (cell == -1)
        {
            yield return new LoseResponse();
            foreach (var mine in Mines.OrderBy(coord.PixelDistance))
            {
                Thread.Sleep(20);
                yield return new MineResponse { X = mine.X, Y = mine.Y };
            }

            yield break;
        }

        var wasCleared = Cleared.Contains(coord);
        yield return new CellResponse { X = x, Y = y, Count = cell };
        Cleared.Add(coord);

        if (Won())
        {
            yield return new WinResponse();
            yield break;
        }
        
        if ((cell == 0 && !wasCleared) || (cell != 0 && wasCleared && CountFlaggedNeighbours(x, y) == cell))
        {
            foreach (var neighbour in GetNeighbours(Dimensions, x, y).Where(n => !Flagged.Contains(n) && !Cleared.Contains(n)))
            foreach (var response in Reveal(neighbour.X, neighbour.Y))
                yield return response;
        }
    }

    private bool Won() => Cleared.SetEquals(Clear);

    private int CountFlaggedNeighbours(int x, int y)
    {
        return GetNeighbours(Dimensions, x, y).Count(n => Flagged.Contains(n));
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
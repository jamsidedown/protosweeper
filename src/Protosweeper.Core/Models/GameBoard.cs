using System.Collections.Concurrent;
using System.Text;
using System.Xml.XPath;
using Protosweeper.Core.Extensions;

namespace Protosweeper.Core.Models;

public class GameBoard
{
    public required HashSet<XyPair> Mines { get; init; }
    public required HashSet<XyPair> Clear { get; init; }
    public HashSet<XyPair> Flagged { get; private set; } = [];
    public HashSet<XyPair> Cleared { get; private set; } = [];
    public int[,] Cells { get; init; } = new int[0, 0];
    public XyPair InitialClick { get; private init; }
    public XyPair Dimensions { get; private init; }
    public bool ReadOnly = false;
    public required ConcurrentBag<IGameEvent> Events { get; init; }
    public DateTime LastEvent { get; private set; }
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public static GameBoard Generate(Guid seed, Difficulty difficulty, XyPair initialClick)
    {
        var dimensions = Definitions.GetDimensions(difficulty);
        var mineCount = Definitions.GetMineCount(difficulty);

        var coords = Enumerable.Range(0, dimensions.X)
            .SelectMany(x => Enumerable.Range(0, dimensions.Y).Select(y => new XyPair(x, y)))
            .ToHashSet();

        var safe = GetNeighbours(dimensions, initialClick);

        var nonSafe = coords.Except(safe).ToArray();
        Random.CreateSeeded(seed).Shuffle(nonSafe);
        var mines = nonSafe.Take(mineCount).ToHashSet();
        var clear = coords.Except(mines).ToHashSet();

        return new GameBoard
        {
            Cells = GetCells(dimensions, mines.ToArray()),
            Dimensions = dimensions,
            InitialClick = initialClick,
            Mines = mines,
            Clear = clear,
            Events = [new GameRequestClick { Button = "left", X = initialClick.X, Y = initialClick.Y }],
            LastEvent = DateTime.Now,
        };
    }

    public async IAsyncEnumerable<IGameResponse> Click(GameRequestClick click, CancellationToken token)
    {
        if (ReadOnly)
            yield break;

        await Semaphore.WaitAsync(token);

        try
        {
            Events.Add(click);
            LastEvent = DateTime.Now;
        
            var x = click.X;
            var y = click.Y;
            var coord = new XyPair(x, y);

            if (coord.X < 0 || coord.X >= Dimensions.X || coord.Y < 0 || coord.Y >= Dimensions.Y)
                yield break;

            if (click.Button.ToLower() == "left")
            {
                if (Flagged.Contains(coord))
                    yield break;

                foreach (var response in Reveal(x, y))
                {
                    Events.Add(response);
                    yield return response;
                }
            }
            else if (click.Button.ToLower() == "right")
            {
                if (!Flagged.Contains(coord) && !Cleared.Contains(coord))
                {
                    Flagged.Add(coord);
                
                    var flagResponse = new FlagResponse { X = x, Y = y };
                    Events.Add(flagResponse);
                    yield return flagResponse;
                    
                    var progressResponse = new ProgressResponse { Flagged = $"{Flagged.Count} / {Mines.Count}" };
                    Events.Add(progressResponse);
                    yield return progressResponse;
                }
                else if (Flagged.Contains(coord))
                {
                    Flagged.Remove(coord);
                
                    var unflagResponse = new UnflagResponse { X = x, Y = y };
                    Events.Add(unflagResponse);
                    yield return unflagResponse;
                
                    var progressResponse = new ProgressResponse { Flagged = $"{Flagged.Count} / {Mines.Count}" };
                    Events.Add(progressResponse);
                    yield return progressResponse;
                }
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public IEnumerable<IGameResponse> ReplayResponses()
    {
        return Events.Where(e => e is IGameResponse)
            .OrderBy(e => e.Timestamp)
            .Cast<IGameResponse>();
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
            yield return new ProgressResponse { Flagged = $"{Mines.Count} / {Mines.Count}" };
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
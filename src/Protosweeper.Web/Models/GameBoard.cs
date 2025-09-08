using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Protosweeper.Web.Models;

public class GameBoard
{
    public required Channel<IGameRequest> Requests { get; init; }
    public required Channel<IGameResponse> Responses { get; init; }
    
    public int[,] Cells { get; private set; } = new int[0, 0];
    public Cell[,] States { get; private set; } = new Cell[0, 0];
    public XyPair InitialClick { get; private set; }
    public XyPair Dimensions { get; private set; }

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
            States = new Cell[dimensions.X, dimensions.Y],
            Dimensions = dimensions,
            InitialClick = initialClick,
            Requests = Channel.CreateBounded<IGameRequest>(256),
            Responses = Channel.CreateBounded<IGameResponse>(256),
        };
    }

    public async Task Run(CancellationToken token)
    {
        var receiver = Requests.Reader;
        var sender = Responses.Writer;

        try
        {
            foreach (var response in Click(new GameRequestClick
                         { Button = "left", X = InitialClick.X, Y = InitialClick.Y }))
            {
                await sender.WriteAsync(response, token);
            }

            await foreach (var request in receiver.ReadAllAsync(token))
            {
                var responses = request switch
                {
                    GameRequestClick click => Click(click),
                    _ => [],
                };

                foreach (var response in responses)
                {
                    await sender.WriteAsync(response, token);
                }
            }
        }
        catch (OperationCanceledException _)
        {
            Console.WriteLine("Cancellation caught in Run");
        }
    }

    private IEnumerable<IGameResponse> Click(GameRequestClick click)
    {
        var x = click.X;
        var y = click.Y;
        
        Console.WriteLine(JsonSerializer.Serialize(click));

        if (click.Button.ToLower() == "left")
        {
            if (States[x, y] == Cell.Flagged)
                yield break;
            
            foreach (var response in Reveal(x, y))
                yield return response;
        }
        else if (click.Button.ToLower() == "right")
        {
            var state = States[x, y];
            if (state == Cell.Hidden)
            {
                States[x, y] = Cell.Flagged;
                yield return new FlagResponse { X = x, Y = y };
            }
            else if (state == Cell.Flagged)
            {
                States[x, y] = Cell.Hidden;
                yield return new UnflagResponse { X = x, Y = y };
            }
        }
    }

    private IEnumerable<IGameResponse> Reveal(int x, int y)
    {
        var cell = Cells[x, y];
        var initialState = States[x, y];

        if (cell == -1)
        {
            yield return new MineResponse { X = x, Y = y };
            States[x, y] = Cell.Exploded;
            yield break;
        }
        
        yield return new CellResponse { X = x, Y = y, Count = cell };
        States[x, y] = Cell.Revealed;

        if ((cell == 0 && initialState == Cell.Hidden) || (cell != 0 && initialState == Cell.Revealed && CountFlaggedNeighbours(x, y) == cell))
        {
            foreach (var neighbour in GetNeighbours(Dimensions, x, y).Where(n => States[n.X, n.Y] == Cell.Hidden))
            foreach (var response in Reveal(neighbour.X, neighbour.Y))
                yield return response;
        }
    }

    private int CountFlaggedNeighbours(int x, int y)
    {
        return GetNeighbours(Dimensions, x, y).Count(n => States[n.X, n.Y] == Cell.Flagged);
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
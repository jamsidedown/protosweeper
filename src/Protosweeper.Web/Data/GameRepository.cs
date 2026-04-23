using Protosweeper.Web.Models;

namespace Protosweeper.Web.Data;

public class GameRepository(GameDbContext db, ILogger<GameRepository> logger)
{
    private static SemaphoreSlim _lock = new(1);
    
    public async Task<GameEntity> Create(GameEntity entity, CancellationToken token)
    {
        await _lock.WaitAsync(token);

        try
        {
            await db.Games.AddAsync(entity, token);
            await db.SaveChangesAsync(token);
            return entity;
        }
        catch (Exception e)
        {
            logger.LogError("{}", e);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<(bool, GameEntity?)> TryGet(Guid id, CancellationToken token)
    {
        var entity = await db.Games.FindAsync([id], cancellationToken: token);
        return (entity is not null, entity);
    }
}

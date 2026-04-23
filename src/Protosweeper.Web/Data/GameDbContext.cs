using Microsoft.EntityFrameworkCore;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Data;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<GameEntity> Games => Set<GameEntity>();
}

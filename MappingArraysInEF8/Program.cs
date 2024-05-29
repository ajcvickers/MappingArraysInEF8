using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using (var context = new SomeDbContext())
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    context.AddRange(new Post
        {
            Title = "Arrays in EF Core 8",
            Contents = "Imagine you want...",
            Tags = ["EF Core", "Entity Framework", ".NET", "Databases"],
            Visits = [DateTime.UtcNow - TimeSpan.FromDays(1), DateTime.UtcNow - TimeSpan.FromDays(2)]
        },
        new Post
        {
            Title = "What’s new in Orleans 8",
            Contents = "Let's take a look at ...",
            Tags = ["Orleans", ".NET", "ASP.NET Core"],
            Visits = [DateTime.UtcNow, DateTime.UtcNow - TimeSpan.FromDays(1), DateTime.UtcNow - TimeSpan.FromDays(2)]
        },
        new Post
        {
            Title = ".NET at Build",
            Contents = "Get ready for a ",
            Tags = [".NET", "ASP.NET Core"],
            Visits = [DateTime.UtcNow - TimeSpan.FromDays(2)]
        });

    await context.SaveChangesAsync();
}

using (var context = new SomeDbContext())
{
    var postTags = await context.Posts
        .Select(post => new
        {
            PostTitle = post.Title,
            FirstTag = post.Tags[0],
            SecondTag = post.Tags[1]
        }).ToListAsync();
    
    var tag = "EF Core";
    var posts = await context.Posts
        .Where(post => post.Tags.Contains(tag))
        .ToListAsync();
    
    var year = DateTime.UtcNow.Year;
    var visited = await context.Posts
        .Where(post => post.Visits.Any(v => v.Year == year))
        .ToListAsync();

    var prefixes = new[] {"What's new", "Getting started", "Intro to"};
    await context.Posts
        .Where(post => prefixes.Any(prefix => post.Title.StartsWith(prefix)))
        .ToListAsync();

    var tags = new[] {".NET", "ASP.NET Core"};
    await context.Posts
        .Where(post => tags.Any(tag => post.Tags.Contains(tag)))
        .ToListAsync();
}

public class SomeDbContext : DbContext
{
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            // Uncomment one line below to use SQL Server, SQLite, or PostgreSQL.
            .UseSqlServer("Data Source=localhost;Database=BuildBlogs;Integrated Security=True;Trust Server Certificate=True;ConnectRetryCount=0")
            //.UseSqlite(@"Data Source=db.dat")
            //.UseNpgsql("Server=localhost;Database=AspireTest;User ID=postgres;Password=clippy77i@")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>().PrimitiveCollection(e => e.Tags).IsUnicode(false).HasMaxLength(2000);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Contents { get; set; }
    public string[] Tags { get; set; }
    public DateTime[] Visits { get; set; }
}

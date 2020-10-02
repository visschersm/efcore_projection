using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Collections.Immutable;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
var connectionString = "Data Source=blogging.db";
//var connectionString = connectionStringBuilder.ToString();
var connection = new SqliteConnection(connectionString);

//connection.CreateFunction("getdate", () => DateTime.Now);

var options = new DbContextOptionsBuilder<MyContext>()
    .UseSqlite(connection)
    .Options;

using var context = new MyContext(options);

context.Users.AddRange(new User[] 
{
    new User
    {
        Username = "HarryP",
        Email = "harry.potter@hogwarts.uk",
        Password = "expelliarmus",
        Blogs = new List<Blog> 
        {
            new Blog
            {
                Title = "My biggest accomplishments",
                Posts = new List<Post> 
                {
                    new Post(),
                    new Post()
                }
            }
        }
    },
    new User
    {
        Username = "RonaldW",
        Email = "ronald.weasley@hogwarts.uk",
        Password = "hermonie",
        Blogs = new List<Blog>
        {
            new Blog
            {
                Title = "Why Quidditch is awesome",
                Posts = new List<Post> 
                {
                    new Post(),
                    new Post()
                }
            },
            new Blog
            {
                Title = "Being friends with Harry Potter"
            }
        }
    },
    new User
    {
        Username = "HermonieG",
        Email = "hermonie.granger@hogwarts.uk",
        Password = "idJcZF68uqx4sn3F47S7a7dDJqDM7F",
        Blogs = new List<Blog>
        {
            new Blog
            {
                Title = "Proof of alternate universes",
                Posts = new List<Post> 
                {
                    new Post(), 
                    new Post(),
                    new Post(),
                }
            },
            new Blog
            {
                Title = "Missed muggle influences in the magical world",
                Posts = new List<Post>
                {
                    new Post(),
                    new Post(),
                    new Post(),
                    new Post()
                }
            }
        }
    }
});

context.SaveChanges();

// TODO:    Find a nice way to project entities 
//          with foreign relations to views.
var blog = context.Blogs.AsNoTracking()
    .Where(blog => blog.Id == 1)
    //.Select(BlogMapper.Projection)
    .SingleOrDefault();



public class MyContext : DbContext
{
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();

    public MyContext()
        : base() 
    {

    }

    public MyContext(DbContextOptions options)
        : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        if(!builder.IsConfigured)
        {
            builder.UseSqlite("Data Source=blogging.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Username)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.Password)
                .IsRequired();
        });
        
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.Property(blog => blog.Title)
                .IsRequired();
            
            // entity.Property(blog => blog.CreatedDate)
            //     .HasDefaultValueSql("getdate()");
            
            entity.HasOne(blog => blog.CreatedBy)
                .WithMany(user => user.Blogs)
                .HasForeignKey(blog => blog.CreatedById);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            //entity.Property(post => post.CreatedDate)
            //    .HasDefaultValueSql("getdate()");

            entity.HasOne(post => post.Blog)
                .WithMany(blog => blog.Posts)
                .HasForeignKey(post => post.BlogId);
        });
    }
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public List<Post> Posts { get; set; } = new List<Post>();
}

public class Post
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public int BlogId { get; set; }
    public Blog Blog { get; set; } = null!;
    
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public List<Blog> Blogs { get; set; } = new List<Blog>();
}

// public static class BlogMapper
// {
//     public Expression<Func<Blog, BlogView>> Projection { get; set; }
// }
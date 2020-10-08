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
var connectionString = "Data Source=hogwarts.db";
//var connectionString = connectionStringBuilder.ToString();
var connection = new SqliteConnection(connectionString);

//connection.CreateFunction("getdate", () => DateTime.Now);

var options = new DbContextOptionsBuilder<MyContext>()
    .UseSqlite(connection)
    .Options;

using var context = new MyContext(options);

context.Database.EnsureDeleted();

var gripphendor = context.Houses.Add(new House
{
    Name = "Gripphendor"
}).Entity;

context.SaveChanges();

var teacher1 = context.Persons.Add(new Person
{
    FirstName = "...",
    LastName = "Magonagol"
}).Entity;

context.SaveChanges();

var student1 = context.Persons.Add(new Person
{
    FirstName = "Harry",
    LastName = "Potter",
    HouseId = gripphendor.Id
}).Entity;

context.SaveChanges();

Console.WriteLine("The one who lived");

context.Persons.AddRange(new Person[] 
{
    new Person
    {
        FirstName = "Ronald",
        LastName = "Weasley",
        HouseId = gripphendor.Id
    },
    new Person 
    {
        FirstName = "Hermonie",
        LastName = "Granger",
        HouseId = gripphendor.Id
    }
});

var course1 = context.Courses.Add(new Course
{
    Title = "Transfiguration",
    ShortCode = "TFG"
}).Entity;

context.TimeTables.AddRange(new TimeTable[] 
{
    new TimeTable
    {
        TeacherId = teacher1.Id,
        StudentId = student1.Id,
        CourseId = course1.Id,
    }
});

context.SaveChanges();

public class MyContext : DbContext
{
    public DbSet<House> Houses => Set<House>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TimeTable> TimeTables => Set<TimeTable>();

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
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(person => person.House)
                .WithMany(house => house.Persons)
                .HasForeignKey(person => person.HouseId);
        });

        modelBuilder.Entity<TimeTable>(entity =>
        {
            entity.HasKey(timeTable => new 
            {
                timeTable.TeacherId,
                timeTable.StudentId, 
                timeTable.CourseId, 
                timeTable.DateTime
            });
        });
    }
}

public class House
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public ICollection<Person> Persons { get; set; }
}

public class Person 
{
    public Guid Id { get; set; }

    public string FirstName { get; set;}
    public string? MiddleName { get; set; }
    public string? LastNamePrefix { get; set; }
    public string LastName { get; set; }

    public Guid HouseId { get; set; }
    public House House { get; set; }
}

public class Course 
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ShortCode { get; set; }
}

public class TimeTable
{
    public Guid TeacherId { get; set; }
    public Person Teacher { get; set; }
    public Guid StudentId { get; set; }
    public Person Student { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
    public DateTime DateTime { get; set; }
}
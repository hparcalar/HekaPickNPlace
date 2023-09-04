using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace PickNPlace.DataAccess
{
    public static class SchemaFactory
    {
        public static string ConnectionString { get; set; } = "Host=localhost;Database=hekapnp;Username=postgres;Password=postgres;";
        public static HekaDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            #region PGSQL
            optionsBuilder.UseNpgsql(ConnectionString);
            #endregion
            HekaDbContext nodeContext = new HekaDbContext(optionsBuilder.Options);
            return nodeContext;
        }

        public static void ApplyMigrations()
        {
            var nodeContext = CreateContext();
            if (nodeContext != null)
            {
                try
                {
                    nodeContext.Database.Migrate();
                    nodeContext.Dispose();

                    Console.WriteLine("Migration Succeeded");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Migration Error: " + ex.Message);
                }
            }
        }
    }
}

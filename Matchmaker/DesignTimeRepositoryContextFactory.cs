using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Matchmaker
{
    public class DesignTimeRepositoryContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    {
        public GameDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new GameDbContext(optionsBuilder.Options);
        }
    }
}

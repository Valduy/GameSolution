using System;
using System.Collections.Generic;
using System.Text;
using DBRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBRepository.Factories
{
    public class RepositoryContextFactory : IRepositoryContextFactory
    {
        public RepositoryContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RepositoryContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new RepositoryContext(optionsBuilder.Options);
        }
    }
}

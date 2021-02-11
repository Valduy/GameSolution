using System;
using System.Collections.Generic;
using System.Text;

namespace DBRepository.Interfaces
{
    public interface IRepositoryContextFactory
    {
        RepositoryContext CreateDbContext(string connectionString);
    }
}

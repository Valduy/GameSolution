using System;
using System.Collections.Generic;
using System.Text;
using DBRepository.Factories;
using DBRepository.Interfaces;

namespace DBRepository.Repositories
{
    public abstract class RepositoryBase
    {
        protected string ConnectionString { get; }
        protected IRepositoryContextFactory ContextFactory { get; }

        public RepositoryBase(string connectionString, IRepositoryContextFactory contextFactory)
        {
            ConnectionString = connectionString;
            ContextFactory = contextFactory;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DataBase.EF
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
    : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<ContractTemplate> ContractTemplate { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("employee","AgentManage");
            modelBuilder.Entity<Contract>().ToTable("contract", "AgentManage");
            modelBuilder.Entity<Customer>().ToTable("customer", "AgentManage"); 
            modelBuilder.Entity<ContractTemplate>().ToTable("contracttemplate", "AgentManage");
            modelBuilder.Entity<Customer>().HasMany(n => n.Contracts).WithOne().HasPrincipalKey(p => p.CustomerId);

        }
    }
}

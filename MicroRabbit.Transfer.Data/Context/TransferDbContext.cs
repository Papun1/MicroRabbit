
using MicroRabbit.Transfer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Tansfer.Data.Context
{
    public class TransferDbContext:DbContext
    {
        public TransferDbContext(DbContextOptions options):base(options)
        {

        }
        public DbSet<Transferlog> Account { get; set; }
    }
}

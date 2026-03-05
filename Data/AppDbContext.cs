using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Times.Data
{
    public class AppDbContext : DbContext
    {



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    "server=localhost;port=3306;database=AI_Times;user=root;password=;",
                    new MySqlServerVersion(new Version(8, 0, 30))
                );
            }
        }
    }
}

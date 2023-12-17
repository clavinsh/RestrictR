using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace DataPacketLibrary.Models
{
    internal class RestrictRDbContextFactory : IDesignTimeDbContextFactory<RestrictRDbContext>
    {
        public RestrictRDbContext CreateDbContext(string[] args)
        {
            string databaseFilename = "RestrictR_DB.db";
            string programDataPath = GetFolderPath(SpecialFolder.CommonApplicationData);
            string databaseDirectoryPath = Path.Combine(programDataPath, "RestrictR");

            Directory.CreateDirectory(databaseDirectoryPath);

            string dbPath = Path.Combine(databaseDirectoryPath, databaseFilename);

            string connectionString = $"Data Source={dbPath}";

            var optionsBuilder = new DbContextOptionsBuilder<RestrictRDbContext>();

            optionsBuilder.UseSqlite(connectionString);

            return new RestrictRDbContext(optionsBuilder.Options);
        }
    }
}

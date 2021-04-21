using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MptService.Track.Server.Data
{
    public class ApplicationContext : DbContext
    {       
        public DbSet<Station> Stations { get; set; }

        public DbSet<GpsDatum> GpsData { get; set; }

        public DbSet<Alarm> Alarms { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            //Database.EnsureCreated();            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<GpsDatum>().HasKey(vf => new { vf.StationId, vf.ReceivedTime }); // в таблице для GpsDatum - составной первичный ключ
        }
    }
}

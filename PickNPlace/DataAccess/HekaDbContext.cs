using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickNPlace.DataAccess
{
    public class HekaDbContext : DbContext, IDisposable
    {
        public DbSet<PalletRecipe> PalletRecipe { get; set; }
        public DbSet<PalletRecipeFloor> PalletRecipeFloor { get; set; }
        public DbSet<PalletRecipeFloorItem> PalletRecipeFloorItem { get; set; }
        public DbSet<PlaceRequest> PlaceRequest { get; set; }
        public DbSet<PlaceRequestItem> PlaceRequestItem { get; set; }
        public DbSet<RawMaterial> RawMaterial { get; set; }
        public DbSet<PalletStateLive> PalletStateLive { get; set; }
        public DbSet<SysParam> SysParam { get; set; }

        public HekaDbContext() : base() { }
        public HekaDbContext(Microsoft.EntityFrameworkCore.DbContextOptions options) : base(options) { }
        public new void Dispose()
        {
            base.Dispose();
        }

        protected override void OnConfiguring(
           DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=hekapnp;Username=postgres;Password=postgres;");
        }
    }
}

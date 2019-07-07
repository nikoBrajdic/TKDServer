namespace TKD.App.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TkdModel : DbContext
    {
        public TkdModel() : base("name=TKC") { }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Contestant> Contestants { get; set; }
        public virtual DbSet<Poomsae> Poomsaes { get; set; }
        public virtual DbSet<PoomsaeType> PoomsaeTypes { get; set; }
        public virtual DbSet<Score> Scores { get; set; }
        public virtual DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes11)
                .WithOptional(e => e.Poomsae11)
                .HasForeignKey(e => e.Poomsae11ID);

            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes12)
                .WithOptional(e => e.Poomsae12)
                .HasForeignKey(e => e.Poomsae12ID);

            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes21)
                .WithOptional(e => e.Poomsae21)
                .HasForeignKey(e => e.Poomsae21ID);

            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes22)
                .WithOptional(e => e.Poomsae22)
                .HasForeignKey(e => e.Poomsae22ID);

            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes31)
                .WithOptional(e => e.Poomsae31)
                .HasForeignKey(e => e.Poomsae31ID);

            modelBuilder.Entity<Poomsae>()
                .HasMany(e => e.Poomsaes32)
                .WithOptional(e => e.Poomsae32)
                .HasForeignKey(e => e.Poomsae32ID);
        }
    }
}

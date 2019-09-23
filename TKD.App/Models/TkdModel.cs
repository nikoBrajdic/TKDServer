namespace TKD.App.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TkdModel : DbContext
    {
        public TkdModel() : base("name=TkdModel")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TkdModel, Migrations.Configuration>());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .Ignore(c => c.Poomsae11)
                .Ignore(c => c.Poomsae12)
                .Ignore(c => c.Poomsae21)
                .Ignore(c => c.Poomsae22)
                .Ignore(c => c.Poomsae31)
                .Ignore(c => c.Poomsae32);

            modelBuilder.Entity<Score>()
                .Ignore(s => s.Accuracy1)
                .Ignore(s => s.Accuracy2)
                .Ignore(s => s.Accuracy3)
                .Ignore(s => s.Accuracy4)
                .Ignore(s => s.Accuracy5)
                .Ignore(s => s.Accuracy6)
                .Ignore(s => s.Accuracy7)
                .Ignore(s => s.Accuracy8)
                .Ignore(s => s.Accuracy9)
                .Ignore(s => s.Presentation1)
                .Ignore(s => s.Presentation2)
                .Ignore(s => s.Presentation3)
                .Ignore(s => s.Presentation4)
                .Ignore(s => s.Presentation5)
                .Ignore(s => s.Presentation6)
                .Ignore(s => s.Presentation7)
                .Ignore(s => s.Presentation8)
                .Ignore(s => s.Presentation9)
                .Ignore(s => s.DisMinorMean)
                .Ignore(s => s.DisMinorTotal)
                .Ignore(s => s.DisGrandTotal)
                .Ignore(s => s.DisAccuracyMinorTotal)
                .Ignore(s => s.DisPresentationMinorTotal)
                .Ignore(s => s.DisAccuracyGrandTotal)
                .Ignore(s => s.DisPresentationGrandTotal);

            modelBuilder.Entity<Contestant>()
                .Ignore(c => c.FullName);
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryPoomsae> CategoryPoomsaes { get; set; }
        public virtual DbSet<Contestant> Contestants { get; set; }
        public virtual DbSet<Poomsae> Poomsaes { get; set; }
        public virtual DbSet<PoomsaeType> PoomsaeTypes { get; set; }
        public virtual DbSet<Score> Scores { get; set; }
        public virtual DbSet<SoloScore> SoloScores { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
    }
}

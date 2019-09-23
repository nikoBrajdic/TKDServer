namespace TKD.App.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTo3NF : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ShortName = c.String(nullable: false),
                        IsFreestyle = c.Boolean(nullable: false),
                        CurrentRound = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CategoryPoomsaes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Round = c.Int(nullable: false),
                        Index = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        PoomsaeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Poomsaes", t => t.PoomsaeId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.PoomsaeId);
            
            CreateTable(
                "dbo.Poomsaes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ShortName = c.String(nullable: false),
                        Ordinal = c.String(nullable: false),
                        PoomsaeTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.PoomsaeTypes", t => t.PoomsaeTypeId, cascadeDelete: true)
                .Index(t => t.PoomsaeTypeId);
            
            CreateTable(
                "dbo.PoomsaeTypes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Contestants",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Surname = c.String(nullable: false),
                        TrackPath = c.String(),
                        Active = c.Boolean(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Teams", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.TeamId);
            
            CreateTable(
                "dbo.Scores",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Round = c.Int(nullable: false),
                        Index = c.Int(nullable: false),
                        MinorMean = c.Double(),
                        MinorTotal = c.Double(),
                        GrandTotal = c.Double(),
                        AccuracyMinorTotal = c.Double(),
                        PresentationMinorTotal = c.Double(),
                        AccuracyGrandTotal = c.Double(),
                        PresentationGrandTotal = c.Double(),
                        ContestantId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Contestants", t => t.ContestantId, cascadeDelete: true)
                .Index(t => t.ContestantId);
            
            CreateTable(
                "dbo.SoloScores",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Index = c.Int(nullable: false),
                        Value = c.Double(),
                        ScoreId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Scores", t => t.ScoreId, cascadeDelete: true)
                .Index(t => t.ScoreId);
            
            CreateTable(
                "dbo.Teams",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Contestants", "TeamId", "dbo.Teams");
            DropForeignKey("dbo.SoloScores", "ScoreId", "dbo.Scores");
            DropForeignKey("dbo.Scores", "ContestantId", "dbo.Contestants");
            DropForeignKey("dbo.Contestants", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Poomsaes", "PoomsaeTypeId", "dbo.PoomsaeTypes");
            DropForeignKey("dbo.CategoryPoomsaes", "PoomsaeId", "dbo.Poomsaes");
            DropForeignKey("dbo.CategoryPoomsaes", "CategoryId", "dbo.Categories");
            DropIndex("dbo.SoloScores", new[] { "ScoreId" });
            DropIndex("dbo.Scores", new[] { "ContestantId" });
            DropIndex("dbo.Contestants", new[] { "TeamId" });
            DropIndex("dbo.Contestants", new[] { "CategoryId" });
            DropIndex("dbo.Poomsaes", new[] { "PoomsaeTypeId" });
            DropIndex("dbo.CategoryPoomsaes", new[] { "PoomsaeId" });
            DropIndex("dbo.CategoryPoomsaes", new[] { "CategoryId" });
            DropTable("dbo.Teams");
            DropTable("dbo.SoloScores");
            DropTable("dbo.Scores");
            DropTable("dbo.Contestants");
            DropTable("dbo.PoomsaeTypes");
            DropTable("dbo.Poomsaes");
            DropTable("dbo.CategoryPoomsaes");
            DropTable("dbo.Categories");
        }
    }
}

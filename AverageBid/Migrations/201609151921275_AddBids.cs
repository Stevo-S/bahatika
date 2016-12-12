namespace AverageBid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBids : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.Int(nullable: false),
                        Bidder = c.String(maxLength: 20),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Bids");
        }
    }
}

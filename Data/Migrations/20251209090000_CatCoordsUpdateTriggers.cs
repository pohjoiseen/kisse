using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kisse.Data.Migrations
{
    /// <summary>
    /// Adds triggers on Observations table that maintain coordinates of associated cats
    /// (as average of observations coords).  NOTE!  Triggers will get deleted if Observations table
    /// would be recreated by a later migration (which is necessary with sqlite to modify/remove fields).
    /// </summary>
    public partial class CatCoordsUpdateTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER Observations_InsertUpdateCatCoords AFTER INSERT ON Observations BEGIN
    UPDATE Cats
    SET Lat = AVG(o.Lat), Lng = AVG(o.Lng)
    FROM Observations o
    WHERE Cats.Id = new.CatId AND o.CatId = new.CatId;
END");
            migrationBuilder.Sql(@"
CREATE TRIGGER Observations_UpdateUpdateCatCoords AFTER UPDATE ON Observations BEGIN
    UPDATE Cats
    SET Lat = AVG(o.Lat), Lng = AVG(o.Lng)
    FROM Observations o
    WHERE Cats.Id = new.CatId AND o.CatId = new.CatId;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER Observations_InsertUpdateCatCoords");
            migrationBuilder.Sql("DROP TRIGGER Observations_UpdateUpdateCatCoords");
        }
    }
}

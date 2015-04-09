namespace NeshHouse.Stats.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RetrieveStats : DbMigration
    {
        public override void Up()
        {
            Sql(InstallScript);
        }
        
        public override void Down()
        {
            Sql(UninstallScript);
        }

        private const string InstallScript = @"
            CREATE PROCEDURE [dbo].[stats_RetrieveRankings]
	            @matchup int
            AS
            BEGIN
	            -- SET NOCOUNT ON added to prevent extra result sets from
	            -- interfering with SELECT statements.
	            SET NOCOUNT ON;

                select 
		                Cast(Rank() over ( order by mean desc, ConservativeRating desc, StandardDeviation desc, wins desc, losses desc, total desc) as int) [Rank]
		            ,Cast(Dense_Rank() over ( order by mean desc, ConservativeRating desc, StandardDeviation desc, wins desc, losses desc, total desc) as int) [DenseRank]
		            ,Name [UserName]
		            ,Mean
		            ,StandardDeviation
		            ,ConservativeRating
		            ,Cast(Total  as int) [Total]
		            ,Cast(Wins  as int) [Wins]
		            ,Cast(Losses  as int) [Losses]
	            from
	            (
		            select 
			            Name as Name
		            ,COUNT(id) as Total
		            ,SUM(case when outcome =1 then 1 else 0 end) as Wins
		            ,SUM(case when outcome =0 then 1 else 0 end) as Losses
		            ,Mean
		            ,StandardDeviation
		            ,ConservativeRating
		            from
		            (
			            select
			            name, mean, StandardDeviation, ConservativeRating, gr.Outcome, g.id
			            from  dbo.Games g
			            inner join dbo.GameTeams gt on g.id = gt.GameId
			            inner join dbo.Teams t on gt.TeamId = t.Id
			            inner join dbo.UserTeams ut on ut.TeamId = t.Id
			            inner join dbo.GameResults gr on g.id =gr.GameId and ut.UserName = gr.UserName
			            where  Matchup = @matchup /* Change this parameter to 1 or 2 depending of if its one on one or two on two */
			            group by name, mean, StandardDeviation, ConservativeRating, gr.Outcome, g.id
		            ) as a
		            group by Name, mean, StandardDeviation, ConservativeRating
	
	            ) as temp
            END";

        private const string UninstallScript = @"
            DROP PROCEDURE [dbo].[stats_RetrieveRankings]";
    }
}

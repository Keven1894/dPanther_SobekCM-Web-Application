
/****** Object:  StoredProcedure [dbo].[SobekCM_Get_Codes]    Script Date: 08/19/2013 04:44:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Gets the lists of all item aggregation codes
ALTER PROCEDURE [dbo].[SobekCM_Get_Codes]
AS
begin
	
	-- No need to perform any locks here
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	-- Get the first aggregations
	SELECT Code, [Type], Name, ShortName=isnull(ShortName, Name), isActive, Hidden, AggregationID, [Description]=isnull([Description],''), ThematicHeadingID=isnull(ThematicHeadingID, -1 ), External_URL=ISNULL(External_Link,''), DateAdded
	FROM SobekCM_Item_Aggregation AS P
	WHERE Deleted = 'false'
	order by Code;
	
end;
GO
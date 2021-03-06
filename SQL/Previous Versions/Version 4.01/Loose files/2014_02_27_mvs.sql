

-- Ensure the stored procedure exists
IF object_id('SobekCM_Get_BibID_VID_From_Identifier') IS NULL EXEC ('create procedure dbo.SobekCM_Get_BibID_VID_From_Identifier as select 1;');
GO


ALTER PROCEDURE [dbo].[SobekCM_Get_BibID_VID_From_Identifier]
	@identifier nvarchar(max)	
AS
BEGIN

  SELECT ig.BibID as bibid,i.VID as vid
  FROM dbo.SobekCM_Metadata_Unique_Search_Table as must join dbo.SobekCM_Metadata_Unique_Link as mul on must.MetadataID=mul.MetadataID
	join dbo.SobekCM_Item as i on mul.ItemID=i.ItemID join dbo.SobekCM_Item_Group as ig on i.GroupID = ig.GroupID 
  where must.MetadataTypeID=17 and must.MetadataValue=@identifier;

END;
GO

GRANT EXECUTE ON [dbo].[SobekCM_Get_BibID_VID_From_Identifier] TO ufdc_user;
GO



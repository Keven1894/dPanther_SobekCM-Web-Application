

-- Saves the ticklers for a single digital resource (item)
-- Written by Mark Sullivan 
CREATE PROCEDURE [dbo].[SobekCM_Save_Item_Ticklers]
	@ItemID int,
	@Tickler1 varchar(50),
	@Tickler2 varchar(50),
	@Tickler3 varchar(50),
	@Tickler4 varchar(50),
	@Tickler5 varchar(50)
AS
begin transaction

	-- Delete all links to ticklers for this item
	select L.MetadataID 
	into #TEMP_TICKLERS
	from SobekCM_Metadata_Unique_Link L, SobekCM_Metadata_Unique_Search_Table S
	where ( L.ItemID = @ItemID ) 
	  and ( L.MetadataID = S.MetadataID )
	  and ( S.MetadataTypeID = 20 );
	  
	delete from SobekCM_Metadata_Unique_Link
	where ItemID=@ItemID
	  and MetadataID in ( select * from #TEMP_TICKLERS );
	  
	drop table #TEMP_TICKLERS;
	
	-- Build the tickler to insert into the basic search table as well
	declare @tickler nvarchar(max);
	set @tickler='';
		
	-- Add the first tickler to this item
	if ( len( coalesce( @Tickler1, '' )) > 0 ) 
	begin
		set @tickler=@tickler + ' | ' + @Tickler1;
		exec SobekCM_Metadata_Save_Single @ItemID, 'Tickler', @Tickler1;
	end

	-- Add the second tickler to this item
	if ( len( coalesce( @Tickler2, '' )) > 0 ) 
	begin
		set @tickler=@tickler + ' | ' + @Tickler2;
		exec SobekCM_Metadata_Save_Single @ItemID, 'Tickler', @Tickler2;
	end
	
	-- Add the third tickler to this item
	if ( len( coalesce( @Tickler3, '' )) > 0 ) 
	begin
		set @tickler=@tickler + ' | ' + @Tickler3;
		exec SobekCM_Metadata_Save_Single @ItemID, 'Tickler', @Tickler3;
	end
	
	-- Add the fourth tickler to this item
	if ( len( coalesce( @Tickler4, '' )) > 0 ) 
	begin
		set @tickler=@tickler + ' | ' + @Tickler4;
		exec SobekCM_Metadata_Save_Single @ItemID, 'Tickler', @Tickler4;
	end
	
	-- Add the fifth tickler to this item
	if ( len( coalesce( @Tickler5, '' )) > 0 ) 
	begin
		set @tickler=@tickler + ' | ' + @Tickler5;
		exec SobekCM_Metadata_Save_Single @ItemID, 'Tickler', @Tickler5;
	end
	
	-- Set the tickler value for this item in the basic search table
	update SobekCM_Metadata_Basic_Search_Table
	set Tickler=@tickler
	where ItemID=@ItemID;

commit transaction
GO

GRANT EXECUTE ON [SobekCM_Save_Item_Ticklers] to sobek_user;
GRANT EXECUTE ON [SobekCM_Save_Item_Ticklers] to sobek_itemeditor;
GO



ALTER PROCEDURE [dbo].[SobekCM_Item_Count_By_Collection]
AS
BEGIN

	-- No need to perform any locks here, especially given the possible
	-- length of this search
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET NOCOUNT ON;
	SET ARITHABORT ON;
	
	-- Get the id for the ALL aggregation
	declare @all_id int;
	set @all_id = coalesce(( select AggregationID from SObekCM_Item_Aggregation where Code='all'), -1);
	
	declare @Aggregation_List TABLE
	(
	  AggregationID int,
	  Code varchar(20),
	  ChildCode varchar(20),
	  Child2Code varchar(20),
	  AllCodes varchar(20),
	  Name nvarchar(255),
	  ShortName nvarchar(100),
	  [Type] varchar(50),
	  isActive bit
	);
	
	-- Insert the list of items linked to ALL or linked to NONE (include ALL)
	insert into @Aggregation_List ( AggregationID, Code, ChildCode, Child2Code, AllCodes, Name, ShortName, [Type], isActive )
	select AggregationID, Code, '', '', Code, Name, ShortName, [Type], isActive
	from SobekCM_Item_Aggregation A
	where ( [Type] not like 'Institut%' )
	  and ( Deleted='false' )
	  and exists ( select * from SobekCM_Item_Aggregation_Hierarchy where ChildID=A.AggregationID and ParentID=@all_id);
	  
	-- Insert the children under those top-level collections
	insert into @Aggregation_List ( AggregationID, Code, ChildCode, Child2Code, AllCodes, Name, ShortName, [Type], isActive )
	select A2.AggregationID, T.Code, A2.Code, '', A2.Code, A2.Name, A2.SHortName, A2.[Type], A2.isActive
	from @Aggregation_List T, SobekCM_Item_Aggregation A2, SobekCM_Item_Aggregation_Hierarchy H
	where ( A2.[Type] not like 'Institut%' )
	  and ( T.AggregationID = H.ParentID )
	  and ( A2.AggregationID = H.ChildID )
	  and ( Deleted='false' );
	  
	-- Insert the grand-children under those child collections
	insert into @Aggregation_List ( AggregationID, Code, ChildCode, Child2Code, AllCodes, Name, ShortName, [Type], isActive )
	select A2.AggregationID, T.Code, T.ChildCode, A2.Code, A2.Code, A2.Name, A2.SHortName, A2.[Type], A2.isActive
	from @Aggregation_List T, SobekCM_Item_Aggregation A2, SobekCM_Item_Aggregation_Hierarchy H
	where ( A2.[Type] not like 'Institut%' )
	  and ( T.AggregationID = H.ParentID )
	  and ( A2.AggregationID = H.ChildID )
	  and ( Deleted='false' )
	  and ( ChildCode <> '' );
	  
	-- Get total item count
	declare @total_item_count int
	select @total_item_count =  ( select count(*) from SobekCM_Item where Deleted = 'false' and Milestone_OnlineComplete is not null );

	-- Get total title count
	declare @total_title_count int
	select @total_title_count =  ( select count(*) from SobekCM_Item_Group G where G.Deleted = 'false' and exists ( select * from SobekCM_Item I where I.GroupID = G.GroupID and I.Deleted = 'false' and Milestone_OnlineComplete is not null ));

	-- Get total title count
	declare @total_page_count int
	select @total_page_count =  coalesce(( select sum( [PageCount] ) from SobekCM_Item where Deleted = 'false'  and ( Milestone_OnlineComplete is not null )), 0 );

	-- Start to build the return set of values
	select code1 = Code, 
	       code2 = ChildCode,
	       code3 = Child2Code,
	       AllCodes,
	    [Name], 
	    C.isActive AS Active,
		title_count = ( select count(distinct(GroupID)) from Statistics_Item_Aggregation_Link_View T where T.AggregationID = C.AggregationID ),
		item_count = ( select count(distinct(ItemID)) from Statistics_Item_Aggregation_Link_View T where T.AggregationID = C.AggregationID ), 
		page_count = coalesce(( select sum( PageCount ) from Statistics_Item_Aggregation_Link_View T where T.AggregationID = C.AggregationID ), 0)
	from @Aggregation_List C
	where ( C.Code <> 'TESTCOL' ) AND ( C.Code <> 'TESTG' )
	union
	select 'ZZZ','','', 'ZZZ', 'Total Count', 'false', @total_title_count, @total_item_count, @total_page_count
	order by code, code2, code3;
END;
GO




-- Ensure the stored procedure exists
IF object_id('Tracking_Add_New_Workflow') IS NULL EXEC ('create procedure dbo.Tracking_Add_New_Workflow as select 1;');
GO

--Stored procedure for getting all the tracking workflow entries by user
--entered through the tracking sheet
ALTER PROCEDURE [dbo].[Tracking_Add_New_Workflow]
	@itemid int,
	@user varchar(50),
	@dateStarted DateTime,
	@dateCompleted DateTime,
	@relatedEquipment varchar(1000),
	@EventNumber int,
	@workflow_entry_id int output
AS
begin transaction
	
	begin
		-- Get the workflow id
		declare @workflowid int
		
		-- Get the existing ID for this workflow
			
	    set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
		-- Add this new workflow entry 
		insert into Tracking_Progress ( ItemID, WorkFlowID, DateStarted, DateCompleted, WorkPerformedBy, RelatedEquipment )
		values ( @itemid, @workflowid, @dateStarted, @dateStarted, @user, @relatedEquipment );
		
		set @workflow_entry_id=@@IDENTITY;
	end
commit transaction
GO

-- Ensure the stored procedure exists
IF object_id('Tracking_Add_New_Workflow') IS NULL EXEC ('create procedure dbo.Tracking_Add_New_Workflow as select 1;');
GO

ALTER PROCEDURE [dbo].[Tracking_Add_New_Workflow]
	@itemid int,
	@user varchar(50),
	@dateStarted DateTime,
	@dateCompleted DateTime,
	@relatedEquipment varchar(1000),
	@EventNumber int,
	@StartEventNumber int,
	@EndEventNumber int,
	@Start_End_Event int,
	@workflow_entry_id int output
AS
begin transaction
	
	begin
		-- Get the workflow id
		declare @workflowid int
		
		-- Get the matching ID for this workflow
			
	    set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
		-- Add this new workflow entry 
		insert into Tracking_Progress ( ItemID, WorkFlowID, DateStarted, DateCompleted, WorkPerformedBy, RelatedEquipment, Start_Event_Number, End_Event_Number, Start_And_End_Event_Number)
		values ( @itemid, @workflowid, @dateStarted, @dateCompleted, @user, @relatedEquipment, @StartEventNumber, @EndEventNumber, @Start_End_Event );
		
		set @workflow_entry_id=@@IDENTITY;
	end
commit transaction
GO


-- Ensure the stored procedure exists
IF object_id('Tracking_Update_Workflow') IS NULL EXEC ('create procedure dbo.Tracking_Update_Workflow as select 1;');
GO

ALTER PROCEDURE [dbo].[Tracking_Update_Workflow]
	@itemid int,
	@user varchar(50),
	@dateStarted DateTime,
	@dateCompleted DateTime,
	@relatedEquipment varchar(1000),
	@EventNumber int,
	@StartEventNumber int,
	@EndEventNumber int,
	@workflow_entry_id int 
AS
	
	begin
		-- Get the workflow id
		declare @workflowid int
		
		-- Get the existing ID for this workflow
			
	    set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
		-- Update this workflow entry 
		Update Tracking_Progress
		set DateStarted=@dateStarted, 
		    DateCompleted=@dateCompleted,
		    RelatedEquipment=@relatedEquipment,
		    Start_Event_Number=@StartEventNumber,
		    End_Event_Number = @EndEventNumber,
		    WorkFlowID = @workflowid,
		    WorkPerformedBy = @user
		where @@IDENTITY=@workflow_entry_id AND ItemID=@itemid;
		 

	end
GO


/* Stored procedure to delete a workflow entry */
-- Ensure the stored procedure exists
IF object_id('Tracking_Delete_Workflow') IS NULL EXEC ('create procedure dbo.Tracking_Delete_Workflow as select 1;');
GO

ALTER PROCEDURE [dbo].[Tracking_Delete_Workflow]
	@workflow_entry_id int 
AS
	
	begin
	
	 
		-- Delete this workflow entry 
		delete from Tracking_Progress
		where ProgressID=@workflow_entry_id;
		 

	end
GO


-- Ensure the stored procedure exists
IF object_id('SobekCM_Get_Last_Open_Workflow_By_ItemID') IS NULL EXEC ('create procedure dbo.SobekCM_Get_Last_Open_Workflow_By_ItemID as select 1;');
GO

ALTER PROCEDURE [dbo].[SobekCM_Get_Last_Open_Workflow_By_ItemID]
	@ItemID int,
	@EventNumber int
AS
BEGIN

	-- Get the workflow id
	declare @workflowid int;
	set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
	-- If there is a match continue
	if ( @workflowid > 0 )
	begin
	
		select P.ProgressID, W.WorkFlowName, W.Start_Event_Desc, W.End_Event_Desc, W.Start_Event_Number, W.End_Event_Number, W.Start_And_End_Event_Number,
		       P.DateStarted, P.DateCompleted, P.RelatedEquipment, P.WorkPerformedBy, P.WorkingFilePath, P.ProgressNote
		from Tracking_Progress P, Tracking_Workflow W
		where ItemID = @ItemID
		  and P.WorkFlowID = @workflowid
		  and P.WorkFlowID = W.WorkFlowID
		  and ( DateCompleted is null );
		  
	
	end;
END;
GO


-- Ensure the stored procedure exists
IF object_id('Tracking_Update_Workflow') IS NULL EXEC ('create procedure dbo.Tracking_Update_Workflow as select 1;');
GO

ALTER PROCEDURE [dbo].[Tracking_Update_Workflow]
	@itemid int,
	@user varchar(50),
	@dateStarted DateTime,
	@dateCompleted DateTime,
	@relatedEquipment varchar(1000),
	@EventNumber int,
	@StartEventNumber int,
	@EndEventNumber int,
	@workflow_entry_id int 
AS
	
	begin
		-- Get the workflow id
		declare @workflowid int
		
		-- Get the existing ID for this workflow
			
	    set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
		-- Update this workflow entry 
		Update Tracking_Progress
		set DateStarted=@dateStarted, 
		    DateCompleted=@dateCompleted,
		    RelatedEquipment=@relatedEquipment,
		    Start_Event_Number=@StartEventNumber,
		    End_Event_Number = @EndEventNumber,
		    WorkFlowID = @workflowid,
		    WorkPerformedBy = @user
		where ProgressID=@workflow_entry_id;
		 

	end
GO

-- Ensure the stored procedure exists
IF object_id('SobekCM_Get_Last_Open_Workflow_By_ItemID') IS NULL EXEC ('create procedure dbo.SobekCM_Get_Last_Open_Workflow_By_ItemID as select 1;');
GO

ALTER PROCEDURE [dbo].[SobekCM_Get_Last_Open_Workflow_By_ItemID]
	@ItemID int,
	@EventNumber int
AS
BEGIN

	-- Get the workflow id
	declare @workflowid int;
	set @workflowid = coalesce((select WorkFlowID from Tracking_Workflow where Start_Event_Number = @EventNumber or End_Event_Number = @EventNumber ), -1);
	
	-- If there is a match continue
	if ( @workflowid > 0 )
	begin
	
		select P.ItemID,P.ProgressID, W.WorkFlowName, W.Start_Event_Desc, W.End_Event_Desc, W.Start_Event_Number, W.End_Event_Number, W.Start_And_End_Event_Number,
		       P.DateStarted, P.DateCompleted, P.RelatedEquipment, P.WorkPerformedBy, P.WorkingFilePath, P.ProgressNote
		from Tracking_Progress P, Tracking_Workflow W
		where ItemID = @ItemID
		  and P.WorkFlowID = @workflowid
		  and P.WorkFlowID = W.WorkFlowID
		  and ( DateCompleted is null );
		  
	
	end;
END;
GO


-- Ensure the stored procedure exists
IF object_id('Tracking_Get_Users_Scanning_Processing') IS NULL EXEC ('create procedure dbo.Tracking_Get_Users_Scanning_Processing as select 1;');
GO

/****** Object:  StoredProcedure [dbo].[Tracking_Get_Users_Scanning_Processing]    Script Date: 10/22/2013 11:52:33 ******/
ALTER PROCEDURE [dbo].[Tracking_Get_Users_Scanning_Processing]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT UserName,EmailAddress,FirstName,LastName,ScanningTechnician, ProcessingTechnician 
	FROM mySobek_User
	WHERE ScanningTechnician=1 OR ProcessingTechnician=1
END
GO


/*Create [Tracking_Get_Scanners_List] Stored Procedure*/

-- Ensure the stored procedure exists
IF object_id('Tracking_Get_Scanners_List') IS NULL EXEC ('create procedure dbo.Tracking_Get_Scanners_List as select 1;');
GO
/****** Object:  StoredProcedure [dbo].[Tracking_Get_Scanners_List]    Script Date: 10/22/2013 12:04:08 ******/
ALTER PROCEDURE [dbo].[Tracking_Get_Scanners_List]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ScanningEquipment, Notes, Location,EquipmentType 
	FROM Tracking_ScanningEquipment
	WHERE isActive=1
END;
GO


-- Ensure the stored procedure exists
IF object_id('Tracking_Get_All_Entries_By_User') IS NULL EXEC ('create procedure dbo.Tracking_Get_All_Entries_By_User as select 1;');
GO

--Stored procedure for getting all the tracking workflow entries by user
--entered through the tracking sheet
ALTER PROCEDURE [dbo].[Tracking_Get_All_Entries_By_User]
	@username nvarchar(50)
	
AS
BEGIN
	
		select P.ItemID,P.ProgressID, W.WorkFlowName, W.Start_Event_Desc, W.End_Event_Desc, W.Start_Event_Number, W.End_Event_Number, W.Start_And_End_Event_Number,
		       P.DateStarted, P.DateCompleted, P.RelatedEquipment, P.WorkPerformedBy, P.WorkingFilePath, P.ProgressNote
		from Tracking_Progress P, Tracking_Workflow W
		where P.WorkFlowID = W.WorkFlowID
		and P.WorkPerformedBy = @username
		and W.WorkFlowName ='Scanning' or W.WorkFlowName='Processing';


END;

GRANT EXECUTE ON Tracking_Get_All_Entries_By_User to sobek_user;


--Script to add new columns related to item tracking
--Check to see if the columns already exist
IF COL_LENGTH('Tracking_Progress', 'Start_Event_Number') IS NULL
BEGIN
ALTER TABLE Tracking_Progress 
ADD Start_Event_Number int;
END

IF COL_LENGTH('Tracking_Progress', 'End_Event_Number') IS NULL
BEGIN
ALTER TABLE Tracking_Progress 
ADD End_Event_Number int;
END

IF COL_LENGTH('Tracking_Progress', 'Start_And_End_Event_Number') IS NULL
BEGIN
ALTER TABLE Tracking_Progress 
ADD Start_And_End_Event_Number int;
END


-- Ensure the stored procedure exists
IF object_id('Tracking_Get_Scanners_List') IS NULL EXEC ('create procedure dbo.Tracking_Get_Scanners_List as select 1;');
GO


--Stored procedure to get the list of Scanning/Processing Equipment
ALTER PROCEDURE [dbo].[Tracking_Get_Scanners_List]
      
AS
BEGIN
      -- SET NOCOUNT ON added to prevent extra result sets from
      -- interfering with SELECT statements.
      SET NOCOUNT ON;

    -- Insert statements for procedure here
      SELECT ScanningEquipment, Notes, Location,EquipmentType 
      FROM Tracking_ScanningEquipment
      WHERE isActive=1
END

GRANT EXECUTE ON Tracking_Get_Scanners_List to sobek_user;
GRANT EXECUTE ON Tracking_Add_New_Workflow to sobek_user;
GRANT EXECUTE ON Tracking_Update_Workflow to sobek_user;
GRANT EXECUTE ON Tracking_Delete_Workflow to sobek_user;
GRANT EXECUTE ON Tracking_Get_All_Entries_By_User to sobek_user;
GRANT EXECUTE ON SobekCM_Get_Last_Open_Workflow_By_ItemID to sobek_user;
GRANT EXECUTE ON Tracking_Get_Users_Scanning_Processing to sobek_user;
GO


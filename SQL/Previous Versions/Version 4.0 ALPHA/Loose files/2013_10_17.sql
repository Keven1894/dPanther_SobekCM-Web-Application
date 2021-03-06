

alter table Tracking_Progress add DateStarted datetime null;
alter table Tracking_Progress add Duration int not null default(0);
alter table Tracking_Progress add RelatedEquipment nvarchar(255) null;
GO

alter table Tracking_Workflow add Start_Event_Number int null;
alter table Tracking_Workflow add End_Event_Number int null;
alter table Tracking_Workflow add Start_And_End_Event_Number int null;
alter table Tracking_Workflow add Start_Event_Desc nvarchar(100) null;
alter table Tracking_Workflow add End_Event_Desc nvarchar(100) null;
GO

-- Update the scanning, processing, and material disposition

CREATE TABLE [dbo].[Tracking_ScanningEquipment](
	[EquipmentID] [int] IDENTITY(1,1) NOT NULL,
	[ScanningEquipment] [nvarchar](255) NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[Location] [nvarchar](255) NULL,
	[EquipmentType] [nvarchar](255) NULL,
	[isActive] [bit] NOT NULL,
 CONSTRAINT [PK_Tracking_ScanningEquipment] PRIMARY KEY CLUSTERED 
(
	[EquipmentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

alter table mySobek_User add ScanningTechnician bit default(0);
alter table mySobek_User add ProcessingTechnician bit default(0);
GO

alter table SobekCM_Item add Complete_KML varchar(max) null;
GO

alter table SobekCM_Item_Footprint add Segment_KML varchar(max) null;
GO

-- Close all current workflows
update Tracking_Progress set DateStarted=DateCompleted, Duration=0;
GO


CREATE PROCEDURE Get_Last_Open_Workflow_By_ItemID
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
	
		select W.WorkFlowName, W.Start_Event_Desc, W.End_Event_Desc, W.Start_Event_Number, W.End_Event_Number, W.Start_And_End_Event_Number,
		       P.DateStarted, P.DateCompleted, P.RelatedEquipment, P.WorkPerformedBy, P.WorkingFilePath, P.ProgressNote
		from Tracking_Progress P, Tracking_Workflow W
		where ItemID = @ItemID
		  and P.WorkFlowID = @workflowid
		  and P.WorkFlowID = W.WorkFlowID
		  and ( DateCompleted is null );
	
	end;
END;
GO

GRANT EXECUTE ON Get_Last_Open_Workflow_By_ItemID TO sobek_user;
GRANT EXECUTE ON Get_Last_Open_Workflow_By_ItemID TO sobek_itemeditor;
GO

-- Procedure links an item to a region
-- Written by Mark Sullivan ( August 2007 )
ALTER PROCEDURE [dbo].[SobekCM_Save_Item_Footprint]
	@ItemID int,
	@point_latitude float,
	@point_longitude float,
	@rect_latitude_A float,
	@rect_longitude_A float,
	@rect_latitude_B float,
	@rect_longitude_B float,
	@segment_kml varchar(max)
AS
begin transaction

	insert into SobekCM_Item_Footprint( ItemID, Point_Latitude, Point_Longitude, Rect_Latitude_A, Rect_Longitude_A, Rect_Latitude_B, Rect_Longitude_B, Segment_KML )
	values ( @itemid, @point_latitude, @point_longitude, @rect_latitude_a, @rect_longitude_a, @rect_latitude_b, @rect_longitude_b, @segment_kml )

commit transaction
GO




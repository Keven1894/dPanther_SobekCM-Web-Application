IF NOT EXISTS
(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SobekCM_QC_Errors_History]') AND type in (N'U'))
BEGIN

CREATE TABLE dbo.SobekCM_QC_Errors_History
	(
	ErrorID bigint NOT NULL IDENTITY(1,1),
	ItemID int NOT NULL,
	ErrorCode nchar(10) NOT NULL,
	isVolumeError bit NULL,
	Count int NOT NULL
	)  ON [PRIMARY];


ALTER TABLE dbo.SobekCM_QC_Errors_History ADD CONSTRAINT
	PK_SobekCM_QC_Errors_History PRIMARY KEY CLUSTERED 
	(
	ErrorID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];


ALTER TABLE dbo.SobekCM_QC_Errors_History SET (LOCK_ESCALATION = TABLE);



ALTER TABLE [dbo].[SobekCM_QC_Errors_History]
  ADD CONSTRAINT ItemID_FK2 FOREIGN KEY (ItemID) references SobekCM_Item(ItemID);


END;
GO


BEGIN TRANSACTION
DECLARE @result int;  
EXEC @result = sp_getapplock @Resource = '<TABLE_NAME>',   
				@LockMode = 'Exclusive';  
IF @result > -1 
BEGIN

	IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
					 WHERE TABLE_SCHEMA = 'dbo' 
					 AND  TABLE_NAME = '<TABLE_NAME>'))
	BEGIN
		CREATE TABLE [dbo].[<TABLE_NAME>](
			[<QUEUE_NAME>QueueId] [int] IDENTITY(1,1) NOT NULL,
			[<QUEUE_NAME>QueueParams] [varchar](500) NOT NULL,
			[Status] [varchar](20) NULL,
			[ExtendedStatus] [varchar](500) NULL,
			[StartTime] [datetime] NULL,
			[StatusTime] [datetime] NULL,
			[CompleteTime] [datetime] NULL,
			[MachineName] [varchar](50) NULL,
		 CONSTRAINT [PK_<TABLE_NAME>] PRIMARY KEY CLUSTERED 
		(
			[<QUEUE_NAME>QueueId] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
	END

	EXEC @result = sp_releaseapplock @Resource = '<TABLE_NAME>'; 
END
COMMIT



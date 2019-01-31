ALTER PROCEDURE [dbo].[<PROC_NAME>] 
	@machine varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Results Table (Id int, Parms varchar(500))

    BEGIN TRANSACTION;  
	DECLARE @result int;  
	EXEC @result = sp_getapplock @Resource = '<PROC_NAME>',   
				   @LockMode = 'Exclusive';  
	IF @result > -1 
	BEGIN  

		insert into @Results
		select top 1 <QUEUE_NAME>QueueId, <QUEUE_NAME>QueueParams 
		from <TABLE_NAME> where [Status] = 'Ready' 
		AND (MachineName = @machine OR IsNULL(MachineName,'') = '')
		 Order by <QUEUE_NAME>QueueId DESC
	
		if ( @@ROWCOUNT = 1 )
		BEGIN
			update <TABLE_NAME> set Status = 'Processing', MachineName = @machine, StartTime = GetDate(), StatusTime = Getdate()
			where <QUEUE_NAME>QueueId in (select top 1 Id from @Results)

		END
	
		EXEC @result = sp_releaseapplock @Resource = '<PROC_NAME>';  
		COMMIT TRANSACTION;  
	END;  
	ELSE
	BEGIN  
		select top 1 <QUEUE_NAME>QueueId, <QUEUE_NAME>QueueParams from <TABLE_NAME> where 1=0
		 Order by <QUEUE_NAME>QueueId DESC

		ROLLBACK TRANSACTION;  
	END  

	select * from @Results

END
Alter PROCEDURE GetTaskManagerData
AS
BEGIN
SELECT [TaskManagerId]
      ,[Title]
      ,[Description]
      ,[Duedate]
      ,[Status]
FROM [dbo].[TaskManagers] where TaskManagerId=1 ;
END;

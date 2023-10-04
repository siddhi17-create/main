CREATE PROCEDURE [dbo].[GetTaskManagerDataALLTasks]
AS
BEGIN
SELECT [TaskManagerId]
      ,[Title]
      ,[Description]
      ,[Duedate]
      ,[Status]
FROM [dbo].[TaskManagers]
END;
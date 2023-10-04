CREATE PROCEDURE [GetTaskManagerDataALL]
AS
BEGIN
SELECT [TaskManagerId]
      ,[Title]
      ,[Description]
      ,[Duedate]
      ,[Status]
FROM [dbo].[TaskManagers]
END;
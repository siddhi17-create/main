ALTER PROCEDURE GetTaskManagerDataALL
AS
BEGIN
SELECT [TaskManagerId]
      ,[Title]
      ,[Description]
      ,[Duedate]
      ,[Status]
FROM [dbo].[TaskManagers] WHERE TaskManagerId = 1;
END;
ALTER PROCEDURE [dbo].[GetTaskManagerDataALL]
AS
BEGIN
SELECT *
FROM [dbo].[TaskManagers] WHERE TaskManagerId = 1;
END;
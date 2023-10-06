CREATE PROCEDURE CheckMigrationNameExists
    @MigrationName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) 
    FROM MigrationHistory
    WHERE MigrationName = @MigrationName;
END

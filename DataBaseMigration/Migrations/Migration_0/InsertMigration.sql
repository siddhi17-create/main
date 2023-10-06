CREATE PROCEDURE InsertMigrationHistory
    @MigrationName NVARCHAR(255),
    @AppliedOn DATETIME2
AS
BEGIN
    INSERT INTO MigrationHistory (MigrationName, AppliedOn)
    VALUES (@MigrationName, @AppliedOn);
END;

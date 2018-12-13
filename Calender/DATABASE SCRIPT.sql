if exists(select name from sys.databases where name = 'AD3_Kalender_teamversie')
BEGIN
	USE master
	PRINT 'Bestaande databank wordt verwijderd...'
	ALTER DATABASE AD3_Kalender_teamversie
		SET SINGLE_USER
		WITH ROLLBACK IMMEDIATE;
	WAITFOR DELAY '00:00:05'
	DROP DATABASE AD3_Kalender_teamversie
	PRINT 'Bestaande databank is verwijderd.'
END
GO

CREATE DATABASE AD3_Kalender_teamversie
PRINT 'Databank werd gemaakt'
GO

USE AD3_Kalender_teamversie


CREATE TABLE tblKalender(
	Id				int				identity primary key   		not null,
	Naam			varchar(50)									not null,
	Beschrijving	nvarchar(Max)								not	null
)


CREATE TABLE tblAfspraak (
	Id				int				identity primary key	not null,
	startTime		datetime2								not null,
	endTime			datetime2								not null,
	subject			varchar(50)								not null,
    beschrijving	nvarchar(max)							not null,
    KalenderID		int	references tblKalender(Id)			not null
)
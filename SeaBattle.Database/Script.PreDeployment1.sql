/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

IF (EXISTS(SELECT * FROM [dbo].[ShipCoordinates]))  
BEGIN  
    DELETE FROM [dbo].[ShipCoordinates]
END

IF (EXISTS(SELECT * FROM [dbo].[Ships]))  
BEGIN  
    DELETE FROM [dbo].[Ships]
END

IF (EXISTS(SELECT * FROM [dbo].[Coordinates]))  
BEGIN  
    DELETE FROM [dbo].[Coordinates]
END

IF (EXISTS(SELECT * FROM [dbo].[GameFields]))  
BEGIN  
    DELETE FROM [dbo].[GameFields]
END

IF (EXISTS(SELECT * FROM [dbo].[Points]))  
BEGIN  
    DELETE FROM [dbo].[Points]
END

IF (EXISTS(SELECT * FROM [dbo].[ShipTypes]))  
BEGIN  
    DELETE FROM [dbo].[ShipTypes]
END

IF (EXISTS(SELECT * FROM [dbo].[CoordinateTypes]))  
BEGIN  
    DELETE FROM [dbo].[CoordinateTypes]
END
-- SQL equivalent of InitialCreateCatalog for MySQL.
USE `fcgdb`;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Games` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Price` double NOT NULL,
    CONSTRAINT `PK_Games` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Library` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `IDUsuario` int NOT NULL,
    `IDGame` int NOT NULL,
    CONSTRAINT `PK_Library` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260923153736_InitialCreateCatalog', '8.0.31');

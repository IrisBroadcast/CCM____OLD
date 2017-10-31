-- ------------------------------------------------------
-- Server version	5.7.19-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `CallHistories`
--

DROP TABLE IF EXISTS `CallHistories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CallHistories` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `CallId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `SipCallId` longtext COLLATE utf8mb4_swedish_ci,
  `Started` datetime NOT NULL,
  `Ended` datetime NOT NULL,
  `DlgHashId` longtext COLLATE utf8mb4_swedish_ci,
  `DlgHashEnt` longtext COLLATE utf8mb4_swedish_ci,
  `ToTag` longtext COLLATE utf8mb4_swedish_ci,
  `FromTag` longtext COLLATE utf8mb4_swedish_ci,
  `FromId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromSip` longtext COLLATE utf8mb4_swedish_ci,
  `FromUsername` longtext COLLATE utf8mb4_swedish_ci,
  `FromDisplayName` longtext COLLATE utf8mb4_swedish_ci,
  `FromComment` longtext COLLATE utf8mb4_swedish_ci,
  `FromLocationId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromLocationName` longtext COLLATE utf8mb4_swedish_ci,
  `FromLocationComment` longtext COLLATE utf8mb4_swedish_ci,
  `FromLocationShortName` longtext COLLATE utf8mb4_swedish_ci,
  `FromCodecTypeId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromCodecTypeName` longtext COLLATE utf8mb4_swedish_ci,
  `FromCodecTypeColor` longtext COLLATE utf8mb4_swedish_ci,
  `FromOwnerId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromOwnerName` longtext COLLATE utf8mb4_swedish_ci,
  `FromRegionId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromRegionName` longtext COLLATE utf8mb4_swedish_ci,
  `FromUserAgentHead` longtext COLLATE utf8mb4_swedish_ci,
  `ToId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToSip` longtext COLLATE utf8mb4_swedish_ci,
  `ToUsername` longtext COLLATE utf8mb4_swedish_ci,
  `ToDisplayName` longtext COLLATE utf8mb4_swedish_ci,
  `ToComment` longtext COLLATE utf8mb4_swedish_ci,
  `ToLocationId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToLocationName` longtext COLLATE utf8mb4_swedish_ci,
  `ToLocationComment` longtext COLLATE utf8mb4_swedish_ci,
  `ToLocationShortName` longtext COLLATE utf8mb4_swedish_ci,
  `ToCodecTypeId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToCodecTypeName` longtext COLLATE utf8mb4_swedish_ci,
  `ToCodecTypeColor` longtext COLLATE utf8mb4_swedish_ci,
  `ToOwnerId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToOwnerName` longtext COLLATE utf8mb4_swedish_ci,
  `ToRegionId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToRegionName` longtext COLLATE utf8mb4_swedish_ci,
  `ToUserAgentHead` longtext COLLATE utf8mb4_swedish_ci,
  `IsPhoneCall` tinyint(4) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `CIX_CallHistories_Ended` (`Ended`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Calls`
--

DROP TABLE IF EXISTS `Calls`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Calls` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `SipCallID` longtext COLLATE utf8mb4_swedish_ci,
  `DlgHashId` longtext COLLATE utf8mb4_swedish_ci,
  `DlgHashEnt` longtext COLLATE utf8mb4_swedish_ci,
  `FromId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `FromUsername` longtext COLLATE utf8mb4_swedish_ci,
  `FromDisplayName` varchar(200) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `ToId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ToUsername` longtext COLLATE utf8mb4_swedish_ci,
  `ToDisplayName` varchar(200) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Started` datetime NOT NULL,
  `Updated` datetime NOT NULL,
  `Closed` tinyint(4) NOT NULL,
  `State` int(11) DEFAULT NULL,
  `ToTag` longtext COLLATE utf8mb4_swedish_ci,
  `FromTag` longtext COLLATE utf8mb4_swedish_ci,
  `IsPhoneCall` tinyint(4) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Closed` (`Closed`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Cities`
--

DROP TABLE IF EXISTS `Cities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Cities` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CodecPresetUserAgents`
--

DROP TABLE IF EXISTS `CodecPresetUserAgents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CodecPresetUserAgents` (
  `CodecPreset_Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `UserAgent_Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  PRIMARY KEY (`CodecPreset_Id`,`UserAgent_Id`),
  KEY `IX_CodecPreset_CodecPresetId` (`CodecPreset_Id`),
  KEY `IX_UserAgent_UserAgentId` (`UserAgent_Id`),
  CONSTRAINT `FK_CodecPresetUserAgents_CodecPresetId` FOREIGN KEY (`CodecPreset_Id`) REFERENCES `CodecPresets` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_CodecPresetUserAgents_UserAgentId` FOREIGN KEY (`UserAgent_Id`) REFERENCES `UserAgents` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CodecPresets`
--

DROP TABLE IF EXISTS `CodecPresets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CodecPresets` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CodecTypes`
--

DROP TABLE IF EXISTS `CodecTypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CodecTypes` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `Color` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Filters`
--

DROP TABLE IF EXISTS `Filters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Filters` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `PropertyName` longtext COLLATE utf8mb4_swedish_ci,
  `Type` longtext COLLATE utf8mb4_swedish_ci,
  `FilteringName` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LocalPasswords`
--

DROP TABLE IF EXISTS `LocalPasswords`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LocalPasswords` (
  `UserId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Password` longtext COLLATE utf8mb4_swedish_ci,
  `Salt` longtext COLLATE utf8mb4_swedish_ci,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Locations`
--

DROP TABLE IF EXISTS `Locations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Locations` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `ShortName` longtext COLLATE utf8mb4_swedish_ci,
  `Comment` longtext COLLATE utf8mb4_swedish_ci,
  `Net_Address_v4` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Net_Address` bigint(20) DEFAULT NULL,
  `Cidr` tinyint(4) DEFAULT NULL,
  `StartAddress` bigint(20) DEFAULT NULL,
  `EndAddress` bigint(20) DEFAULT NULL,
  `Net_Address_v6` varchar(200) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Cidr_v6` tinyint(4) DEFAULT NULL,
  `CarrierConnectionId` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  `City_Id` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Region_Id` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `ProfileGroup_Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_City_CityId` (`City_Id`),
  KEY `IX_Region_RegionId` (`Region_Id`),
  KEY `IX_Location_StartAddress_EndAddress` (`StartAddress`,`EndAddress`),
  KEY `fk_locations_profilegroups1_idx` (`ProfileGroup_Id`),
  CONSTRAINT `FK_City_CityId` FOREIGN KEY (`City_Id`) REFERENCES `Cities` (`Id`),
  CONSTRAINT `FK_Region_RegionId` FOREIGN KEY (`Region_Id`) REFERENCES `Regions` (`Id`),
  CONSTRAINT `fk_locations_profilegroups1` FOREIGN KEY (`ProfileGroup_Id`) REFERENCES `ProfileGroups` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Logs`
--

DROP TABLE IF EXISTS `Logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Logs` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Date` datetime(6) DEFAULT NULL,
  `Level` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `LevelValue` int(11) DEFAULT NULL,
  `Message` longtext COLLATE utf8mb4_swedish_ci,
  `Callsite` varchar(512) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Exception` varchar(512) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Application` varchar(64) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `ActivityId` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=15418 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MetaTypes`
--

DROP TABLE IF EXISTS `MetaTypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MetaTypes` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `PropertyName` longtext COLLATE utf8mb4_swedish_ci,
  `Type` longtext COLLATE utf8mb4_swedish_ci,
  `FullPropertyName` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Owners`
--

DROP TABLE IF EXISTS `Owners`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Owners` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ProfileGroupProfileOrders`
--

DROP TABLE IF EXISTS `ProfileGroupProfileOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ProfileGroupProfileOrders` (
  `Profile_Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ProfileGroup_Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `SortIndex` int(11) NOT NULL,
  PRIMARY KEY (`Profile_Id`,`ProfileGroup_Id`),
  KEY `fk_ProfileGroupProfileOrders_profilegroups1_idx` (`ProfileGroup_Id`),
  KEY `fk_ProfileGroupProfileOrders_profile_idx` (`Profile_Id`),
  CONSTRAINT `fk_ProfileGroupProfileOrders_profilegroups1` FOREIGN KEY (`ProfileGroup_Id`) REFERENCES `ProfileGroups` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `fk_ProfileGroupProfileOrders_profiles1` FOREIGN KEY (`Profile_Id`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ProfileGroups`
--

DROP TABLE IF EXISTS `ProfileGroups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ProfileGroups` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` varchar(64) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Description` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime(3) NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime(3) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `fk_ProfileGroup_Id_idx` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Profiles`
--

DROP TABLE IF EXISTS `Profiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Profiles` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` varchar(64) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Description` longtext COLLATE utf8mb4_swedish_ci,
  `Sdp` longtext COLLATE utf8mb4_swedish_ci NOT NULL,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  `SortIndex` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Regions`
--

DROP TABLE IF EXISTS `Regions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Regions` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `RegisteredSips`
--

DROP TABLE IF EXISTS `RegisteredSips`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `RegisteredSips` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `SIP` varchar(128) COLLATE utf8mb4_swedish_ci NOT NULL,
  `UserAgentHead` longtext COLLATE utf8mb4_swedish_ci,
  `Username` longtext COLLATE utf8mb4_swedish_ci,
  `DisplayName` longtext COLLATE utf8mb4_swedish_ci,
  `IP` longtext COLLATE utf8mb4_swedish_ci,
  `Port` int(11) NOT NULL,
  `ServerTimeStamp` bigint(20) NOT NULL,
  `Updated` datetime NOT NULL,
  `Expires` int(11) NOT NULL,
  `UserAgentId` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Location_LocationId` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `User_UserId` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserAgentId` (`UserAgentId`),
  KEY `IX_Location_LocationId` (`Location_LocationId`),
  KEY `IX_User_UserId` (`User_UserId`),
  CONSTRAINT `FK_dbo.RegisteredSips_dbo.Locations_Location_LocationId` FOREIGN KEY (`Location_LocationId`) REFERENCES `Locations` (`Id`),
  CONSTRAINT `FK_dbo.RegisteredSips_dbo.UserAgents_UserAgentId` FOREIGN KEY (`UserAgentId`) REFERENCES `UserAgents` (`Id`),
  CONSTRAINT `FK_dbo.RegisteredSips_dbo.Users_User_UserId` FOREIGN KEY (`User_UserId`) REFERENCES `Users` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Roles` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Settings`
--

DROP TABLE IF EXISTS `Settings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Settings` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `Value` longtext COLLATE utf8mb4_swedish_ci,
  `Description` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Studios`
--

DROP TABLE IF EXISTS `Studios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Studios` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci NOT NULL,
  `CodecSipAddress` longtext COLLATE utf8mb4_swedish_ci,
  `CameraAddress` longtext COLLATE utf8mb4_swedish_ci,
  `CameraActive` tinyint(4) NOT NULL,
  `CameraUsername` longtext COLLATE utf8mb4_swedish_ci,
  `CameraPassword` longtext COLLATE utf8mb4_swedish_ci,
  `CameraVideoUrl` longtext COLLATE utf8mb4_swedish_ci,
  `CameraImageUrl` longtext COLLATE utf8mb4_swedish_ci,
  `CameraPlayAudioUrl` longtext COLLATE utf8mb4_swedish_ci,
  `AudioClipNames` longtext COLLATE utf8mb4_swedish_ci,
  `InfoText` longtext COLLATE utf8mb4_swedish_ci,
  `MoreInfoUrl` longtext COLLATE utf8mb4_swedish_ci,
  `NrOfAudioInputs` int(11) NOT NULL,
  `AudioInputNames` longtext COLLATE utf8mb4_swedish_ci,
  `AudioInputDefaultGain` int(11) NOT NULL,
  `NrOfGpos` int(11) NOT NULL,
  `GpoNames` longtext COLLATE utf8mb4_swedish_ci,
  `InactivityTimeout` int(11) NOT NULL,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci NOT NULL,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci NOT NULL,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserAgentProfileOrders`
--

DROP TABLE IF EXISTS `UserAgentProfileOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserAgentProfileOrders` (
  `UserAgentId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `ProfileId` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `SortIndex` int(11) NOT NULL,
  PRIMARY KEY (`UserAgentId`,`ProfileId`),
  KEY `IX_ProfileId` (`ProfileId`),
  KEY `IX_UserAgentId` (`UserAgentId`),
  CONSTRAINT `FK_dbo.UserAgentProfileOrders_dbo.Profiles_ProfileId` FOREIGN KEY (`ProfileId`) REFERENCES `Profiles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_dbo.UserAgentProfileOrders_dbo.UserAgents_UserAgentId` FOREIGN KEY (`UserAgentId`) REFERENCES `UserAgents` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserAgents`
--

DROP TABLE IF EXISTS `UserAgents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `UserAgents` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `Name` longtext COLLATE utf8mb4_swedish_ci,
  `Identifier` longtext COLLATE utf8mb4_swedish_ci,
  `MatchType` int(11) NOT NULL,
  `Image` longtext COLLATE utf8mb4_swedish_ci,
  `UserInterfaceLink` longtext COLLATE utf8mb4_swedish_ci,
  `Ax` tinyint(4) NOT NULL,
  `Width` int(11) NOT NULL,
  `Height` int(11) NOT NULL,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  `Api` longtext COLLATE utf8mb4_swedish_ci,
  `Lines` int(11) NOT NULL,
  `Inputs` int(11) NOT NULL,
  `MaxInputDb` int(11) NOT NULL,
  `MinInputDb` int(11) NOT NULL,
  `Comment` longtext COLLATE utf8mb4_swedish_ci,
  `InputGainStep` int(11) NOT NULL,
  `GpoNames` longtext COLLATE utf8mb4_swedish_ci,
  `UserInterfaceIsOpen` tinyint(4) NOT NULL,
  `UseScrollbars` tinyint(4) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Users` (
  `Id` char(36) COLLATE utf8mb4_swedish_ci NOT NULL,
  `UserName` longtext COLLATE utf8mb4_swedish_ci,
  `FirstName` longtext COLLATE utf8mb4_swedish_ci,
  `LastName` longtext COLLATE utf8mb4_swedish_ci,
  `Comment` longtext COLLATE utf8mb4_swedish_ci,
  `UserType` int(11) NOT NULL,
  `RadiusId` bigint(20) NOT NULL,
  `LocalUser` tinyint(4) NOT NULL,
  `AccountLocked` tinyint(4) NOT NULL,
  `CreatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `CreatedOn` datetime NOT NULL,
  `UpdatedBy` longtext COLLATE utf8mb4_swedish_ci,
  `UpdatedOn` datetime NOT NULL,
  `CodecType_Id` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Owner_Id` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Role_Id` char(36) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `DisplayName` longtext COLLATE utf8mb4_swedish_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_CodecType_CodecTypeId` (`CodecType_Id`),
  KEY `IX_Owner_OwnerId` (`Owner_Id`),
  KEY `IX_Role_RoleId` (`Role_Id`),
  CONSTRAINT `FK_CodecTypes_CodecTypeId` FOREIGN KEY (`CodecType_Id`) REFERENCES `CodecTypes` (`Id`),
  CONSTRAINT `FK_Owners_OwnerId` FOREIGN KEY (`Owner_Id`) REFERENCES `Owners` (`Id`),
  CONSTRAINT `FK_Roles_RoleId` FOREIGN KEY (`Role_Id`) REFERENCES `Roles` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;


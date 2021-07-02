CREATE SCHEMA flying;
use flying;

CREATE TABLE `airlines` (
  `sqlID` int(11) NOT NULL AUTO_INCREMENT,
  `AirlineCode` varchar(10) DEFAULT NULL,
  `AirlineName` varchar(100) DEFAULT NULL,
  `ICAOCode` varchar(10) DEFAULT NULL,
  `CallSign` varchar(45) DEFAULT NULL,
  `CountryOrRegion` varchar(45) DEFAULT NULL,
  `ScanForData` varchar(1) DEFAULT NULL,
  `Priority` int(11) DEFAULT NULL,
  PRIMARY KEY (`sqlID`),
  UNIQUE KEY `idx_AirlineCodeAndName` (`AirlineCode`,`AirlineName`)
) ENGINE=InnoDB AUTO_INCREMENT=5759 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `airports` (
  `ICAOcode` varchar(10) NOT NULL,
  `AirportType` varchar(100) DEFAULT NULL,
  `AirportName` varchar(100) DEFAULT NULL,
  `LatDegrees` double DEFAULT NULL,
  `LongDegrees` double DEFAULT NULL,
  `Elevation` int(11) DEFAULT NULL,
  `Continent` varchar(45) DEFAULT NULL,
  `ISOcountry` varchar(45) DEFAULT NULL,
  `ISOregion` varchar(45) DEFAULT NULL,
  `IATAcode` varchar(45) DEFAULT NULL,
  `Municipality` varchar(100) DEFAULT NULL,
  `ScheduleService` varchar(45) DEFAULT NULL,
  `TimeZone` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ICAOcode`),
  KEY `idx_IATA_code` (`IATAcode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `flightaware` (
  `sqlID` int(11) NOT NULL AUTO_INCREMENT,
  `FlightNumber` varchar(45) DEFAULT NULL,
  `FlightDate` varchar(45) DEFAULT NULL,
  `FlightTime` varchar(45) DEFAULT NULL,
  `DepartureCity` varchar(45) DEFAULT NULL,
  `ArrivalCity` varchar(45) DEFAULT NULL,
  `DepartureHerbDateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`sqlID`),
  UNIQUE KEY `idx_unique` (`FlightNumber`,`FlightDate`,`FlightTime`,`DepartureCity`,`ArrivalCity`),
  KEY `idx_depart_city` (`DepartureCity`),
  KEY `idx_arrival_city` (`ArrivalCity`)
) ENGINE=InnoDB AUTO_INCREMENT=375300 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `timezones` (
  `TimeZoneName` varchar(100) NOT NULL,
  `DaylightSavingsTimeOffset` varchar(100) DEFAULT NULL,
  `StandardTimeOffset` varchar(100) DEFAULT NULL,
  `NextChangeDirection` varchar(45) DEFAULT NULL,
  `NextChangeDate` datetime DEFAULT NULL,
  PRIMARY KEY (`TimeZoneName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


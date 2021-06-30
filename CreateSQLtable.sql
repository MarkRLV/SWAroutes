use flying;
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

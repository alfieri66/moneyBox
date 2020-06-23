-- phpMyAdmin SQL Dump
-- version 4.9.0.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Creato il: Mar 16, 2020 alle 21:31
-- Versione del server: 10.3.16-MariaDB
-- Versione PHP: 7.3.7

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `pushdatabase`
--

-- --------------------------------------------------------

--
-- Struttura della tabella `incassi`
--

CREATE TABLE `incassi` (
  `idIncasso` int(11) NOT NULL,
  `data` datetime DEFAULT NULL,
  `idUtente` int(11) DEFAULT NULL,
  `idLocale` int(11) DEFAULT NULL,
  `acconto` float DEFAULT NULL,
  `recupero` float DEFAULT NULL,
  `daRiportare` float DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dump dei dati per la tabella `incassi`
--

INSERT INTO `incassi` (`idIncasso`, `data`, `idUtente`, `idLocale`, `acconto`, `recupero`, `daRiportare`) VALUES
(1, '2020-01-01 12:20:00', 2, 1, 100, 200, 75),
(2, '2020-01-02 11:20:00', 2, 2, 110, 210, 50),
(3, '2020-01-02 12:00:00', 3, 3, 120.12, 220, 75),
(4, '2020-01-02 11:20:00', 3, 1, 130, 230, 50),
(5, '2020-01-03 12:40:00', 4, 2, 140, 240, 75),
(6, '2020-01-03 11:40:00', 4, 3, 150, 250, 50),
(7, '2020-01-03 12:10:00', 2, 1, 160, 260, 75),
(8, '2020-01-03 11:30:00', 2, 2, 170, 270, 50),
(9, '2020-01-03 12:30:00', 3, 3, 180, 280, 75),
(10, '2020-01-04 11:30:00', 3, 1, 190, 290, 50),
(11, '2020-01-04 12:20:00', 4, 2, 200, 300, 75),
(12, '2020-01-04 11:10:00', 4, 3, 210, 300, 50),
(13, '2020-01-04 12:40:00', 2, 1, 220, 300, 75),
(14, '2020-01-04 11:30:00', 2, 2, 230, 300, 50),
(15, '2020-01-04 11:10:00', 3, 3, 240, 300, 75),
(16, '2020-01-04 12:40:00', 3, 1, 250, 300, 50);

-- --------------------------------------------------------

--
-- Struttura della tabella `locali`
--

CREATE TABLE `locali` (
  `idLocale` int(11) NOT NULL,
  `nome` varchar(40) DEFAULT NULL,
  `citta` varchar(40) DEFAULT NULL,
  `indirizzo` varchar(40) DEFAULT NULL,
  `tel` varchar(15) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dump dei dati per la tabella `locali`
--

INSERT INTO `locali` (`idLocale`, `nome`, `citta`, `indirizzo`, `tel`) VALUES
(1, 'Mordi e fuggi', 'Ortanova FG', 'Via Roma 44', '0881-101010'),
(2, 'Via col vento', 'Carapelle FG', 'Via Bologna 88', '0882-210210'),
(3, 'Resta con noi', 'San Giovanni Rotondo FG', 'Via Palermo 22', '0883-310310'),
(4, 'Se scappi ti riprendo', 'Barletta BT', 'Via Napoli 11', '0884-410410'),
(5, 'Mannaggia a  te e a me', 'Cerignola FG', 'Via Torino 33', '0885-510510');

-- --------------------------------------------------------

--
-- Struttura della tabella `utenti`
--

CREATE TABLE `utenti` (
  `idUtente` int(11) NOT NULL,
  `nome` varchar(25) DEFAULT NULL,
  `cognome` varchar(25) DEFAULT NULL,
  `ruolo` varchar(10) DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `hashPassword` bigint(20) DEFAULT NULL,
  `password` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dump dei dati per la tabella `utenti`
--

INSERT INTO `utenti` (`idUtente`, `nome`, `cognome`, `ruolo`, `email`, `hashPassword`, `password`) VALUES
(1, 'sal', 'alf', 'admin', 'salvatore.alfieri.66@gmail.com', -362550379, 'pippo'),
(2, 'lac', 'mal', 'user', 'lacmal@gmail.com', -362550379, 'pippo'),
(3, 'lec', 'mel', 'user', 'lecmel@gmail.com', -362550379, 'pippo'),
(4, 'lic', 'mil', 'user', 'licmil@gmail.com', -362550379, 'pippo'),
(5, 'loc', 'mol', 'user', 'locmol@gmail.com', -362550379, 'pippo'),
(6, 'luc', 'mul', 'user', 'lucmul@gmail.com', -362550379, 'pippo');

--
-- Indici per le tabelle scaricate
--

--
-- Indici per le tabelle `incassi`
--
ALTER TABLE `incassi`
  ADD PRIMARY KEY (`idIncasso`),
  ADD KEY `idUtente` (`idUtente`),
  ADD KEY `idLocale` (`idLocale`);

--
-- Indici per le tabelle `locali`
--
ALTER TABLE `locali`
  ADD PRIMARY KEY (`idLocale`);

--
-- Indici per le tabelle `utenti`
--
ALTER TABLE `utenti`
  ADD PRIMARY KEY (`idUtente`);

--
-- AUTO_INCREMENT per le tabelle scaricate
--

--
-- AUTO_INCREMENT per la tabella `incassi`
--
ALTER TABLE `incassi`
  MODIFY `idIncasso` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT per la tabella `locali`
--
ALTER TABLE `locali`
  MODIFY `idLocale` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1789348320;

--
-- AUTO_INCREMENT per la tabella `utenti`
--
ALTER TABLE `utenti`
  MODIFY `idUtente` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- Limiti per le tabelle scaricate
--

--
-- Limiti per la tabella `incassi`
--
ALTER TABLE `incassi`
  ADD CONSTRAINT `incassi_ibfk_1` FOREIGN KEY (`idUtente`) REFERENCES `utenti` (`idUtente`),
  ADD CONSTRAINT `incassi_ibfk_2` FOREIGN KEY (`idLocale`) REFERENCES `locali` (`idLocale`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

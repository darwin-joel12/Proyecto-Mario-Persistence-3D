-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 18-06-2026 a las 05:50:02
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.1.25

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `juego_bd`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `partidas_guardadas`
--

CREATE TABLE `partidas_guardadas` (
  `id` int(11) NOT NULL,
  `jugador_id` varchar(50) NOT NULL,
  `jugador_nombre` varchar(50) NOT NULL,
  `puntuacion` int(11) DEFAULT 0,
  `posicion_x` float DEFAULT 0,
  `posicion_z` float DEFAULT 0,
  `vida` int(11) DEFAULT 100,
  `nivel` int(11) DEFAULT 1,
  `ultima_actualizacion` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `tiempo_juego` float DEFAULT 0,
  `inventario` text DEFAULT NULL,
  `posicion_y` float DEFAULT 0,
  `rotacion_y` float DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `partidas_guardadas`
--

INSERT INTO `partidas_guardadas` (`id`, `jugador_id`, `jugador_nombre`, `puntuacion`, `posicion_x`, `posicion_z`, `vida`, `nivel`, `ultima_actualizacion`, `tiempo_juego`, `inventario`, `posicion_y`, `rotacion_y`) VALUES
(1, '', '', 0, 0, 0, 0, 0, '2026-06-17 17:40:37', 0, NULL, 0, 0),
(2, 'prueba001', 'MarioPrueba', 100, 10.5, 20.3, 85, 2, '2026-06-17 17:42:39', 0, NULL, 0, 0),
(4, '2182d145-16fb-42c0-b7c9-9e4d72d8db9b', 'Mario', 640, 5.71102, 21.0137, 90, 1, '2026-06-18 03:43:57', 1533.79, 'Moneda,Moneda,Moneda,Moneda,Moneda', 3.12419, 338.581);

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `partidas_guardadas`
--
ALTER TABLE `partidas_guardadas`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `jugador_id` (`jugador_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `partidas_guardadas`
--
ALTER TABLE `partidas_guardadas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=736;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

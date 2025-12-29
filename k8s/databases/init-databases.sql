-- Script para crear las 6 databases aisladas en PostgreSQL
-- Ejecutar en DigitalOcean Managed PostgreSQL después de crear la instancia

-- Base de datos para Identity (usuarios, roles, autenticación)
CREATE DATABASE cryptojackpot_identity_db;

-- Base de datos para Lottery (loterías, premios, números)
CREATE DATABASE cryptojackpot_lottery_db;

-- Base de datos para Order (tickets, compras)
CREATE DATABASE cryptojackpot_order_db;

-- Base de datos para Wallet (billeteras, balances)
CREATE DATABASE cryptojackpot_wallet_db;

-- Base de datos para Winner (ganadores)
CREATE DATABASE cryptojackpot_winner_db;

-- Base de datos para Notification (logs de notificaciones)
CREATE DATABASE cryptojackpot_notification_db;

-- Crear usuario específico para la aplicación (opcional pero recomendado)
-- CREATE USER cryptojackpot_app WITH ENCRYPTED PASSWORD 'your_secure_password';

-- Dar permisos al usuario en cada database
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_identity_db TO cryptojackpot_app;
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_lottery_db TO cryptojackpot_app;
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_order_db TO cryptojackpot_app;
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_wallet_db TO cryptojackpot_app;
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_winner_db TO cryptojackpot_app;
-- GRANT ALL PRIVILEGES ON DATABASE cryptojackpot_notification_db TO cryptojackpot_app;


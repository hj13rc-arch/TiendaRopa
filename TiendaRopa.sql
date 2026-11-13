IF DB_ID(N'TiendaRopa') IS NULL
    EXEC(N'CREATE DATABASE TiendaRopa');
GO
USE TiendaRopa;
GO
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
    CREATE TABLE dbo.Usuarios (
        IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
        NombreUsuario NVARCHAR(50) NOT NULL UNIQUE,
        ClaveHash NVARCHAR(255) NOT NULL,
        Rol NVARCHAR(20) NOT NULL CHECK (Rol IN (N'Administrador', N'Cajero'))
    );

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
    CREATE TABLE dbo.Clientes (
        IdCliente INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Telefono NVARCHAR(20) NULL
    );

IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
    CREATE TABLE dbo.Productos (
        IdProducto INT IDENTITY(1,1) PRIMARY KEY,
        Codigo NVARCHAR(30) NOT NULL UNIQUE,
        Nombre NVARCHAR(100) NOT NULL,
        Talla NVARCHAR(15) NOT NULL,
        Color NVARCHAR(30) NOT NULL,
        Precio DECIMAL(10,2) NOT NULL CHECK (Precio > 0),
        Stock INT NOT NULL CHECK (Stock >= 0),
        Activo BIT NOT NULL DEFAULT (1)
    );

IF OBJECT_ID(N'dbo.Ventas', N'U') IS NULL
    CREATE TABLE dbo.Ventas (
        IdVenta INT IDENTITY(1,1) PRIMARY KEY,
        Fecha DATETIME2(0) NOT NULL DEFAULT (SYSDATETIME()),
        IdCliente INT NULL REFERENCES dbo.Clientes(IdCliente),
        IdUsuario INT NOT NULL REFERENCES dbo.Usuarios(IdUsuario)
    );

IF OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
    CREATE TABLE dbo.DetalleVenta (
        IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
        IdVenta INT NOT NULL REFERENCES dbo.Ventas(IdVenta),
        IdProducto INT NOT NULL REFERENCES dbo.Productos(IdProducto),
        Cantidad INT NOT NULL CHECK (Cantidad > 0),
        PrecioUnitario DECIMAL(10,2) NOT NULL CHECK (PrecioUnitario > 0),
        Subtotal AS (Cantidad * PrecioUnitario) PERSISTED,
        UNIQUE (IdVenta, IdProducto)
    );
GO

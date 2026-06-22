USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ParkingDB')
BEGIN
    ALTER DATABASE ParkingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ParkingDB;
END
GO

CREATE DATABASE ParkingDB;
GO

USE ParkingDB;
GO


CREATE TABLE KLIENCI (
    id_klienta INT PRIMARY KEY IDENTITY(1,1),
    imie VARCHAR(50) NOT NULL,
    nazwisko VARCHAR(50) NOT NULL,
    telefon VARCHAR(15),
    email VARCHAR(100) UNIQUE
);

CREATE TABLE POJAZDY (
    id_pojazdu INT PRIMARY KEY IDENTITY(1,1),
    id_klienta INT NOT NULL,
    numer_rejestracyjny VARCHAR(10) NOT NULL UNIQUE,

    FOREIGN KEY (id_klienta) 
        REFERENCES KLIENCI(id_klienta) 
        ON DELETE NO ACTION 
        ON UPDATE CASCADE
);

CREATE TABLE MIEJSCA_PARKINGOWE (
    id_miejsca INT PRIMARY KEY IDENTITY(1,1),
    poziom INT CHECK (poziom BETWEEN 0 AND 10),
    status VARCHAR(15) DEFAULT 'wolne' 
        CHECK (status IN ('wolne', 'zajęte', 'zarezerwowane'))
);

CREATE TABLE PARKOWANIA (
    id_parkowania INT PRIMARY KEY IDENTITY(1,1),
    id_pojazdu INT NOT NULL,
    id_miejsca INT NOT NULL,
    data_wjazdu DATETIME NOT NULL,
    data_wyjazdu DATETIME NULL,

    CHECK (data_wyjazdu IS NULL OR data_wyjazdu >= data_wjazdu),

    FOREIGN KEY (id_pojazdu) 
        REFERENCES POJAZDY(id_pojazdu) 
        ON DELETE NO ACTION,

    FOREIGN KEY (id_miejsca) 
        REFERENCES MIEJSCA_PARKINGOWE(id_miejsca) 
        ON DELETE NO ACTION
);

CREATE TABLE BILETY (
    id_biletu INT PRIMARY KEY IDENTITY(1,1),
    id_parkowania INT UNIQUE NOT NULL,
    data_wystawienia DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (id_parkowania) 
        REFERENCES PARKOWANIA(id_parkowania) 
        ON DELETE CASCADE
);

CREATE TABLE PLATNOSCI (
    id_platnosci INT PRIMARY KEY IDENTITY(1,1),
    id_biletu INT UNIQUE NOT NULL,
    kwota DECIMAL(10,2) NOT NULL CHECK (kwota > 0),
    status VARCHAR(15) DEFAULT 'nieopłacona' 
        CHECK (status IN ('opłacona', 'nieopłacona')),

    FOREIGN KEY (id_biletu) 
        REFERENCES BILETY(id_biletu) 
        ON DELETE CASCADE
);

CREATE TABLE REZERWACJE (
    id_rezerwacji INT PRIMARY KEY IDENTITY(1,1),
    id_klienta INT NOT NULL,
    id_miejsca INT NOT NULL,
    data_od DATETIME NOT NULL,
    data_do DATETIME NOT NULL,

    CHECK (data_do > data_od),

    FOREIGN KEY (id_klienta) 
        REFERENCES KLIENCI(id_klienta) 
        ON DELETE NO ACTION,

    FOREIGN KEY (id_miejsca) 
        REFERENCES MIEJSCA_PARKINGOWE(id_miejsca) 
        ON DELETE NO ACTION
);
GO


CREATE TRIGGER trg_sprawdz_aktywne_parkowanie
ON PARKOWANIA
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM PARKOWANIA p
        JOIN INSERTED i ON p.id_pojazdu = i.id_pojazdu
        WHERE p.data_wyjazdu IS NULL
        GROUP BY p.id_pojazdu
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR ('Pojazd ma już aktywne parkowanie', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

CREATE TRIGGER trg_sprawdz_miejsce_zajete
ON PARKOWANIA
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM PARKOWANIA p
        JOIN INSERTED i ON p.id_miejsca = i.id_miejsca
        WHERE p.data_wyjazdu IS NULL
        GROUP BY p.id_miejsca
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR ('Miejsce jest już zajęte', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

CREATE TRIGGER trg_ustaw_status_zajete
ON PARKOWANIA
AFTER INSERT
AS
BEGIN
    UPDATE MIEJSCA_PARKINGOWE
    SET status = 'zajęte'
    WHERE id_miejsca IN (SELECT id_miejsca FROM INSERTED);
END;
GO

CREATE TRIGGER trg_zwolnij_miejsce
ON PARKOWANIA
AFTER UPDATE
AS
BEGIN
    UPDATE MIEJSCA_PARKINGOWE
    SET status = 'wolne'
    WHERE id_miejsca IN (
        SELECT i.id_miejsca
        FROM INSERTED i
        JOIN DELETED d ON i.id_parkowania = d.id_parkowania
        WHERE i.data_wyjazdu IS NOT NULL
          AND d.data_wyjazdu IS NULL
    );
END;
GO

CREATE TRIGGER trg_sprawdz_rezerwacje
ON REZERWACJE
AFTER INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM REZERWACJE r
        JOIN INSERTED i ON r.id_miejsca = i.id_miejsca
        WHERE i.data_od < r.data_do
          AND i.data_do > r.data_od
          AND r.id_rezerwacji <> i.id_rezerwacji 
    )
    BEGIN
        RAISERROR ('Kolizja rezerwacji', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

CREATE TRIGGER trg_status_platnosci
ON PLATNOSCI
AFTER INSERT
AS
BEGIN
    UPDATE PLATNOSCI
    SET status = 'opłacona'
    WHERE id_platnosci IN (
        SELECT id_platnosci FROM INSERTED WHERE kwota > 0
    );
END;
GO

CREATE TRIGGER trg_usuwanie_klienta
ON KLIENCI
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM POJAZDY p
        JOIN DELETED d ON p.id_klienta = d.id_klienta
    )
    BEGIN
        RAISERROR ('Klient ma przypisane pojazdy', 16, 1);
        RETURN;
    END

    DELETE FROM KLIENCI
    WHERE id_klienta IN (SELECT id_klienta FROM DELETED);
END;
GO
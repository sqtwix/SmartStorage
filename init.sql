-- Пользователи системы
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Role" VARCHAR(50) NOT NULL, -- 'operator', 'admin', 'viewer'
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Роботы
CREATE TABLE IF NOT EXISTS "Robots" (
    "Id" VARCHAR(50) PRIMARY KEY, -- 'RB-001'
    "Status" VARCHAR(50) DEFAULT 'active',
    "BatteryLevel" INTEGER,
    "LastUpdate" TIMESTAMP,
    "CurrentZone" VARCHAR(10),
    "CurrentRow" INTEGER,
    "CurrentShelf" INTEGER
);

-- Товары
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" VARCHAR(50) PRIMARY KEY, -- 'TEL-4567'
    "Name" VARCHAR(255) NOT NULL,
    "Category" VARCHAR(100),
    "min_stock" INTEGER DEFAULT 10,
    "optimal_stock" INTEGER DEFAULT 100
);

-- История инвентаризации
CREATE TABLE IF NOT EXISTS "InventoryHistory" (
    "Id" SERIAL PRIMARY KEY,
    "RobotId" VARCHAR(50) REFERENCES "Robots"("Id"),
    "ProductId" VARCHAR(50) REFERENCES "Products"("Id"),
    "Quantity" INTEGER NOT NULL,
    "Zone" VARCHAR(10) NOT NULL,
    "RowNumber" INTEGER,
    "ShelfNumber" INTEGER,
    "Status" VARCHAR(50), -- 'OK', 'LOW_STOCK', 'CRITICAL'
    "ScannedAt" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Прогнозы ИИ
CREATE TABLE IF NOT EXISTS "AiPredictions" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" VARCHAR(50) REFERENCES "Products"("Id"),
    "PredictionDate" DATE NOT NULL,
    "DaysUntilStockout" INTEGER,
    "RecommendedOrder" INTEGER,
    "ConfidenceScore" DECIMAL(3,2),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для оптимизации
CREATE INDEX IF NOT EXISTS idx_inventory_scanned ON "InventoryHistory"("ScannedAt" DESC);
CREATE INDEX IF NOT EXISTS idx_inventory_product ON "InventoryHistory"("ProductId");
CREATE INDEX IF NOT EXISTS idx_inventory_zone ON "InventoryHistory"("Zone");

INSERT INTO "Robots" ("Id", "Status", "BatteryLevel", "LastUpdate")
VALUES ('manual_import', 'active', 100, NOW())
ON CONFLICT ("Id") DO NOTHING;

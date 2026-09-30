DROP TABLE IF EXISTS "OrderLine";

CREATE TABLE "OrderLine" (
    "OrderId" INT,
    "ProductItemNumber" INT,
    "Quantity" INT,
    "Price" NUMERIC(18, 2),
    "ProductId" INT
);
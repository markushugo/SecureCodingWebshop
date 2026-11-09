-- Indsæt testdata i Products-tabelen
USE SecureCodingWebshop;
GO

INSERT INTO Products (Name, Description, Price, IsActive) VALUES 
('Laptop', 'Højytende laptop med 16 GB RAM og 512 GB SSD.', 7999.99, 1),
('Smartphone', 'Nyeste smartphone med 6,5" skærm og triple-kamera.', 4999.99, 1),
('Højttaler', 'Trådløs højttaler med ekstra bas.', 1299.99, 1),
('Headset', 'Over-ear headset med noise-cancelling.', 1599.99, 0),
('Tablet', '10" tablet med stylus inkluderet.', 2499.99, 1);
GO
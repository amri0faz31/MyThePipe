# MyThePipe
pipeline test


database inital setting

-- Create new DB
CREATE DATABASE vetdb;
USE vetdb;

-- Create table
CREATE TABLE Vets (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  FullName VARCHAR(100) NOT NULL,
  Email VARCHAR(100) UNIQUE NOT NULL
);

-- Insert test data
INSERT INTO Vets (FullName, Email) VALUES ('Dr. Jane Smith', 'jane.smith@vetdb.com');


-- There is Person table. It contains Name field and two foriegn key for mother and father.
--

IF OBJECT_Id('dbo.Person','U') IS NOT NULL DROP TABLE dbo.Person;  
GO

CREATE TABLE dbo.Person  
(
  Id INT NOT NULL CONSTRAINT PK_Person PRIMARY KEY,
  [Name] varchar(30) NOT NULL,
  Mother INT NULL,
  Father INT NULL
);

ALTER TABLE dbo.Person
ADD CONSTRAINT FK_Mother FOREIGN KEY (Mother)
REFERENCES dbo.Person (Id)
ALTER TABLE dbo.Person
ADD CONSTRAINT FK_Father FOREIGN KEY (Father)
REFERENCES dbo.Person (Id)



INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 1, 'Balbo Baggins'  
  INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 2, 'Berylla Boffin'  
    INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 3, 'Mungo Baggins'  
      , NULL, NULL)
    , NULL, NULL)
  ,    1,    2)
INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 4, 'Laura Grubb'  
  INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 5, 'Bungo Baggins'  
    INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 6, 'Belladonna Took'  
      INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 7, 'Bilbo Baggins'  
        INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 8, 'Largo Baggins'
         
          INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES( 9, 'Tanta Hornblower' , NULL, NULL)  
          INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(10, 'Fosco Baggins'
           
            INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(11, 'Ruby Bolger'
             
              INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(12, 'Dora Baggins'
               
                INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(13, 'Drogo Baggins'
                 
                  INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(14, 'Dudo Baggins'
                   
                    INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(15, 'Primula Brandybuck', NULL, NULL)  
                    INSERT INTO dbo.Person(Id, [Name], Mother, Father) VALUES(16, 'Frodo Baggins' , 13, 15)  
                    SELECT * FROM dbo.Person
                    
--Query looks for ancestors using a CTE with Multiple Recursive Members for person with id = 16  
DECLARE @Id AS INT
SET @Id = 16;

, NULL, NULL)
                  ,    3,    4)
                , NULL, NULL)
              ,    5,    6)
            ,    1,    2)
,    8,    9)
, NULL, NULL)
,   10,   11)
,   10,   11)
,   10,   11)
WITH C AS (
-- anchor queries  -- Father of input
SELECT Father AS Id, 1 AS lvl
FROM dbo.Person
WHERE Id = @Id
AND Father IS NOT NULL  UNION ALL
  -- Mother of input
  SELECT Mother AS Id, 1 AS lvl
  FROM dbo.Person
  WHERE Id = @Id
  AND Mother IS NOT NULL  UNION ALL
 -- recursive queries
  -- Fathers of those in previous level
  SELECT Father AS Id, C.lvl + 1 AS lvl
  FROM C
  JOIN dbo.Person AS P
  ON C.Id = P.Id
  WHERE P.Father IS NOT NULL
  UNION ALL
-- Fathers of those in previous level


SELECT Mother AS Id, C.lvl + 1 AS lvl
FROM C
JOIN dbo.Person AS P
ON C.Id = P.Id
WHERE P.Mother IS NOT NULL
) 
SELECT C.Id, FT.name, lvl  
FROM C
JOIN dbo.Person AS FT
ON C.Id = FT.Id;
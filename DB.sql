DROP TABLE IF EXISTS Login;
DROP TABLE IF EXISTS StoricoTessereAllenatori;
DROP TABLE IF EXISTS StoricoTessereAtleti;
DROP TABLE IF EXISTS Partita;
DROP TABLE IF EXISTS Partecipa;
DROP TABLE IF EXISTS ParametroTorneo;
DROP TABLE IF EXISTS ParametroQualita;
DROP TABLE IF EXISTS ListaIscritti;
DROP TABLE IF EXISTS ImpiantoTorneo;
DROP TABLE IF EXISTS ImpiantoSocieta;
DROP TABLE IF EXISTS Impianto;
DROP TABLE IF EXISTS ArBITraTorneo;
DROP TABLE IF EXISTS Torneo;
DROP TABLE IF EXISTS FormulaTorneo;
DROP TABLE IF EXISTS TipoTorneo;
DROP TABLE IF EXISTS Allenatore;
DROP TABLE IF EXISTS Squadra;
DROP TABLE IF EXISTS Atleta;
DROP TABLE IF EXISTS Societa;
DROP TABLE IF EXISTS DelegatoTecnico;
DROP TABLE IF EXISTS Comune;

CREATE TABLE Comune(
	IDComune NVARCHAR (4) NOT NULL PRIMARY KEY,
	Citta NVARCHAR (50) NOT NULL,
    Provincia NVARCHAR (30) NOT NULL,
    Regione NVARCHAR (20) NOT NULL,
	SiglaProvincia NVARCHAR (2) NOT NULL,
	SiglaRegione NVARCHAR (2) NOT NULL
);

CREATE TABLE DelegatoTecnico(
	IDDelegato INTEGER NOT NULL PRIMARY KEY IDENTITY,
	Nome NVARCHAR (20) NOT NULL,
    Cognome NVARCHAR (20) NOT NULL,
	Sesso CHAR,
	CF NVARCHAR (20) NOT NULL,
	DataNascita date  NOT NULL,
	IDComuneNascita NVARCHAR (4),
	IDComuneResidenza NVARCHAR (4),
	Indirizzo NVARCHAR (50),
	CAP NVARCHAR (5),
	Email NVARCHAR (100) NOT NULL,
	Tel NVARCHAR (20),
	ArBITro BIT NOT NULL,
	Supervisore BIT NOT NULL,
	FOREIGN KEY (IDComuneNascita) REFERENCES Comune (IDComune),
	FOREIGN KEY (IDComuneResidenza) REFERENCES Comune (IDComune) 
);

CREATE TABLE Societa(
	IDSocieta INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDComune NVARCHAR (4) NOT NULL,
	NomeSocieta NVARCHAR (50) NOT NULL,
	Indirizzo NVARCHAR (50) NOT NULL,	
	CAP NVARCHAR (5),
    DataFondazione date NOT NULL,
	DataAffiliazione date NOT NULL,
	CodiceAffiliazione NVARCHAR (10),
	Affiliata BIT,
	Email NVARCHAR (100) NOT NULL,
	Sito NVARCHAR (50),
	Tel1 NVARCHAR (20),
	Tel2 NVARCHAR (20),
	Pec  NVARCHAR (50),	
	PIVA  NVARCHAR (11),
	CF  NVARCHAR (16),
	CU  NVARCHAR (6),
	FOREIGN KEY (IDComune) REFERENCES Comune (IDComune) 
);

CREATE TABLE Atleta(
	IDAtleta INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDSocieta INTEGER NOT NULL,
	CodiceTessera NVARCHAR (20) NOT NULL,
	Nome NVARCHAR (40) NOT NULL,
    Cognome NVARCHAR (40) NOT NULL,
	Sesso CHAR,
	CF NVARCHAR (20) NOT NULL,
	DataNascita date NOT NULL,
	IDComuneNascita NVARCHAR (4),
	IDComuneResidenza NVARCHAR (4),
	Indirizzo NVARCHAR (50),
	CAP NVARCHAR (5),
	Email NVARCHAR (100) NOT NULL,
	Tel NVARCHAR (20),
	Altezza INTEGER DEFAULT 0,
	Peso INTEGER DEFAULT 0,
	DataScadenzaCertificato date,
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta),
	FOREIGN KEY (IDComuneNascita) REFERENCES Comune (IDComune),
	FOREIGN KEY (IDComuneResidenza) REFERENCES Comune (IDComune) 
);

CREATE TABLE Squadra(
	IDSquadra INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDAtleta1 INTEGER NOT NULL,
	IDAtleta2 INTEGER NOT NULL,
	Confermata BIT,
	NomeTeam NVARCHAR (50),
	FOREIGN KEY (IDAtleta1) REFERENCES Atleta (IDAtleta),
	FOREIGN KEY (IDAtleta2) REFERENCES Atleta (IDAtleta) 
);

CREATE TABLE Allenatore(
	IDAllenatore INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDSocieta INTEGER,
	CodiceTessera NVARCHAR (20) NOT NULL,
	Grado NVARCHAR (30) NOT NULL,
	Nome NVARCHAR (40) NOT NULL,
    Cognome NVARCHAR (40) NOT NULL,
	Sesso CHAR,
	CF NVARCHAR (20) NOT NULL,
	DataNascita date  NOT NULL,
	IDComuneNascita NVARCHAR (4),
	IDComuneResidenza NVARCHAR (4),
	Indirizzo NVARCHAR (50),
	CAP NVARCHAR (5),
	Email NVARCHAR (100) NOT NULL,
	Tel NVARCHAR (20),
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta),
	FOREIGN KEY (IDComuneNascita) REFERENCES Comune (IDComune),
	FOREIGN KEY (IDComuneResidenza) REFERENCES Comune (IDComune)	
);

CREATE TABLE TipoTorneo(
	IDTipoTorneo INTEGER NOT NULL PRIMARY KEY,
	Descrizione NVARCHAR (50) NOT NULL,
	Note NVARCHAR (200)
);

CREATE TABLE FormulaTorneo(
	IDFormula INTEGER NOT NULL PRIMARY KEY,
	Formula NVARCHAR (50) NOT NULL,
	Descrizione NVARCHAR (1000)
);	

CREATE TABLE Torneo(
	IDTorneo INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDTipoTorneo INTEGER NOT NULL,
	IDSupervisore INTEGER NOT NULL,
    IDSupervisoreArbitrale INTEGER,
    IDDirettoreCompetizione INTEGER,
	IDFormula INTEGER NOT NULL,
	QuotaIscrizione DECIMAL(3,2),
	PuntiVittoria INTEGER NOT NULL,
	Montepremi DECIMAL(6,2)  NOT NULL,
	DataInizio date NOT NULL,
	DataFine date NOT NULL,
	Gender CHAR NOT NULL,
	NumTeamTabellone INTEGER NOT NULL,
	NumTeamQualifiche INTEGER NOT NULL,
	FOREIGN KEY (IDTipoTorneo) REFERENCES TipoTorneo (IDTipoTorneo),
	FOREIGN KEY (IDSupervisore) REFERENCES DelegatoTecnico (IDDelegato), 
	FOREIGN KEY (IDSupervisoreArbitrale) REFERENCES DelegatoTecnico (IDDelegato), 
	FOREIGN KEY (IDDirettoreCompetizione) REFERENCES DelegatoTecnico (IDDelegato),
	FOREIGN KEY (IDFormula) REFERENCES FormulaTorneo (IDFormula)	
);

CREATE TABLE ArBITraTorneo(
	IDTorneo INTEGER NOT NULL,
	IDDelegato INTEGER NOT NULL,
	MezzaGiornata BIT,
	PRIMARY KEY (IDTorneo,IDDelegato),
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo),
	FOREIGN KEY (IDDelegato) REFERENCES DelegatoTecnico (IDDelegato) 
);

CREATE TABLE Impianto(
	IDImpianto INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDComune NVARCHAR (4) NOT NULL,
	NomeImpianto NVARCHAR (100) NOT NULL,
	NumeroCampi INTEGER NOT NULL,
    Indirizzo NVARCHAR (100) NOT NULL,
	CAP NVARCHAR (5),
	Descrizione NVARCHAR (200),
	Email NVARCHAR (100),
	Sito NVARCHAR (50),
	Tel NVARCHAR (20),
	FOREIGN KEY (IDComune) REFERENCES Comune (IDComune) 
);

CREATE TABLE ImpiantoSocieta(
	IDSocieta INTEGER NOT NULL,
	IDImpianto INTEGER NOT NULL,
	PRIMARY KEY (IDSocieta,IDImpianto),
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta),
	FOREIGN KEY (IDImpianto) REFERENCES Impianto (IDImpianto) 
);

CREATE TABLE ImpiantoTorneo(
	IDTorneo INTEGER NOT NULL,
	IDImpianto INTEGER  NOT NULL,
	PRIMARY KEY (IDTorneo,IDImpianto),
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo),
	FOREIGN KEY (IDImpianto) REFERENCES Impianto (IDImpianto) 
);

CREATE TABLE ListaIscritti(
	IDSquadra INTEGER NOT NULL,
	IDTorneo INTEGER  NOT NULL,
	IDAllenatore INTEGER  NOT NULL,
	EntryPoints DECIMAL (6,2) DEFAULT 0,
	DataIscrizione date NOT NULL,
    Cancellata  BIT  NOT NULL,
	PRIMARY KEY (IDSquadra,IDTorneo),
	FOREIGN KEY (IDSquadra) REFERENCES Squadra (IDSquadra),
	FOREIGN KEY (IDAllenatore) REFERENCES Allenatore (IDAllenatore), 
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo) 
);


CREATE TABLE ParametroQualita(
	IDParametro INTEGER NOT NULL PRIMARY KEY IDENTITY,
	NomeParametro NVARCHAR (100) NOT NULL,
	ValoreParametro NVARCHAR (100) NOT NULL
);

CREATE TABLE ParametroTorneo(
	IDTorneo INTEGER NOT NULL,
	IDParametro INTEGER NOT NULL,
	PRIMARY KEY (IDTorneo,IDParametro),
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo),
	FOREIGN KEY (IDParametro) REFERENCES ParametroQualita (IDParametro) 
);

CREATE TABLE Partecipa(
	IDSquadra INTEGER NOT NULL,
	IDTorneo INTEGER  NOT NULL,
	IDAllenatore INTEGER  NOT NULL,
	EntryPoints DECIMAL (6,2) DEFAULT 0,
	PosizioneFinale INTEGER NOT NULL,
	Punti INTEGER NOT NULL,
    Montepremi DECIMAL(5,2)  NOT NULL,
	PRIMARY KEY (IDSquadra,IDTorneo),
	FOREIGN KEY (IDSquadra) REFERENCES Squadra (IDSquadra),
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo),
	FOREIGN KEY (IDAllenatore) REFERENCES Allenatore (IDAllenatore) 
);


CREATE TABLE Partita(
	IDPartita INTEGER NOT NULL PRIMARY KEY IDENTITY,
	IDSQ1 INTEGER NOT NULL,
	IDSQ2 INTEGER NOT NULL,
	IDArBITro1 INTEGER NOT NULL,
	IDArBITro2 INTEGER,
	IDTorneo INTEGER NOT NULL,
	NumPartita INTEGER NOT NULL,
    Campo NVARCHAR(20) NOT NULL,
	DataPartita date NOT NULL,
	OraPartita time NOT NULL,
	PT1S1 INTEGER NOT NULL,
	PT2S1 INTEGER NOT NULL,
	PT1S2 INTEGER NOT NULL,
	PT2S2 INTEGER NOT NULL,
	PT1S3 INTEGER NOT NULL,
	PT2S3 INTEGER NOT NULL,
	FOREIGN KEY (IDSQ1) REFERENCES Squadra (IDSquadra),
	FOREIGN KEY (IDSQ2) REFERENCES Squadra (IDSquadra),
	FOREIGN KEY (IDTorneo) REFERENCES Torneo (IDTorneo),
	FOREIGN KEY (IDArBITro1) REFERENCES DelegatoTecnico (IDDelegato),
	FOREIGN KEY (IDArBITro2) REFERENCES DelegatoTecnico (IDDelegato)
);

CREATE TABLE StoricoTessereAtleti(
	IDAtleta INTEGER NOT NULL,
	IDSocieta INTEGER NOT NULL,
	CodiceTessera NVARCHAR (20) NOT NULL,
	TipoTessera NVARCHAR (10),
	DataTesseramento date NOT NULL,
	AnnoTesseramento INTEGER NOT NULL,
	Importo DECIMAL (3,2),
	PRIMARY KEY (IDAtleta,AnnoTesseramento),
	FOREIGN KEY (IDAtleta) REFERENCES Atleta (IDAtleta),
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta) 
);

CREATE TABLE StoricoTessereAllenatori(
	IDAllenatore INTEGER NOT NULL,
	IDSocieta INTEGER,
	CodiceTessera NVARCHAR (20) NOT NULL,
	TipoTessera NVARCHAR (10),
	DataTesseramento date NOT NULL,
	AnnoTesseramento INTEGER NOT NULL,
	Importo DECIMAL (3,2),
	PRIMARY KEY (IDAllenatore,AnnoTesseramento),
	FOREIGN KEY (IDAllenatore) REFERENCES Allenatore (IDAllenatore),
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta) 
);

CREATE TABLE Login(
	Email NVARCHAR (50) NOT NULL PRIMARY KEY,
	PWD NVARCHAR (50) NOT NULL,
	IDSocieta INTEGER,
	IDDelegato INTEGER,
	IDAtleta INTEGER,
	IDAllenatore INTEGER,
	DataUltimoCambioPwd DATE NOT NULL,
	DataRichiestaCambioPwd DATE NOT NULL,
	DataUltimoAccesso DATE NOT NULL,
	Bloccato BIT DEFAULT 0,
	Admin BIT DEFAULT 0,
	FOREIGN KEY (IDSocieta) REFERENCES Societa (IDSocieta),
	FOREIGN KEY (IDDelegato) REFERENCES DelegatoTecnico (IDDelegato),
	FOREIGN KEY (IDAtleta) REFERENCES Atleta (IDAtleta),
	FOREIGN KEY (IDAllenatore) REFERENCES Allenatore (IDAllenatore)
);


INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('H294','Rimini','Rimini','Emilia Romagna','RN','ER');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('C574','Cesenatico','Forlì-Cesena','Emilia Romagna','FC','ER');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('A944','Bologna','Bologna','Emilia Romagna','BO','ER');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('A271','Ancona','Ancona','Marche','AN','MA');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('H501','Roma','Roma','Lazio','RM','LA');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('G337','Parma','Parma','Emilia Romagna','PR','ER');
INSERT INTO Comune (IDComune,Citta,Provincia,Regione,SiglaProvincia,SiglaRegione)VALUES('F205','Milano','Milano','Lombardia','MI','LO');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Beach Volley University','Via Don Giovanni Minzoni 15','47042','C574','2010-07-10','2020-08-06','EM-432233',1,'info@beachvolleyuniversity.it','www.beachvolleyuniversity.it','3472422495','','bvu@pec.it');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Beach Volley Rimini','Viale Roma 1','47900','H294','2010-07-10','2020-08-06','EM-111111',1,'info@beachvolleyrimini.it','www.beachvolleyrimini.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Beach Volley School','Viale Verdi 1','','A271','2010-07-10','2020-08-06','MA-111111',1,'info@bvs.it','www.bvs.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Dream Beach','Viale Milano 1','','A944','2010-07-10','2020-08-06','MA-111111',1,'info@dreambeach.it','www.dreambeach.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Active Beach','Viale Carducci 1','','A271','2010-07-10','2020-08-06','MA-111111',1,'info@active.it','www.active.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Beach Volley Bologna','Viale Marche 1','','A271','2010-07-10','2020-08-06','MA-111111',1,'info@bbv.it','www.bbv.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('GTA','Viale Cardine 16','','F205','2010-07-10','2020-08-06','LO-111111',1,'info@gta.it','www.gta.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Beach Volley Academy','Viale Italia 3','','H501','2010-07-10','2020-08-06','LA-111111',1,'info@bva.it','www.bva.it','','','');
INSERT INTO Societa(NomeSocieta,Indirizzo,CAP,IDComune,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec) VALUES('Urban Beach','Via Doria 4','','H501','2010-07-10','2020-08-06','LA-111111',1,'info@urban.it','www.urban.it','','','');
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(2,'Niccolò','Bertozzi','111','BRTNCL','2002-11-08','M','bertoz@gmail.com','',181,72);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(2,'Matteo','Brocchi','222','MTBRC','2002-01-01','M','matbroc@gmail.com','',182,70);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(2,'Alessandro','Boldrini','333','ALSBLD','2002-09-10','M','ale.boldro@gmail.com','',179,71);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(2,'Edoardo','Casali','444','EDRCSL','2002-06-13','M','casali.edo@gmail.com','',183,75);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Thomas','Casali','112233','CSLTMS','1972-03-20','M','casali.thomas@gmail.com','3472422495',182,90);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Fabio','Casali','22233','CSLFBA','1979-10-03','M','casali.fabio@gmail.com','',181,80);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Maria','Bianchi','3334444','BNCMRO','1985-12-07','F','bianchi.maria@gmail.com','',175,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Giovanna','Verdi','','','1985-12-07','F','maria@gmail.com','',175,66);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Marta','Corso','','','1986-10-07','F','marta@gmail.com','',176,67);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Giulia','Toti','','','1987-11-07','F','giulia@gmail.com','',177,68);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Eleonora','Annibalini','','','1988-01-07','F','eleonora@gmail.com','',178,69);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Nicol','Bertozzi','','','1989-02-07','F','nicol@gmail.com','',179,70);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Bianca','Mazzotti','','','1990-03-07','F','bianca@gmail.com','',180,71);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Sara','Leoni','','','1991-04-07','F','sara@gmail.com','',181,72);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Chiara','Ferretti','','','1992-12-07','F','chiararr@gmail.com','',174,73);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Anna','Dalmazzo','','','1993-11-07','F','maria@gmail.com','',173,74);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Valentina','Gottardi','','','1994-12-07','F','valentina@gmail.com','',172,75);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Marta','Menegatti','','','1995-05-07','F','martazza@gmail.com','',171,76);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Irene','Enzo','','','1996-06-07','F','irene@gmail.com','',170,77);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Margherita','Bianchin','','','1997-07-07','F','margherita@gmail.com','',169,78);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Jessica','Allengretti','','','1998-08-07','F','jessica@gmail.com','',168,79);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Debora','Allegretti','','','1999-09-07','F','debora@gmail.com','',182,80);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Giorgia','Gianandrea','','','2000-10-07','F','giorgia@gmail.com','',183,81);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Eleonora','Mansueti','','','2001-11-07','F','eleonora@gmail.com','',184,62);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Silvia','Leonardi','','','2002-12-07','F','silvia@gmail.com','',175,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Rosa','Natalini','','','2003-11-07','F','rosa@gmail.com','',165,64);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Anna','Santi','','','2004-10-07','F','anaaaaa@gmail.com','',165,66);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Martina','Foeresti','','','1985-09-07','F','martinafor@gmail.com','',179,75);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Alice','Pratesi','','','1986-08-07','F','alice@gmail.com','',167,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Ludovica','Rossi','','','1987-07-07','F','ludo@gmail.com','',180,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Chiara','Maestri','','','1988-06-07','F','chiararrra@gmail.com','',175,67);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Valentina','Vizio','','','1989-02-07','F','vale@gmail.com','',176,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Serena','Fazzini','','','1985-02-07','F','serena@gmail.com','',177,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Anna','Baggio','','','1986-01-07','F','baggio@gmail.com','',178,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Silvia','Brilli','','','1981-02-07','F','silviab@gmail.com','',175,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Anna','Brilli','','','1982-05-07','F','annab@gmail.com','',178,64);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Denise','Merighi','','','1983-06-07','F','denise@gmail.com','',175,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Chiara','Baili','','','1984-05-07','F','chiarabai@gmail.com','',198,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(2,'Elisa','Mazza','','','1985-11-07','F','elisamazza@gmail.com','',195,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Elisa','Casali','','','2006-08-13','F','elisacasali@gmail.com','',175,59);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Mila','Casali','','','2004-08-17','F','milacasali@gmail.com','',165,62);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Flavia','Fiori','','','1985-05-07','F','flavia@gmail.com','',175,69);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Francesca','Gori','','','1981-02-07','F','francescagori@gmail.com','',185,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Giulio','Moti','','','1987-11-07','M','giulio@gmail.com','',187,78);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Elon','Annibalini','','','1988-01-07','M','elon@gmail.com','',188,79);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Emil','Bertozzi','','','1989-02-07','M','emil@gmail.com','',189,80);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(3,'Bianco','Razzotti','','','1990-03-07','M','bianco@gmail.com','',190,81);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Sandro','Meoni','','','1991-04-07','M','sandro@gmail.com','',191,82);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Cristian','Derretti','','','1992-12-07','M','cristian@gmail.com','',184,83);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Mirco','Palmazzo','','','1993-11-07','M','mirco@gmail.com','',183,84);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(4,'Valentino','Lottardi','','','1994-12-07','M','valentino@gmail.com','',182,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Marco','Penegatti','','','1995-05-07','M','marco@gmail.com','',171,76);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Aron','Renzo','','','1996-06-07','M','aron@gmail.com','',170,77);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Marco','Rianchin','','','1997-07-07','M','rianchin@gmail.com','',199,88);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(5,'Jan','Allengretti','','','1998-08-07','M','jan@gmail.com','',198,89);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Pier','Allegretti','','','1999-09-07','M','pier@gmail.com','',192,90);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Giorgio','Tianandrea','','','2000-10-07','M','giorgio@gmail.com','',193,91);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Antonio','Dansueti','','','2001-11-07','M','antonio@gmail.com','',194,82);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(6,'Silvio','Teonardi','','','2002-12-07','M','silvio@gmail.com','',195,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Rossano','Pratalini','','','2003-11-07','M','rossano@gmail.com','',195,64);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Tom','Santi','','','2004-10-07','M','tommmm@gmail.com','',195,76);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Martino','Potresti','','','1985-09-07','M','martino@gmail.com','',179,75);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(7,'Al','Cratesi','','','1986-08-07','M','alcra@gmail.com','',177,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Ludovico','Grossi','','','1987-07-07','M','ludovico@gmail.com','',190,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Mike','Maestri','','','1988-06-07','M','mike@gmail.com','',185,67);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Valentino','Vizioso','','','1989-02-07','M','vizioso@gmail.com','',186,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(8,'Sereno','Pazzini','','','1985-02-07','M','sereno@gmail.com','',187,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Roberto','Raggio','','','1986-01-07','M','baggiorob@gmail.com','',188,85);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Silvio','Brillo','','','1981-02-07','M','silviob@gmail.com','',185,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Roby','Grilli','','','1982-05-07','M','robyg@gmail.com','',188,64);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(9,'Den','Perighi','','','1983-06-07','M','denper@gmail.com','',185,65);
INSERT INTO Atleta(IDSocieta,Nome,Cognome,CodiceTessera,CF,DataNascita,Sesso,Email,Tel,Altezza,Peso)VALUES(1,'Conan','Maili','','','1984-05-07','M','conan@gmail.com','',198,65);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,1,'111','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,2,'222','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,3,'333','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,4,'444','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,5,'555','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,6,'666','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,7,'777','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,8,'888','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,9,'999','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,10,'112','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,11,'113','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,12,'114','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,13,'115','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,14,'116','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,15,'117','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,16,'118','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,17,'119','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,18,'200','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,19,'201','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,20,'202','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,21,'203','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,22,'204','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,23,'205','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,24,'206','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,25,'207','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,26,'208','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,27,'309','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,28,'210','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,29,'211','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,30,'212','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,31,'213','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,32,'214','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,33,'215','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,34,'216','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,35,'217','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,36,'218','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,37,'219','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,38,'220','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,39,'221','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,40,'223','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,41,'224','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,42,'225','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,43,'226','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,44,'227','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,45,'228','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,46,'229','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,47,'230','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,48,'231','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,49,'232','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,50,'233','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,51,'234','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,52,'235','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,53,'236','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,54,'237','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,55,'238','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,56,'239','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,57,'240','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,58,'241','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,59,'242','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,60,'243','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,61,'244','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,62,'245','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,63,'246','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,64,'247','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,65,'248','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,66,'249','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,67,'250','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,68,'251','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,69,'252','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,70,'253','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,71,'254','B','2020-10-08',2020);
INSERT INTO StoricoTessereAtleti(IDSocieta,IDAtleta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,72,'255','B','2020-10-08',2020);
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(1,2,'Team Blu');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(3,4,'Team Barcollo');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(5,6,'Team 1');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(7,8,'Team 2');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(9,10,'Team 3');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(11,12,'Team 4');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(13,14,'Team 5');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(15,16,'Team 6');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(17,18,'Team 7');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(19,20,'Team 8');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(21,22,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(23,24,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(25,26,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(27,28,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(29,30,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(31,32,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(33,34,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(35,36,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(37,38,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(40,39,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(41,42,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(43,44,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(45,46,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(47,48,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(49,50,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(51,52,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(53,54,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(55,56,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(57,58,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(59,60,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(61,62,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(63,64,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(65,66,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(67,68,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(69,70,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(71,72,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(63,70,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(71,61,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(69,59,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(60,70,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(59,65,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(58,64,'');
INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)VALUES(57,63,'');
INSERT INTO TipoTorneo(IDTipoTorneo,Descrizione,Note)VALUES(1,'L1','Tornei di primo livello con montepremi minimo di 500 Euro');
INSERT INTO TipoTorneo(IDTipoTorneo,Descrizione,Note)VALUES(2,'L2','Tornei di secondo livello senza montepremi');
INSERT INTO TipoTorneo(IDTipoTorneo,Descrizione,Note)VALUES(3,'L3','Tornei di terzo livello che non generano punti per la classifica generale');
INSERT INTO ParametroQualita(NomeParametro,ValoreParametro)VALUES('Speaker',10);
INSERT INTO ParametroQualita(NomeParametro,ValoreParametro)VALUES('Diretta Web',10);
INSERT INTO ParametroQualita(NomeParametro,ValoreParametro)VALUES('Ospitalità  per gli atleti',10);
INSERT INTO ParametroQualita(NomeParametro,ValoreParametro)VALUES('Catering per gli atleti',10);
INSERT INTO ParametroQualita(NomeParametro,ValoreParametro)VALUES('Presenza di tribuna da almeno 200 posti',10);
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('PalaBVU','Via Don Giovanni Minzoni 15','C574',4,'Impianto sportivo dedicato agli sport da spiaggia con 4 campi in sabbia bianca...','info@palabvu.it','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Garden','Via Aldo Moro 1','H294',4,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Beach Stadium','Via Mora 1','A271',6,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Palabeach','Via Satellite 1','A271',4,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('PalaBVA','Via Luna 1','H501',8,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Gioka','Via Bologna 1','A944',6,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Country','Via Marte 1','A944',8,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('PalaUno','Via Mercurio 1','F205',5,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('OpenBeach','Via Giove 1','F205',2,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Beach Paradise','Via Venere 1','F205',3,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Beach Center','Via Saturno 1','F205',4,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Playa Bonita','Via Plutone 1','G337',4,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('Parma Beach Arena','Via Nettuno 1','G337',5,'','','','');
INSERT INTO Impianto(NomeImpianto,Indirizzo,IDComune,NumeroCampi,Descrizione,Email,Sito,Tel)VALUES('PalaUrban','Via Urban 1','H501',3,'','','','');
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(1,1);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(2,2);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(3,4);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(4,7);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(5,6);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(6,6);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(7,8);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(8,5);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(9,14);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(1,2);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(4,9);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(8,10);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(5,11);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(7,12);
INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto)VALUES(3,13);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Marco','Frigatti','MCFRT','frigatti@gmail.com','1987-08-12',1,0);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Franco','Giorgetti','GRGGR','giorgetti@gmail.com','1981-10-02',1,0);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Led','Amadori','','led@gmail.com','1991-11-06',1,0);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Max','Fanucci','','max@gmail.com','1991-11-06',0,1);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Daniele','Rossi','','danired@gmail.com','1991-11-06',1,1);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Marco','Delvecchio','','marcdelv@gmail.com','1991-11-06',1,0);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Roberto','Manzi','','robyma@gmail.com','1991-11-06',0,1);
INSERT INTO DelegatoTecnico(Nome,Cognome,CF,Email,DataNascita,Arbitro,Supervisore)VALUES('Andrea','Rosati','','andrearo@gmail.com','1991-11-06',1,0);
INSERT INTO FormulaTorneo(IDFormula,Formula,Descrizione)VALUES(1,'World Tour','Il torneo si svolgere con una fase iniziale a gironi con la formula della Pool play e poi ad eliminazione diretta');
INSERT INTO FormulaTorneo(IDFormula,Formula,Descrizione)VALUES(2,'Doppia eliminazione','Il torneo si svolge con la formula del vincenti/perdenti');
INSERT INTO FormulaTorneo(IDFormula,Formula,Descrizione)VALUES(3,'Singola eliminazione','Torneo ad eliminazione diretta');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-123',1,'Valentina','Bellucci','','F','1979-05-19','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-234',2,'Mario','Rossi','','M','1972-03-21','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-334',1,'Gianlu','Casadei','','M','1982-12-22','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-434',1,'Marco','Negri','','M','1992-11-23','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-534',1,'Paolo','Fico','','M','1976-10-24','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-634',1,'Gianni','Mascagna','','M','1969-09-25','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-734',1,'Andrea','Raffaelli','','M','1966-08-26','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-834',2,'Marco','Monticelli','','M','1993-07-27','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-934',2,'Carlo','Ferlo','','M','1992-06-28','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-244',2,'Aron','Monte','','M','1992-05-29','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-254',2,'Caterina','Marini','','F','1962-04-30','','');
INSERT INTO Allenatore(CodiceTessera,Grado,Nome,Cognome,CF,Sesso,DataNascita,Email,Tel)VALUES('ALL-111',1,'Thomas','Casali','','M','1972-03-20','','');
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(1,1,'ALL-123','AL','2020-09-08',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(2,1,'ALL-234','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(3,2,'ALL-334','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(4,3,'ALL-434','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(5,4,'ALL-534','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(6,5,'ALL-634','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(7,6,'ALL-734','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(8,7,'ALL-834','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(9,8,'ALL-934','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(10,9,'ALL-244','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(11,2,'ALL-254','AL','2020-09-10',2020);
INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento)VALUES(12,1,'ALL-111','AL','2020-09-10',2020);
INSERT INTO Torneo(IDTipoTorneo,IDSupervisore,IDSupervisoreArbitrale,IDDirettoreCompetizione,IDFormula,QuotaIscrizione,PuntiVittoria,Montepremi,Gender,DataInizio,DataFine,NumTeamTabellone,NumTeamQualifiche)VALUES(1,3,NULL,NULL,1,NULL,21,3500,'M','2020-12-10','2020-12-10',16,24);INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato)VALUES(1,1);
INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato)VALUES(1,2);
INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato)VALUES(1,4);
INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato)VALUES(1,6);
INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato)VALUES(1,8);
INSERT INTO ParametroTorneo(IDTorneo,IDParametro)VALUES(1,1);
INSERT INTO ParametroTorneo(IDTorneo,IDParametro)VALUES(1,2);
INSERT INTO ImpiantoTorneo(IDTorneo,IDImpianto)VALUES(1,1);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(1,1,1,'2020-12-05',100,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(2,1,1,'2020-12-06',0,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(3,1,2,'2020-12-06',150,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(4,1,1,'2020-12-07',1400,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(5,1,2,'2020-12-07',1201,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(6,1,3,'2020-12-07',203,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(7,1,1,'2020-12-08',434,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(8,1,1,'2020-12-09',434.5,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(9,1,4,'2020-12-10',250,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(10,1,1,'2020-12-11',1000,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(11,1,12,'2020-12-12',1200,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(12,1,2,'2020-12-12',0,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(13,1,3,'2020-12-13',10,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(14,1,5,'2020-12-14',5,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(15,1,8,'2020-12-15',333,0);
INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,EntryPoints,Cancellata)VALUES(16,1,9,'2020-12-15',442,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(4,1,1,1400,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(5,1,2,1201,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(11,1,12,1200,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(10,1,2,1000,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(8,1,3,434.5,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(7,1,2,434,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(16,1,9,442,0,0,0);
INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti,Montepremi,PosizioneFinale)VALUES(15,1,8,333,0,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,4,15,1,1,1,1,'2020-12-31','10:30',21,10,18,21,15,10);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,5,16,2,2,2,2,'2020-12-31','10:30',21,19,13,21,15,5);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,11,7,3,3,3,1,'2020-12-31','12:00',19,21,18,21,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,10,8,4,2,4,2,'2020-12-31','12:00',0,0,0,0,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,4,5,1,3,5,1,'2020-12-31','14:00',0,0,0,0,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,7,2,2,3,6,2,'2020-12-31','14:00',0,0,0,0,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,1,3,3,4,7,1,'2020-12-31','15:30',0,0,0,0,0,0);
INSERT INTO Partita(IDTorneo,IDSQ1,IDSQ2,IDArbitro1,IDArbitro2,NumPartita,Campo,DataPartita,OraPartita,PT1S1,PT2S1,PT1S2,PT2S2,PT1S3,PT2S3)VALUES(1,2,3,4,5,8,1,'2020-12-31','16:30',0,0,0,0,0,0);
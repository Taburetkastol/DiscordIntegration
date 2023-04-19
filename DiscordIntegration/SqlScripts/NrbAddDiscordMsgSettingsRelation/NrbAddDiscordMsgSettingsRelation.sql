IF OBJECT_ID('DiscordMsgSettings') IS NULL 
	BEGIN
	CREATE TABLE DiscordMsgSettings(
		Id uniqueidentifier NOT NULL DEFAULT (newid()),
		Token nvarchar(250) NOT NULL DEFAULT (''),
		UserName nvarchar(250) NOT NULL DEFAULT (''),
		CONSTRAINT PK_DiscordMsgSettings_Id PRIMARY KEY CLUSTERED (Id)
	)
END
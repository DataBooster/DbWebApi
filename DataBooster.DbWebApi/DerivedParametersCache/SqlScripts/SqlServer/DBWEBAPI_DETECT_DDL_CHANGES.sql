IF OBJECT_ID(N'dbo.DBWEBAPI_DETECT_DDL_CHANGES') IS NULL
	EXECUTE ('CREATE PROCEDURE dbo.DBWEBAPI_DETECT_DDL_CHANGES AS
BEGIN
	SET NOCOUNT ON;
END');
GO

-- Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
-- Licensed under the MIT license. See LICENSE file in the project root for full license information.

-- ================================================================
-- Original Author:	Abel Cheng <abelcys@gmail.com>
-- Create date:		2015-04-15
-- Description:		StoredProcedures/Functions DDL Change Detection
-- Repository:		https://github.com/DataBooster/DbWebApi
-- ================================================================

ALTER PROCEDURE dbo.DBWEBAPI_DETECT_DDL_CHANGES
	@inCommaDelimitedString	NVARCHAR(4000),
	@inElapsedMinutes		INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE	@SP_DDL_STATE_TAB TABLE
	(
		FULL_NAME		NVARCHAR(256),
		SP_NAME			NVARCHAR(128),
		SP_SCHEMA		NVARCHAR(64),
		DATABASE_NAME	NVARCHAR(64),
		SERVER_NAME		NVARCHAR(64),
		SYS_VIEW		NVARCHAR(128),
		MODIFY_DATE		DATETIME
	);
	DECLARE	@tLen			INT;
	DECLARE	@tStartPos		INT;
	DECLARE	@tEndPos		INT;
	DECLARE	@tFullName		NVARCHAR(256);
	DECLARE	@tSpName		NVARCHAR(128);
	DECLARE	@tSchemaName	NVARCHAR(64);
	DECLARE	@tDbName		NVARCHAR(64);
	DECLARE	@tServerName	NVARCHAR(64);
	DECLARE	@tSysView		NVARCHAR(128);

	SET	@tLen		= LEN(@inCommaDelimitedString);
	SET	@tStartPos	= 1;

	WHILE @tStartPos < @tLen
	BEGIN
		SET	@tEndPos	= CHARINDEX(',', @inCommaDelimitedString, @tStartPos);

		IF	@tEndPos = 0
			SET	@tEndPos	= @tLen + 1;

		SET	@tFullName	= LTRIM(RTRIM(SUBSTRING(@inCommaDelimitedString, @tStartPos, @tEndPos - @tStartPos)));

		IF LEN(@tFullName) > 0
		BEGIN
			SET	@tSpName		= PARSENAME(@tFullName, 1);
			SET	@tSchemaName	= ISNULL(PARSENAME(@tFullName, 2), 'dbo');
			SET	@tDbName		= PARSENAME(@tFullName, 3);
			SET	@tServerName	= PARSENAME(@tFullName, 4);

			IF @tSpName IS NOT NULL AND (@tServerName IS NULL OR @tDbName IS NOT NULL)
			BEGIN
				SET	@tSysView	= '';

				IF @tServerName IS NOT NULL
					SET	@tSysView	= QUOTENAME(@tServerName) + '.' + QUOTENAME(@tDbName) + '.';
				ELSE IF @tDbName IS NOT NULL
					SET	@tSysView	= QUOTENAME(@tDbName) + '.';

				SET	@tSysView = @tSysView + 'INFORMATION_SCHEMA.ROUTINES';

				INSERT INTO @SP_DDL_STATE_TAB (FULL_NAME, SP_NAME, SP_SCHEMA, DATABASE_NAME, SERVER_NAME, SYS_VIEW)
				VALUES(@tFullName, @tSpName, @tSchemaName, @tDbName, @tServerName, @tSysView);
			END;
		END;

		SET @tStartPos	= @tEndPos + 1;
	END;

	DECLARE	@tSql				NVARCHAR(1024);
	DECLARE	@tLastAltered		DATETIME;
	DECLARE	@tParmDefinition	NVARCHAR(512);
	DECLARE	@tExpiration		DATETIME;

	SET	@tParmDefinition	= N'@tSchemaName NVARCHAR(64), @tSpName NVARCHAR(128), @tLastAltered DATETIME OUTPUT';
	SET	@tExpiration		= DATEADD(minute, -@inElapsedMinutes, GETDATE());
	SET	@tExpiration		= DATEADD(second, -6, @tExpiration);			-- Plus approximate connection time

	DECLARE	tCursor CURSOR FOR
		SELECT SYS_VIEW, SP_SCHEMA, SP_NAME FROM @SP_DDL_STATE_TAB
		FOR UPDATE OF MODIFY_DATE;

	OPEN tCursor;
	FETCH NEXT FROM tCursor INTO @tSysView, @tSchemaName, @tSpName
	WHILE @@fetch_status = 0
	BEGIN
		SET	@tLastAltered = NULL;
		SET	@tSql = N'SELECT @tLastAltered = LAST_ALTERED FROM ' + @tSysView + ' WHERE ROUTINE_SCHEMA = @tSchemaName AND ROUTINE_NAME = @tSpName';

		BEGIN TRY
			EXECUTE sp_executesql @tSql, @tParmDefinition, @tSchemaName = @tSchemaName, @tSpName = @tSpName, @tLastAltered = @tLastAltered OUTPUT;
			IF @tLastAltered IS NOT NULL
				UPDATE	@SP_DDL_STATE_TAB
				SET		MODIFY_DATE	= @tLastAltered
				WHERE	CURRENT OF tCursor;
		END TRY
		BEGIN CATCH
		END CATCH;

		FETCH NEXT FROM tCursor INTO @tSysView, @tSchemaName, @tSpName;
	END;

	CLOSE tCursor;
	DEALLOCATE tCursor;

	SELECT FULL_NAME FROM @SP_DDL_STATE_TAB WHERE MODIFY_DATE > @tExpiration OR MODIFY_DATE IS NULL;
END;
GO

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Please globally replace following "TargetSchema" @
@ with your actual target schema for installation, @
@ and then remove this block of comment (5 lines). @
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CREATE OR REPLACE PACKAGE "TargetSchema".DBWEBAPI IS

-- ================================================================
-- Original Author:	Abel Cheng <abelcys@gmail.com>
-- Create date:		2015-04-15
-- Description:		StoredProcedures/Functions DDL Change Detection
-- Repository:		https://github.com/DataBooster/DbWebApi
-- ================================================================

TYPE SP_DDL_STATE	IS RECORD
(
	FULL_NAME		VARCHAR2(256),
	OWNER_SCHEMA	VARCHAR2(128),
	OBJECT_NAME		VARCHAR2(128)
);

TYPE SP_DDL_STATE_TAB	IS TABLE OF SP_DDL_STATE;


FUNCTION SPLIT_STRING_TO_SP
(
	inCommaDelimitedString	CLOB
)
RETURN SP_DDL_STATE_TAB PIPELINED;


-- Main --------------------
PROCEDURE DETECT_DDL_CHANGES
(
	inCommaDelimitedString	CLOB,
	inElapsedMinutes		PLS_INTEGER,
	RC1						OUT SYS_REFCURSOR
);


END DBWEBAPI;
/
CREATE OR REPLACE PACKAGE BODY "TargetSchema".DBWEBAPI IS

-- Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
-- Licensed under the MIT license. See LICENSE file in the project root for full license information.
-- Repository:	https://github.com/DataBooster/DbWebApi

FUNCTION NORMALIZE_NAME
(
	inName			VARCHAR2
)	RETURN			VARCHAR2
AS
BEGIN
	IF inName LIKE '"%"' THEN
		RETURN SUBSTR(inName, 2, LENGTH(inName) - 2);
	ELSE
		RETURN UPPER(inName);
	END IF;
END NORMALIZE_NAME;


PROCEDURE EXTRACT_SP
(
	inFullName		VARCHAR2,
	outOwner		OUT VARCHAR2,
	outObjectName	OUT VARCHAR2
)	AS
	tDot1			PLS_INTEGER;
	tDot2			PLS_INTEGER;
BEGIN
	tDot1	:= INSTR(inFullName, '.');

	IF tDot1 > 1 THEN
		outOwner	:= NORMALIZE_NAME(SUBSTR(inFullName, 1, tDot1 - 1));
		tDot2		:= INSTR(inFullName, '.', tDot1 + 1);

		IF tDot2 > tDot1 THEN
			outObjectName	:= NORMALIZE_NAME(SUBSTR(inFullName, tDot1 + 1, tDot2 - tDot1 - 1));
		ELSE
			outObjectName	:= NORMALIZE_NAME(SUBSTR(inFullName, tDot1 + 1));
		END IF;
	ELSE
		outOwner		:= NULL;
		outObjectName	:= NULL;
	END IF;
END EXTRACT_SP;


FUNCTION SPLIT_STRING_TO_SP
(
	inCommaDelimitedString	CLOB
)
RETURN SP_DDL_STATE_TAB PIPELINED
AS
	tStartPos	PLS_INTEGER		:= 1;
	tEndPos		PLS_INTEGER;
	tSp			SP_DDL_STATE;
BEGIN
	LOOP
		tEndPos	:= INSTR(inCommaDelimitedString, ',', tStartPos);

		IF tEndPos >= tStartPos THEN
			tSp.FULL_NAME	:= TRIM(SUBSTR(inCommaDelimitedString, tStartPos, tEndPos - tStartPos));
			tStartPos	:= tEndPos + 1;

			IF (LENGTH(tSp.FULL_NAME) > 0) THEN
				EXTRACT_SP(tSp.FULL_NAME, tSp.OWNER_SCHEMA, tSp.OBJECT_NAME);

				IF tSp.OWNER_SCHEMA IS NOT NULL AND tSp.OBJECT_NAME IS NOT NULL THEN
					PIPE ROW(tSp);
				END IF;
			END IF;
		ELSE
			tSp.FULL_NAME	:= TRIM(SUBSTR(inCommaDelimitedString, tStartPos));

			IF (LENGTH(tSp.FULL_NAME) > 0) THEN
				EXTRACT_SP(tSp.FULL_NAME, tSp.OWNER_SCHEMA, tSp.OBJECT_NAME);

				IF tSp.OWNER_SCHEMA IS NOT NULL AND tSp.OBJECT_NAME IS NOT NULL THEN
					PIPE ROW(tSp);
				END IF;
			END IF;
			EXIT;
		END IF;
	END LOOP;

	RETURN;
END SPLIT_STRING_TO_SP;


PROCEDURE DETECT_DDL_CHANGES
(
	inCommaDelimitedString	CLOB,
	inElapsedMinutes		PLS_INTEGER,
	RC1						OUT SYS_REFCURSOR
)	AS
	tExpiration	DATE	:= SYSDATE - (inElapsedMinutes + 0.1) / 1440.0;		-- Plus approximate connection time
BEGIN
	OPEN RC1 FOR
	SELECT
		A.FULL_NAME
	FROM
		TABLE(SPLIT_STRING_TO_SP(inCommaDelimitedString))	A
		JOIN ALL_OBJECTS									C
		ON
		(
				C.OBJECT_TYPE	IN ('PACKAGE', 'PROCEDURE', 'FUNCTION')
			AND	C.OBJECT_NAME	= A.OBJECT_NAME
			AND	C.OWNER			= A.OWNER_SCHEMA
		)
	WHERE
		C.LAST_DDL_TIME	>= tExpiration;

END DETECT_DDL_CHANGES;


END DBWEBAPI;
/

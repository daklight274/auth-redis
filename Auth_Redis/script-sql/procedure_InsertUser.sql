CREATE PROCEDURE spUser_Insert(
	@Id AS VARCHAR(50),
	@UserName AS VARCHAR(50),
	@Email AS VARCHAR(100),
	@PassWord AS VARCHAR(MAX),
	@IsEmailConfirmed AS BIT
)
AS
BEGIN
	INSERT INTO [dbo].[Users](
		[Id],
		[UserName],
		[Email],
		[PassWord],
		[IsEmailConfirmed]
	)
	VALUES(
		@Id,
		@UserName,
		@Email,
		@PassWord,
		@IsEmailConfirmed
	)
END
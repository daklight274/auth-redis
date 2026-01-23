CREATE PROCEDURE spUser_ChangeEmailConfirmed(
	@UserName AS VARCHAR(50),
	@IsEmailConfirmed AS BIT
)
AS
BEGIN
	UPDATE [dbo].[Users]
	SET IsEmailConfirmed = @IsEmailConfirmed
	WHERE UserName = @UserName AND IsEmailConfirmed=0
END
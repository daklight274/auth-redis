CREATE PROCEDURE spUser_GetByUsername(
	@UserName AS VARCHAR(50)
)
AS
BEGIN
	SELECT Id,UserName,PassWord,Email,IsEmailConfirmed
	FROM Users WITH (NOLOCK)
	WHERE UserName=@UserName
END
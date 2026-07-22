IF COL_LENGTH('Users', 'ResetPasswordToken') IS NULL
BEGIN
    ALTER TABLE Users ADD ResetPasswordToken NVARCHAR(200) NULL;
END

IF COL_LENGTH('Users', 'ResetPasswordTokenExpiry') IS NULL
BEGIN
    ALTER TABLE Users ADD ResetPasswordTokenExpiry DATETIME NULL;
END
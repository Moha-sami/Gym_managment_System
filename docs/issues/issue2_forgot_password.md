# Issue: Implement OTP-Based Forgot Password Feature

## What happened

Users who forgot their password had no self-service option to reset their account access, requiring manual database/admin intervention.

## What I expected

A user-facing workflow to securely reset password:
1. "Forgot Password" link on Login page.
2. Email verification screen to trigger OTP.
3. 6-digit numeric OTP validation screen.
4. Secure password resetting screen checking complexity rules.

## Resolution Implemented

1. **Security Token Provider:** Created custom `EmailOtpTokenProvider` registered in `Program.cs` to generate time-limited 6-digit numeric OTPs.
2. **Dedicated Actions:** Added actions `ForgotPassword`, `VerifyOtp`, and `ResetPassword` inside `AccountController.cs`.
3. **Responsive Views:** Added custom, floating-label CSS form layouts matching the branding of the login screens.
4. **Console/Debug Mock:** Enabled logging of the OTP to the debug/stdout console output for local development.


using System.ComponentModel;

namespace MasterApi.Core.Account.Enums
{
    public enum UserAccountMessages
    {
        [Description("You'll soon receive a SMS with instructions to verify your phone number and activate your account")]
        SuccessOnCreatingSms,

        [Description("You'll soon receive an email with instructions to verify and activate your account")]
        SuccessOnCreatingEmail,

        [Description("Your account was updated successfully")]
        SuccessOnUpdating,

        [Description("The username or password you entered is incorrect")]
        InvalidCredentials,

        [Description("Email is invalid")]
        InvalidEmail,

        [Description("Phone is invalid")]
        InvalidPhone,

        [Description("Missing Tenant")]
        MissingTenant,

        [Description("Password is required")]
        MissingPassword,

        [Description("Username is required")]
        MissingUsername,

        [Description("Your account hasn't been verified yet")]
        AccountNotVerified,

        [Description("Your account is closed")]
        AccountClosed,

        [Description("Your login is not allowed")]
        LoginNotAllowed,

        [Description("Email already in use")]
        EmailAlreadyInUse,

        [Description("Phone number already in use")]
        PhoneAlreadyInUse,

        [Description("User not found")]
        UserNotFound,

        [Description("Mobile device token is missing")]
        MissingDeviceToken,

        [Description("Mobile device installation Id is missing")]
        MissingInstallationId,

        [Description("Only one device is allowed per user account .")]
        OnlyOneAllowedDevicePerAccount,

        [Description("Failed login attempts exceeded")]
        FailedLoginAttemptsExceeded,

        [Description("")]
        None
    }
}

using System;

namespace svc_usr.Data {
    public static class Messages {
        public static String ERROR_INVALID_USERNAME_PASSWORD => "The username/password couple is invalid.";
        public static String ERROR_ACCOUNT_LOGIN_REVOKED => "The user is no longer allowed to sign in.";
        public static String ERROR_UNSUPPORTED_GRANT_TYPE => "The specified grant type is not supported";
    }
}
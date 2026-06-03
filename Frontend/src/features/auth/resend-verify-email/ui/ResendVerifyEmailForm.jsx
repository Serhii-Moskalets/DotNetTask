import React from "react";
import { Link } from "react-router";
import Button from "../../../../shared/components/Button";
import FormAlert from "../../../../shared/components/FormAlert";

const ResendVerifyEmailForm = ({
    email,
    loading,
    success,
    error,
    handleResend,
    handleLogout,
}) => {
    const showFormError = error && !error.isRateLimit;
    const showSuccess = success && !error?.isRateLimit;
    
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">

            <div className="w-16 h-16 bg-indigo-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <svg className="w-8 h-8 text-indigo-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                        d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">Verify your email</h1>
            <p className="text-slate-500 text-sm mb-2">
                We sent a confirmation link to
            </p>
            <p className="text-indigo-600 font-semibold text-sm mb-6 truncate px-2">
                {email || "your inbox"}
            </p>
            <p className="text-slate-400 text-xs mb-8 leading-relaxed">
                Click the link in the email to activate your account. Check your spam folder if you don't see it.
            </p>

            {showSuccess && (
                <div className="bg-green-50 border border-green-200 text-green-600 text-sm rounded-xl px-4 py-3 mb-4 text-left">
                    Email resent successfully! Check your inbox.
                </div>
            )}

            {showFormError && <FormAlert error={error} />}

            <Button 
                onClick={handleResend}
                loading={loading} 
                className="mb-3"
                disabled={error?.isRateLimit}
            >
                Resend verification email
            </Button>

            <Link
                type="button"
                onClick={handleLogout}
                className="w-full py-3 text-sm text-slate-500 hover:text-slate-700 transition-colors"
            >
                Back to login
            </Link>
        </div>
    );
};

export default ResendVerifyEmailForm;
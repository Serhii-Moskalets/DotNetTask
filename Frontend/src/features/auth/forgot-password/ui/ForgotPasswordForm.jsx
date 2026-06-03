import React from "react";
import { Link } from "react-router";
import Input from "../../../../shared/components/Input";
import Button from "../../../../shared/components/Button";
import { ROUTES } from "../../../../shared/constants/routes";
import FormAlert from "../../../../shared/components/FormAlert";

const ForgotPasswordForm = ({
    email,
    setEmail,
    loading,
    error,
    handleSubmit,
}) => {

    const showFormError = error && !error.isRateLimit;
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8">
            <div className="mb-8 text-center">
                <h1 className="text-2xl font-bold text-slate-800">Forgot Password</h1>
            </div>
            
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <Input 
                    label="Your Email" 
                    type="email"
                    placeholder="you@example.com" 
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />

                {showFormError && <FormAlert error={error} />}

                <Button type="submit" loading={loading} className="mt-2">
                    Restore
                </Button>
            </form>

            <p className="text-center text-sm text-slate-500 mt-6">
                Remember your password?{" "}
                <Link to={ROUTES.LOGIN} className="text-indigo-600 font-semibold hover:underline">
                    Sign in
                </Link>
            </p>
        </div>
    );
};

export default ForgotPasswordForm;
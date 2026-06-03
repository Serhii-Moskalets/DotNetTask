import React from "react";
import { Link } from "react-router";
import Input from "../../../../shared/components/Input";
import Button from "../../../../shared/components/Button";
import { ROUTES } from "../../../../shared/constants/routes";
import FormAlert from "../../../../shared/components/FormAlert";

const LoginForm = ({
    email, setEmail,
    password, setPassword,
    error, loading,
    handleSubmit
}) => {
    const showFormError = error && !error.isRateLimit;
    
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8">

            <div className="mb-8 text-center">
                <h1 className="text-2xl font-bold text-slate-800">Welcome back</h1>
                <p className="text-slate-500 text-sm mt-1">Sign in to your account</p>
            </div>

            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <Input
                    label="Email"
                    type="email"
                    placeholder="you@example.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
                
                <Input
                    label="Password"
                    type="password"
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />

                {showFormError && <FormAlert error={error} />}

                <Button 
                    type="submit" 
                    loading={loading} 
                    className="mt-2" 
                    disabled={error?.isRateLimit}
                >
                    Sign in
                </Button>
            </form>
            
            <p className="text-center text-sm text-slate-500 mt-6">
                Forgot your password?{" "}
                <Link to={ROUTES.FORGOT_PASSWORD} className="text-indigo-600 font-semibold hover:underline">
                    Reset password
                </Link>
            </p>

            <p className="text-center text-sm text-slate-500 mt-6">
                Don't have an account?{" "}
                <Link to={ROUTES.REGISTER} className="text-indigo-600 font-semibold hover:underline">
                    Register
                </Link>
            </p>
        </div>
    );
};

export default LoginForm;
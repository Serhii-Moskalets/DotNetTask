import Input from "../../../../shared/components/Input";
import Button from "../../../../shared/components/Button";
import { Link } from "react-router";
import { ROUTES } from "../../../../shared/constants/routes";
import FormAlert from "../../../../shared/components/FormAlert";

const ResetPasswordForm = ({
    password,
    setPassword,
    loading,
    error,
    isInvalidToken,
    handleSubmit,
}) => {
    const showFormError = error && !error.isRateLimit;
    
    if (isInvalidToken) {
        return (
            <div className="w-full max-w-md bg-white rounded-2xl shadow-xl p-8 text-center">
                <h1 className="text-xl font-bold text-red-500">
                    Invalid or expired link
                </h1>
                <p className="text-slate-500 mt-2">
                    This reset link is not valid.
                </p>
                <Link to={ROUTES.FORGOT_PASSWORD}>
                    <Button className="mt-6">
                        Request new link
                    </Button>
                </Link>
            </div>
        );
    }
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8">
            <div className="mb-8 text-center">
                <h1 className="text-2xl font-bold text-slate-800">
                    Set new password
                </h1>
            </div>
            
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <Input 
                    type="password"
                    label="New password" 
                    placeholder="••••••••" 
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />

                {showFormError && <FormAlert error={error} />}

                <Button type="submit" loading={loading} className="mt-2">
                    Update password
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

export default ResetPasswordForm;
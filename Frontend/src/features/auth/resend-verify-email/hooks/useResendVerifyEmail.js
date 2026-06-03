import { useState } from "react";
import { useAuth } from "../../../../app/providers/AuthProvider";
import { useNavigate } from "react-router";
import authService from "../../api/authService";
import { ROUTES } from "../../../../shared/constants/routes";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useResendVerifyEmail = () => {
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    
    const [error, setError] = useThrottledError(COOLDOWN_TIMES.AUTH_ACTION)

    const { user, logout } = useAuth();
    const navigate = useNavigate();


    const handleResend = async () => {
        setError(null);
        setSuccess(false);
        setLoading(true);
        try {
            await authService.resendEmailVerification();
            setSuccess(true);
        } catch (err) {
            setError(
                err.message || "Failed to resend email.",
                err.type === ERROR_TYPE.RATE_LIMIT,
        );
        } finally {
            setLoading(false);
        }
    };

    const handleLogout = () => {
        logout();
        navigate(ROUTES.LOGIN);
    };

    return {
        email: user?.email,
        loading,
        success,
        error,
        handleResend,
        handleLogout,
    };
};

export default useResendVerifyEmail;
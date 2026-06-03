import { useState, useEffect } from "react"
import { useAuth } from "../../../../app/providers/AuthProvider";
import { useNavigate } from "react-router";
import authService from "../../api/authService";
import { ROUTES } from "../../../../shared/constants/routes";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useLogin = () => {
    const [email, setEmailState] = useState('');
    const [password, setPasswordState] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError, clearNormalError] = useThrottledError(COOLDOWN_TIMES.AUTH_ACTION);
    
    const { login } = useAuth();
    const navigate = useNavigate();

    
    const setEmail = (value) => {
        setEmailState(value);
        clearNormalError();
    };

    const setPassword = (value) => {
        setPasswordState(value);
        clearNormalError();
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        setLoading(true);

        try {
            const data = await authService.login(email, password);
            login({
                userName: data.userName, 
                email: data.email,
                isEmailConfirmed: data.isEmailConfirmed,
                isAccountPendingDeletion: data.isAccountPendingDeletion,
                mustChangePassword: data.mustChangePassword
            }, data.token);

            if (!data.isEmailConfirmed) {
                navigate(ROUTES.RESEND_VERIFY_EMAIL);
            } else {
                navigate(ROUTES.DASHBOARD);
            }
        } catch (err) {
            setError(
                err.message || "Something went wrong",
                err.type === ERROR_TYPE.RATE_LIMIT,
            );
            setPassword('');
        } finally {
            setLoading(false);
        }
    };

    return {
        email,
        setEmail,
        password,
        setPassword,
        error,
        loading,
        handleSubmit,
    };
};

export default useLogin;
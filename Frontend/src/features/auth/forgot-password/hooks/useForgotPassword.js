import { useState } from "react"
import authService from "../../api/authService";
import { ROUTES } from "../../../../shared/constants/routes";
import { useNavigate } from "react-router";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useForgotPassword = () => {
    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const [error, setError, clearNormalError] = useThrottledError(COOLDOWN_TIMES.FOGOT_PASSWORD);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        setLoading(true);

        try {
            await authService.forgotPassword(email);
            navigate(ROUTES.FORGOT_PASSWORD_SUCCESS);
        } catch (err) {
            setError(
                err.message || "Something went wrong. Please try again.",
                err.type === ERROR_TYPE.RATE_LIMIT,
            );
        } finally {
            setLoading(false);
        }
    };

    return {
        email,
        setEmail,
        loading,
        error,
        handleSubmit
    };
};

export default useForgotPassword;
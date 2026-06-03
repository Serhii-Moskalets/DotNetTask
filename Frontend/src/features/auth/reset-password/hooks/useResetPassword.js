import { use, useMemo, useState } from "react";
import { useNavigate, useSearchParams } from "react-router"
import authService from "../../api/authService";
import { ROUTES } from "../../../../shared/constants/routes";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useResetPassword = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    const token = useMemo(
        () => searchParams.get("token"),
        [searchParams]
    );

    const [password, setPasswordState] = useState('');
    const [loading, setLoading] = useState(false);

    const [error, setError, clearNormalError] = useThrottledError(COOLDOWN_TIMES.AUTH_ACTION)

    const isInvalidToken = !token;

    const setPassword = (value) => {
        setPasswordState(value);
        clearNormalError();
    }

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        try {
            await authService.resetPassword(password, token);
            navigate(ROUTES.LOGIN, {
                state: { message: "Password updated successfully"}
            });
        } catch (err) {
            setError(
                err.message || "Failed to reset password.",
                err.type === ERROR_TYPE.RATE_LIMIT,
            );
        } finally {
            setLoading(false);
        }
    };

    return {
        password,
        setPassword,
        loading,
        error,
        isInvalidToken,
        handleSubmit,
    };

};

export default useResetPassword;
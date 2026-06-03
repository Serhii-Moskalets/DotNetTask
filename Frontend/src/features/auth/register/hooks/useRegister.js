import { useState, useEffect } from "react"
import { useNavigate } from "react-router";
import authService from "../../api/authService";
import { ROUTES } from "../../../../shared/constants/routes";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useRegister = () => {
    const [form, setForm] = useState({
        firstName: '',
        lastName: '',
        email: '',
        userName: '',
        password: '',
    });
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const [error, setError, clearNormalError] = useThrottledError(COOLDOWN_TIMES.REGISTRATION);

    const handleChange = (field) => (e) => {
        setForm(prev => ({ ...prev, [field]: e.target.value }));
        clearNormalError();
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);
        setLoading(true);

        try {
            await authService.register(form);
            navigate(ROUTES.VERIFY_EMAIL);
        } catch (err) {
            setError(
                err.message || "Registration failed. Please try again.",
                err.type === ERROR_TYPE.RATE_LIMIT
            );
        } finally {
            setLoading (false);
        }
    };

    return {
        form,
        handleChange,
        handleSubmit,
        error,
        loading,
    };
};

export default useRegister;
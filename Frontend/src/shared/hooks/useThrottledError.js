import { useEffect, useState } from "react"
import { COOLDOWN_TIMES } from "../constants/cooldowns";

export const useThrottledError = (defaultCooldownMs = COOLDOWN_TIMES.DEFAULT) => {
    const [error, setErrorState] = useState(null);

    useEffect(() => {
        if (error?.isRateLimit) {
            const cooldown = error.retryAfterMs || defaultCooldownMs;

            const timer = setTimeout(() => {
                setErrorState(null);
            }, cooldown);

            return () => clearTimeout(timer);
        }
    }, [error, defaultCooldownMs]);

    const setError = (err, isRateLimit = false, retryAfterMs = null) => {
        if (!err) {
             setErrorState(null);
             return;
        }
        setErrorState({
            text: err.message || err,
            isRateLimit,
            retryAfterMs
        });
    };

    const clearNormalError = () => {
        if (error && !error.isRateLimit) {
            setErrorState(null);
        }
    };

    return [error, setError, clearNormalError];
};
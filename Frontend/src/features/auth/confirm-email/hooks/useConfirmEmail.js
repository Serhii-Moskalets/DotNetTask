import { useEffect, useRef, useState } from "react"
import { useNavigate, useSearchParams } from "react-router";
import { STATUS } from "../constants/status";
import authService from "../../api/authService";
import { ERROR_TYPE } from "../../../../shared/constants/errorType";
import { useThrottledError } from "../../../../shared/hooks/useThrottledError";
import { COOLDOWN_TIMES } from "../../../../shared/constants/cooldowns";

const useConfirmEmail = () => {
    const [status, setStatus] = useState(STATUS.LOADING);
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [searchParams] = useSearchParams();

    const [resendError, setResendError] = useThrottledError(COOLDOWN_TIMES.AUTH_ACTION);

    const hasCalled = useRef(false);

    useEffect(() => {
        const token = searchParams.get("token");

        if(!token) {
            setStatus(STATUS.INVALID);
            return;
        }

        if (hasCalled.current) return;
        hasCalled.current = true;

        authService.confirmEmail(token)
            .then(() => {
                setStatus(STATUS.SUCCESS)
            })
            .catch((err) => {
                if (err.type === ERROR_TYPE.EXPIRED) {
                    setStatus(STATUS.EXPIRED);
                    return;
                }

                if (err.type === ERROR_TYPE.RATE_LIMIT) {
                    setStatus(STATUS.TOO_MANY_ATTEMPTS);
                    return;
                }

                setStatus(STATUS.INVALID);
            });
    }, [searchParams]);

    const resend = async () => {
        setResendError(null);
        setSuccess(false);
        setLoading(true);

        try {
            await authService.resendEmailVerification();
            setSuccess(true);
        } catch (err) {
            setResendError(
                err.message || "Failed to resend email",
                err.type === ERROR_TYPE.RATE_LIMIT,
            );
        } finally {
            setLoading(false);
        }
    };

    return {
        status,
        resend,
        loading,
        success,
        resendError,
    };
};

export default useConfirmEmail;
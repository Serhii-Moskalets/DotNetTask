import { useNavigate } from "react-router";

import useConfirmEmail from "../hooks/useConfirmEmail"

import Loading from "../ui/Loading";
import Success from "../ui/Success";
import Expired from "../ui/Expired";
import Invalid from "../ui/Invalid";

import { STATUS } from "../constants/status";
import { ROUTES } from "../../../../shared/constants/routes";
import TooManyAttempts from "../ui/TooManyAttempts";

const  ConfirmEmailPage = ({ status, resend, loading, success, resendError }) => {
    const navigate = useNavigate();
    const handleBackToLogin = () => navigate(ROUTES.LOGIN);

    switch (status) {
        case STATUS.LOADING:
            return <Loading />
        
        case STATUS.SUCCESS:
            return <Success onDone={handleBackToLogin} />;
            
        case STATUS.EXPIRED:
            return <Expired 
            onResend={resend}
            loading={loading}
            success={success}
            error={resendError}
            onDone={handleBackToLogin}
        />;

        case STATUS.TOO_MANY_ATTEMPTS:
            return <TooManyAttempts onDone={handleBackToLogin} />;

        case STATUS.INVALID:
        default:
            return <Invalid onDone={handleBackToLogin} />;
    }
};

export default ConfirmEmailPage;
import { Navigate } from "react-router";
import { useAuth } from "../../providers/AuthProvider";
import { ROUTES } from "../../../shared/constants/routes";

const AuthGuard = ({ children }) => {
    const { user, loading } = useAuth();

    if (loading) return null;

    if (!user) {
        return <Navigate to={ROUTES.LOGIN} replace />;
    }

    if (!user.isEmailConfirmed) {
        return <Navigate to={ROUTES.RESEND_VERIFY_EMAIL} replace />;
    }

    return children;
};

export default AuthGuard;
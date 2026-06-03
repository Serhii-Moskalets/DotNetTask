import { Navigate } from "react-router";
import { useAuth } from "../../providers/AuthProvider";
import { ROUTES } from "../../../shared/constants/routes";

const UnverifiedGuard = ({ children }) => {
    const { user, loading } = useAuth();

    if (loading) return null;

    if (!user) {
        return <Navigate to={ROUTES.LOGIN} replace />;
    }

    if (user.isEmailConfirmed) {
        return <Navigate to={ROUTES.DASHBOARD} replace />;
    }

    return children;
};

export default UnverifiedGuard;
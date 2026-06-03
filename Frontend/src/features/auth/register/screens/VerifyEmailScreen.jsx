import { useNavigate } from "react-router"
import Layout from "../../../../shared/components/Layout"
import { ROUTES } from "../../../../shared/constants/routes";
import VerifyEmailForm from "../ui/VerifyEmailForm";

const VerifyEmailScreen = () => {
    const navigate = useNavigate();

    const handleBackToLogin = () => {
        navigate(ROUTES.LOGIN);
    };
    
    return (
        <Layout>
            <VerifyEmailForm onBackToLogin={handleBackToLogin} />
        </Layout>
    );
};

export default VerifyEmailScreen;
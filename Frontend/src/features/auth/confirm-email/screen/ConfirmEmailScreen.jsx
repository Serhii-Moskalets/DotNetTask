import Layout from "../../../../shared/components/Layout";
import useConfirmEmail from "../hooks/useConfirmEmail";
import ConfirmEmailPage from "../page/ConfirmEmailPage";
import TopAlertContainer from "../../../../shared/components/TopAlertContainer";
import { useNavigate } from "react-router";
import { ROUTES } from "../../../../shared/constants/routes";

const ConfirmEmailScreen = () => {
    const props = useConfirmEmail();
    
    return (
        <Layout>
            <TopAlertContainer error={props.resendError} />
            <ConfirmEmailPage />
        </Layout>
    );
};

export default ConfirmEmailScreen;
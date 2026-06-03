import Layout from "../../../../shared/components/Layout";
import TopAlertContaimer from "../../../../shared/components/TopAlertContainer";
import useForgotPassword from "../hooks/useForgotPassword";
import ForgotPasswordForm from "../ui/ForgotPasswordForm";

const ForgotPasswordScreen = () => {
    const props = useForgotPassword();

    return (
        <Layout>
            <TopAlertContaimer error={props.error} />
            <ForgotPasswordForm {...props} />
        </Layout>
    )
};

export default ForgotPasswordScreen;
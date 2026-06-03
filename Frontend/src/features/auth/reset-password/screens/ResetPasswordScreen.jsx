import Layout from "../../../../shared/components/Layout";
import RateLimitAlert from "../../../../shared/components/RateLimitAlert";
import TopAlertContaimer from "../../../../shared/components/TopAlertContainer";
import useResetPassword from "../hooks/useResetPassword";
import ResetPasswordForm from "../ui/ResetPasswordForm";

const ResetPasswordScreen = () => {
    const props = useResetPassword();

    return (
        <Layout>
            <TopAlertContaimer  error={props.error} />
            <ResetPasswordForm {...props} />
        </Layout>
    );
};

export default ResetPasswordScreen;
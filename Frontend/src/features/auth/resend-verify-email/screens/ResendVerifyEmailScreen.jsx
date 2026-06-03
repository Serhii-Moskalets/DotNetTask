import Layout from "../../../../shared/components/Layout";
import RateLimitAlert from "../../../../shared/components/RateLimitAlert";
import TopAlertContaimer from "../../../../shared/components/TopAlertContainer";
import useResendVerifyEmail from "../hooks/useResendVerifyEmail";
import ResendVerifyEmailForm from "../ui/ResendVerifyEmailForm";

const ResendVerifyEmailScreen = () => {
    const props = useResendVerifyEmail();

    return (
        <Layout>
            <TopAlertContaimer  error={props.error} />
            <ResendVerifyEmailForm {...props} />
        </Layout>
    );
};

export default ResendVerifyEmailScreen;
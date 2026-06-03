import Layout from "../../../../shared/components/Layout";
import RateLimitAlert from "../../../../shared/components/RateLimitAlert";
import TopAlertContaimer from "../../../../shared/components/TopAlertContainer";
import useLogin from "../hooks/useLogin";
import LoginForm from "../ui/LoginForm";

const LoginScreen = () => {
    const props = useLogin();
    
    return (
        <Layout>
            <TopAlertContaimer error={props.error} />
            <LoginForm {...props} />
        </Layout>    
    );
};

export default LoginScreen;
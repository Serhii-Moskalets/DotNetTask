import Layout from "../../../../shared/components/Layout";
import RateLimitAlert from "../../../../shared/components/RateLimitAlert";
import TopAlertContaimer from "../../../../shared/components/TopAlertContainer";
import useRegister from "../hooks/useRegister";
import RegisterForm from "../ui/RegisterForm";

const RegisterScreen = () => {
    const props = useRegister()
    
    return (
        <Layout>
            <TopAlertContaimer error={props.error} />
            <RegisterForm {...props} />
        </Layout>
    )
};

export default RegisterScreen;
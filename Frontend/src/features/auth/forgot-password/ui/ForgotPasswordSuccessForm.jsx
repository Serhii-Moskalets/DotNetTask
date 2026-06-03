import { Link } from "react-router";
import Button from "../../../../shared/components/Button";
import { ROUTES } from "../../../../shared/constants/routes";

const ForgotPasswordSuccessForm = () => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
            <div className="w-16 h-16 bg-indigo-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <svg className="w-8 h-8 text-indigo-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path 
                        strokeLinecap="round" 
                        strokeLinejoin="round" 
                        strokeWidth={2} 
                        d="M3 19v-8.93a2 2 0 01.89-1.664l8-4.8a2 2 0 012.22 0l8 4.8A2 2 0 0121 10.07V19a2 2 0 01-2 2H5a2 2 0 01-2-2z" 
                    />
                    <path 
                        strokeLinecap="round" 
                        strokeLinejoin="round" 
                        strokeWidth={2} 
                        d="M3 10l9 6 9-6" 
                    />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">Check your email</h1>
            
            <p className="text-slate-500 text-sm mb-8">
                We have sent a password reset link to your email address. 
                Please check your inbox and follow the instructions.
            </p>

            <Link to={ROUTES.LOGIN} className="block w-full">
                <Button>
                    Back to Sign In
                </Button>
            </Link>
        </div>
    );
};

export default ForgotPasswordSuccessForm;
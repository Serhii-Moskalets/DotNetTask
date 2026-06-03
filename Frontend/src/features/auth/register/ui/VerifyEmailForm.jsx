import Button from "../../../../shared/components/Button";

const VerifyEmailForm = ({ onBackToLogin }) => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
            <div className="w-16 h-16 bg-indigo-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <svg className="w-8 h-8 text-indigo-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                        d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">Registration Successful!</h1>
            
            <p className="text-slate-400 text-xs mb-8 leading-relaxed">
                Please check your inbox and click the link to activate your account. 
                If you don't see the email, check your spam folder.
            </p>

            <Button onClick={onBackToLogin} className="w-full">
                Back to sign in
            </Button>
        </div>
    );
};

export default VerifyEmailForm;
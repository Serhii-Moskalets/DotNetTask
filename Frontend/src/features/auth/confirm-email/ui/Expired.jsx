import { Link } from "react-router";
import Button from "../../../../shared/components/Button";
import { ROUTES } from "../../../../shared/constants/routes";


const Expired = ({ onResend, loading, success, onDone }) => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
            
            <div className="w-16 h-16 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-6">
                
                <svg className="w-8 h-8 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">Link expired</h1>
            
            <p className="text-slate-500 text-sm mb-8">
                This verification link is invalid or has expired.
            </p>

            {success  ? (
                <div className="bg-green-50 border border-green-200 text-green-600 text-sm rounded-xl px-4 py-3 mb-4">
                    New verification email sent! Check your inbox.
                </div>
            ) : (
                <Button 
                    onClick={onResend}
                    loading={loading}
                    className="mb-3"
                    disabled={error?.isRateLimit}
                >
                    {loading ? 'Sending...' : 'Resend email'}
                </Button>
            )}
            
            <Link
                to={ROUTES.LOGIN}
                className="block w-full py-3 text-sm text-slate-500 hover:text-slate-700 transition-colors"
            >
                Back to login
            </Link>
        </div>
    );
};

export default Expired;
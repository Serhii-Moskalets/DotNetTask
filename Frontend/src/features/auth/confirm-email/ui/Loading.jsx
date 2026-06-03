import { Link } from "react-router";
import { ROUTES } from "../../../../shared/constants/routes";

const Loading = () => (
    <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
        <div className="w-16 h-16 bg-indigo-50 rounded-full flex items-center justify-center mx-auto mb-6">
            <svg className="animate-spin w-8 h-8 text-indigo-500" viewBox="0 0 24 24" fill="none">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
            </svg>
        </div>
        <p className="text-slate-500 text-sm">Verifying your email...</p>
        <Link
            to={ROUTES.LOGIN}
            className="block w-full py-3 text-sm text-slate-500 hover:text-slate-700 transition-colors"
        >
            Back to login
        </Link>
    </div>
);

export default Loading;
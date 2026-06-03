import Button from "../../../../shared/components/Button";

const TooManyAttempts = ({ onDone }) => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
            
            <div className="w-16 h-16 bg-amber-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <svg
                    className="w-8 h-8 text-amber-500"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                >
                    <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">
                Too many attempts
            </h1>

            <p className="text-slate-500 text-sm mb-8">
                You have made too many requests in a short period. Please wait a few minutes before trying again.
            </p>

            <Button onClick={onDone}>
                Back to login
            </Button>

        </div>
    );
};

export default TooManyAttempts;
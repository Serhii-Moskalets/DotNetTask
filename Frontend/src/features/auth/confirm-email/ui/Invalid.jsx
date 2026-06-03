import Button from "../../../../shared/components/Button";

const Invalid  = ({ onDone  }) => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">

            <div className="w-16 h-16 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <svg
                    className="w-8 h-8 text-red-500"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                >
                    <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                    />
                </svg>
            </div>

            <h1 className="text-2xl font-bold text-slate-800 mb-2">
                Invalid token
            </h1>

            <p className="text-slate-500 text-sm mb-8">
                Verification token is missing or invalid.
            </p>

            <Button onClick={onDone}>
                Back to login
            </Button>

        </div>
    );
    
};

export default Invalid;
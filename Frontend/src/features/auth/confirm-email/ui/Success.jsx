import Button from "../../../../shared/components/Button";

const Success = ({ onDone  }) => {
    return (
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl shadow-slate-200 p-8 text-center">
        <div className="w-16 h-16 bg-green-50 rounded-full flex items-center justify-center mx-auto mb-6">
            <svg className="w-8 h-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
        </div>
        <h1 className="text-2xl font-bold text-slate-800 mb-2">Email confirmed!</h1>
        <p className="text-slate-500 text-sm mb-8">
            Your account is now active and ready to use.
        </p>
        <Button onClick={onDone}>
            Continue
        </Button>
    </div>
    );
};

export default Success;
const variants = {
    primary: "bg-gradient-to-r from-indigo-500 to-violet-500 hover:from-indigo-600 hover:to-violet-600 text-white shadow-lg shadow-indigo-200",
    outline: "border-2 border-indigo-500 text-indigo-600 hover:bg-indigo-50",
};

const Button = ({ children, variant = "primary", loading, className = "", ...props}) => (
    <button
        className={`w-full py-3 px-4 rounded-xl font-semibold text-sm transition-all duration-200 
        disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer ${variants[variant]} ${className}`}
        disabled={loading || props.disabled}
        {...props}
    >
        {loading ? (
            <span className="flex items-center justify-center gap-2">
               <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                </svg>
                {children}
            </span>
        ) : children}
    </button>
);

export default Button;
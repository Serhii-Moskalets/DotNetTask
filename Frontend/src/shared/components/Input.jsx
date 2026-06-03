const Input = ({ label, error, className = "", ...props }) => (
    <div className="flex flex-col gap-1.5">
        {label && (
            <label className="text-sm font-semibold text-slate-700">{label}</label>
        )}
        <input
            className={`w-full px-4 py-3 rounded-xl border text-sm transition-all duration-200
            bg-white text-slate-800 placeholder-slate-400
            border-slate-200 focus:outline-none focus:border-indigo-400 focus:ring-2 focus:ring-indigo-100
            ${error ? 'border-red-400 focus:border-red-400 focus:ring-red-100' : ''}
            ${className}`}
            {...props}
        />
        {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
);

export default Input;
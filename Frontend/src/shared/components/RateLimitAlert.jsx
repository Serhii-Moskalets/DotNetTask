import React from "react";

const RateLimitAlert = ({ error }) => {
    if (!error || !error.isRateLimit) return null;

    return (
        <div className="bg-amber-50 border border-amber-200 text-amber-700 text-sm rounded-xl px-4 py-3 flex gap-3 items-start text-left w-full shadow-md status-warning animate-fade-in">
            <svg className="w-5 h-5 text-amber-500 shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
                <span className="font-semibold block">Too many attempts</span>
                <span className="text-amber-600/90 text-xs">{error.text}</span>
            </div>
        </div>
    );
};

export default RateLimitAlert;
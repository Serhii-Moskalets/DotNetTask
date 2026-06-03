import RateLimitAlert from "./RateLimitAlert";


const TopAlertContaimer = ({ error }) => {
    if (!error?.isRateLimit) return null;

    return(
        <div className="fixed top-4 left-1/2 -translate-x-1/2 w-full max-w-md px-4 z-50 pointer-events-none">
            <div className="pointer-events-auto shadow-2xl">
                <RateLimitAlert error={error} />
            </div>
        </div>
    );
}

export default TopAlertContaimer;
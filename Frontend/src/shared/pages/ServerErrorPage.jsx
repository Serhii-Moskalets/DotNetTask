import { Link, useNavigate } from "react-router";
import useServerHealth from "../../shared/hooks/useServerHealth";
import { useEffect } from "react";

const ServerErrorPage = () => {
    const status = useServerHealth();
    const navigate = useNavigate();

    useEffect(() => {
        if (status === "online") {
            const t = setTimeout(() => {
                navigate("/", { replace: true });
            }, 1000);

            return () => clearTimeout(t);
        }
    }, [status]);

    return (
        <main className="grid min-h-screen place-items-center bg-white px-6 py-24">
            <div className="text-center">

                {status === "checking" && (
                    <>
                        <p className="text-indigo-600 font-semibold">
                            Reconnecting…
                        </p>
                        <h1 className="text-5xl font-bold mt-4">
                            Server is waking up
                        </h1>
                    </>
                )}

                {status === "offline" && (
                    <>
                        <p className="text-red-600 font-semibold">
                            Connection lost
                        </p>
                        <h1 className="text-5xl font-bold mt-4">
                            Server unavailable
                        </h1>
                        <p className="text-gray-500 mt-4">
                            We’ll reconnect automatically
                        </p>
                    </>
                )}

                {status === "online" && (
                    <>
                        <p className="text-green-600 font-semibold">
                            Back online
                        </p>
                        <h1 className="text-5xl font-bold mt-4">
                            You’re back
                        </h1>
                    </>
                )}

                <div className="mt-10 flex justify-center gap-4">
                    <Link
                        to="/"
                        className="bg-indigo-600 text-white px-4 py-2 rounded-md"
                    >
                        Go home
                    </Link>
                </div>
            </div>
        </main>
    );
};

export default ServerErrorPage;
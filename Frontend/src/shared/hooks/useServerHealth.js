import { useEffect, useState } from "react";
import { healthService } from "../api/healthService";

const useServerHealth = () => {
    const [status, setStatus] = useState("checking");

    useEffect(() => {
        let isMounted = true;
        let intervalId;

        const check = async () => {
            try {
                await healthService.check();

                if (!isMounted) return;

                setStatus("online");
                clearInterval(intervalId);
            } catch {
                if (!isMounted) return;
                setStatus("offline");
            }
        };

        check();
        intervalId = setInterval(check, 5000);
     
        return () => {
            isMounted = false;
            clearInterval(intervalId);
        };
    
    }, []);

    return status;
};

export default useServerHealth;
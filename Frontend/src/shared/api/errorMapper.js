import { ERROR_TYPE } from "../constants/errorType";

const mapApiError = (error) => {
    if (!error.response) {
         return {
            type: ERROR_TYPE.NETWORK,
            message: "Server is unreachable",
            status: null,
         };
    }

    const { status, data } = error.response;

    const message = 
        data?.detail ||
        data?.title || 
        "Server error";

    let type = "server";

    if (status === 400) {
        if (message === "Too many attempts. Please try later.") {
             type = ERROR_TYPE.RATE_LIMIT
        } else { 
            type = ERROR_TYPE.VALIDATION;
        }
    }
    if (status === 401) type = ERROR_TYPE.AUTH;
    if (status === 403) type = ERROR_TYPE.FORBIDDEN;
    if (status === 404) type = ERROR_TYPE.NOT_FOUND;
    if (status === 409) type = ERROR_TYPE.CONFLICT;
    if (status === 410) type = ERROR_TYPE.EXPIRED;
    if (status === 429) type = ERROR_TYPE.RATE_LIMIT;

    return {
        type,
        message,
        status,
        raw: data,
    };
};

export default mapApiError;
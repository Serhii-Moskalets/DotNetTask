import { ERROR_TYPE } from "../constants/errorType";
import { ROUTES } from "../constants/routes"
import mapApiError from "./errorMapper";

const interceptors = (api) => {
    api.interceptors.response.use(
        (response) => response,

        (error) => {
            const normalizedError = mapApiError(error);

            if (
                normalizedError.type === ERROR_TYPE.NETWORK ||
                normalizedError.status >= 502
            ) {
                window.location.href = ROUTES.SERVER_ERROR;

                return new Promise(() => {});
            }
            
            if (normalizedError.status === 401) {
                localStorage.removeItem("token");
                localStorage.removeItem("user");
            
                window.location.href = ROUTES.LOGIN;
            }

            return Promise.reject(normalizedError);
        }
    );
};

export default interceptors;
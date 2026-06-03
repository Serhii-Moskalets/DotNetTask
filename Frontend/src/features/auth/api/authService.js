import api from "../../../shared/api/apiClient";

const authService = {
    login: (email, password) =>
        api.post("/auth/login", { email, password })
           .then(r => r.data),

    register: (userData) =>
        api.post("/auth/register", userData)
           .then(r => r.data),

    confirmEmail: (token) =>
        api.post("/auth/confirm-email", null, {
            params: { token }
        }).then(r => r.data),

    resendEmailVerification: () =>
        api.post("/users/resend-email-verification")
           .then(r => r.data),

    forgotPassword: (email) =>
        api.post("/auth/forgot-password", { email })
           .then(r => r.data),

    resetPassword: (newPassword, token) =>
        api.post("/auth/reset-password", {
            newPassword,
            token
        }).then(r => r.data),
};


export default authService;
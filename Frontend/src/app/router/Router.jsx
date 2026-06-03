import { BrowserRouter, Routes, Route, Navigate } from "react-router";
import { ROUTES } from "../../shared/constants/routes";

import GuestGuard from "./guards/GuestGuard";
import AuthGuard from "./guards/AuthGuard";
import UnverifiedGuard from "./guards/UnverifiedGuard";

import ServerErrorPage from "../../shared/pages/ServerErrorPage.jsx";
import NotFoundPage from "../../shared/pages/NotFoundPage";


import ConfirmEmailScreen from "../../features/auth/confirm-email/screen/ConfirmEmailScreen";
import LoginScreen from "../../features/auth/login/screens/LoginScreen.jsx";
import VerifyEmailScreen from "../../features/auth/register/screens/VerifyEmailScreen.jsx";
import RegisterScreen from "../../features/auth/register/screens/RegisterScreen.jsx";
import ResendVerifyEmailScreen from "../../features/auth/resend-verify-email/screens/ResendVerifyEmailScreen.jsx";
import ForgotPasswordScreen from "../../features/auth/forgot-password/screens/ForgotPasswordScreen.jsx";
import ForgotPasswordSuccessScreen from "../../features/auth/forgot-password/screens/ForgotPasswordSuccessScreen.jsx";
import ResetPasswordScreen from "../../features/auth/reset-password/screens/ResetPasswordScreen.jsx";
import DashboardScreen from "../../features/dashboard/screens/DashboardScreen.jsx";


const Router = () => (
   <BrowserRouter>
        <Routes>
            
            <Route 
                path="/"
                element={
                    <Navigate to={ROUTES.LOGIN} replace />
                } 
            />

            <Route
                path={ROUTES.LOGIN}
                element={
                    <GuestGuard>
                        <LoginScreen />
                    </GuestGuard>
                }
            />

            <Route
                path={ROUTES.REGISTER}
                element={
                    <GuestGuard>
                        <RegisterScreen />
                    </GuestGuard>
                }
            />

            <Route
                path={ROUTES.VERIFY_EMAIL}
                element={ <VerifyEmailScreen /> }
            />


            <Route
                path={ROUTES.FORGOT_PASSWORD}
                element={
                    <GuestGuard>
                        <ForgotPasswordScreen />
                    </GuestGuard>
                }
            />

            <Route
                path={ROUTES.FORGOT_PASSWORD_SUCCESS}
                element={
                    <GuestGuard>
                        <ForgotPasswordSuccessScreen />
                    </GuestGuard>
                }
            />

            <Route
                path={ROUTES.RESET_PASSWORD}
                element={
                    <GuestGuard>
                        <ResetPasswordScreen />
                    </GuestGuard>
                }
            />

            <Route
                path={ROUTES.RESEND_VERIFY_EMAIL}
                element={
                    <UnverifiedGuard>
                        <ResendVerifyEmailScreen />
                    </UnverifiedGuard>
                }
            />

            <Route
                path={ROUTES.CONFIRM_EMAIL}
                element={
                        <ConfirmEmailScreen />
                }
            />
    
            <Route
                path={ROUTES.DASHBOARD}
                element={
                    <AuthGuard>
                        <DashboardScreen />
                    </AuthGuard>
                }
            />

            <Route 
                path={ROUTES.SERVER_ERROR}
                element={<ServerErrorPage />} 
            />

            <Route 
                path="*"
                element={<NotFoundPage />} 
            />
        
        </Routes>
   </BrowserRouter>
);

export default Router;

import React from "react";
import Layout from "../../../shared/components/Layout";
import { useAuth } from "../../../app/providers/AuthProvider";
import DashboardView from "../ui/DashboardView";

const DashboardScreen = () => {
    const { logout } = useAuth();

    return (
        <Layout>
            <DashboardView onLogout={logout} />
        </Layout>
    );
};

export default DashboardScreen;
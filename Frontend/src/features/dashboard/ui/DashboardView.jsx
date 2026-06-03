import React from "react";
import Button from "../../../shared/components/Button";

const DashboardView = ({ onLogout }) => {
    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold text-slate-800 mb-4">Dashboard</h1>

            <Button onClick={onLogout}>
                Logout
            </Button>
        </div>
    );
};

export default DashboardView;
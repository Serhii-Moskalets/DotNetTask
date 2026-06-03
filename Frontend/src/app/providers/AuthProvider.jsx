import { createContext, use, useContext, useEffect, useMemo, useState } from "react";

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const storedUser = localStorage.getItem("user");

        if (storedUser) {
            setUser(JSON.parse(storedUser));
        }

        setLoading(false);
    }, []);

    const login = ( userData, token ) => {
        localStorage.setItem("token", token);

        localStorage.setItem(
            "user",
            JSON.stringify(userData)
        );

        setUser(userData);
    };

    const logout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');

        setUser(null);
    };

    const value = useMemo(() => ({
        user, 
        login,
        logout,
        loading,
        isAuthenticated: !!user,
    }), [user, loading]);

    return ( 
        <AuthContext.Provider value={value} > 
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
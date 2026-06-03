import axios from "axios";
import { API_BASE_URL } from "../constants/apiConstants";
import setupInterceptors from "./interceptors";

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem("token");

    if (token) {
         config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

setupInterceptors(api);

export default api;
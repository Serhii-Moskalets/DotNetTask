import axios from "axios";
import { BASE_URL } from "../constants/apiConstants";

const healthClient = axios.create({
    baseURL: BASE_URL,
});

export const healthService = {
    check: () => healthClient.get("/healthz")
};
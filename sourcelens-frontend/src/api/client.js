import axios from "axios";

// Base URL of the ASP.NET Core Web API (SourceLens backend).
// Set VITE_API_BASE_URL in a .env file to point at your running API,
// e.g. VITE_API_BASE_URL=http://localhost:5181/api
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5181/api";

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
  timeout: 6000,
});

import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "node:path";

// Smart QR web — Vite + React 19 + Tailwind v4, consuming @wow-two-beta/ui.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 7025,
    // Dev: proxy API calls to the running SmartQr.Api (http endpoint, bound by both launch profiles).
    proxy: {
      "/api": { target: "http://localhost:7021", changeOrigin: true, secure: false },
      "/health": { target: "http://localhost:7021", changeOrigin: true, secure: false },
    },
  },
  // Prod-ish: build straight into the Api's wwwroot so the backend serves the SPA.
  build: {
    outDir: "../backend/SmartQr.Api/wwwroot",
    emptyOutDir: true,
  },
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
});

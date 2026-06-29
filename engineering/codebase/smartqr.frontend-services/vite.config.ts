import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import mkcert from "vite-plugin-mkcert";
import path from "node:path";

// Smart QR web — Vite + React 19 + Tailwind v4, consuming @wow-two-beta/ui.
export default defineConfig({
  // mkcert → HTTPS dev server (locally-trusted cert). Keeps `Secure` auth cookies + secure-context
  // working; convention: frontend/project-structure.md §Dev server.
  // Set `VITE_HTTPS=false` to skip mkcert and serve plain HTTP (e.g. headless preview tools whose
  // browser doesn't trust the local mkcert CA). Normal dev stays HTTPS.
  plugins: [react(), tailwindcss(), ...(process.env.VITE_HTTPS === "false" ? [] : [mkcert()])],
  server: {
    port: 7024,
    // Proxy /api to the backend's HTTPS (even) port with secure:false (self-signed .NET dev cert) — per
    // frontend/state-and-data.md. changeOrigin:false preserves the dev origin end-to-end.
    proxy: {
      "/api": { target: "https://localhost:7020", changeOrigin: false, secure: false },
      "/health": { target: "https://localhost:7020", changeOrigin: false, secure: false },
    },
  },
  // Prod-ish: build straight into the Api's wwwroot so the backend serves the SPA.
  build: {
    outDir: "../smartqr.backend-services/SmartQr.Api/wwwroot",
    emptyOutDir: true,
  },
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
});

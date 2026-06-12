import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import mkcert from "vite-plugin-mkcert";
import path from "node:path";

// Smart QR web — Vite + React 19 + Tailwind v4, consuming @wow-two-beta/ui.
export default defineConfig({
  // mkcert → HTTPS dev server (locally-trusted cert). Keeps `Secure` auth cookies + secure-context
  // working; convention: frontend/project-structure.md §Dev server.
  plugins: [react(), tailwindcss(), mkcert()],
  server: {
    port: 7025,
    // Proxy /api to the backend's single HTTP port (TLS is terminated upstream in prod — see
    // backend/launch-profiles.md). changeOrigin:false preserves the dev origin end-to-end.
    proxy: {
      "/api": { target: "http://localhost:7020", changeOrigin: false, secure: false },
      "/health": { target: "http://localhost:7020", changeOrigin: false, secure: false },
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

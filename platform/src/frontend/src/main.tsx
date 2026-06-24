import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { BrowserRouter } from "react-router-dom";
import { ColorModeProvider } from "@wow-two-beta/ui/primitives";
import "@fontsource-variable/geist";
import "@fontsource-variable/geist-mono";
import "./index.css";
import { App } from "./App";
import { GOOGLE_CLIENT_ID } from "./api";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ColorModeProvider>
      <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </GoogleOAuthProvider>
    </ColorModeProvider>
  </StrictMode>,
);

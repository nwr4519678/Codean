import React, { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ClerkProvider, useAuth } from "@clerk/clerk-react";
import App from "./App";
import "./styles.css";
import { setAuthTokenProvider } from "@platform/api";

function ClerkTokenBridge() {
  const { getToken } = useAuth();
  React.useEffect(() => {
    setAuthTokenProvider(() => getToken());
    return () => setAuthTokenProvider(null);
  }, [getToken]);
  return null;
}

const publishableKey = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY;
const allowedRedirectOrigins = [
  window.location.origin,
  "https://codean-web.vercel.app",
  "https://codean-admin.vercel.app",
];

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ClerkProvider
      publishableKey={publishableKey}
      allowedRedirectOrigins={allowedRedirectOrigins}
    >
      <ClerkTokenBridge />
      <BrowserRouter><App /></BrowserRouter>
    </ClerkProvider>
  </StrictMode>,
);

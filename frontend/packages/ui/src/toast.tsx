"use client";

// Re-export the Sonner toast as a thin wrapper so call sites can
// stay framework-agnostic. The actual component is rendered in apps/web
// once at the root layout. This file just publishes the API.
import { toast as sonnerToast } from "sonner";

export const ToastProvider = ({ children }: { children: React.ReactNode }) => <>{children}</>;

export const toast = {
  success: (msg: string) => sonnerToast.success(msg),
  error: (msg: string) => sonnerToast.error(msg),
  info: (msg: string) => sonnerToast.info(msg),
  warning: (msg: string) => sonnerToast.warning(msg),
  promise: sonnerToast.promise,
};

'use client';

import React, { useState } from 'react';
import { AdminSidebar } from '@/components/layout/AdminSidebar';
import { QueryProvider } from '@/providers/QueryProvider';
import './globals.css';

export default function AdminRootLayout({ children }: { children: React.ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <html lang="en" className="dark" suppressHydrationWarning>
      <body suppressHydrationWarning className="bg-background text-foreground antialiased">
        <QueryProvider>
          <div className="flex min-h-screen bg-background">
            <AdminSidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
            <div className="flex flex-1 flex-col">
              <header className="sticky top-0 z-30 h-16 border-b border-border/40 bg-background/90 backdrop-blur-md flex items-center justify-between px-6">
                <button
                  onClick={() => setSidebarOpen(true)}
                  className="flex lg:hidden h-8 w-8 items-center justify-center rounded-xl hover:bg-muted"
                >
                  <span className="sr-only">Open sidebar</span>☰
                </button>
                <span className="text-sm font-semibold text-muted-foreground">Platform Administration Console</span>
              </header>
              <main className="flex-1 overflow-y-auto">{children}</main>
            </div>
          </div>
        </QueryProvider>
      </body>
    </html>
  );
}

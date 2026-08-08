'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { getStoredRefreshToken, useCurrentUser, useLogout } from '@platform/api';
import { User, Settings, CreditCard, LogOut, ChevronDown } from 'lucide-react';
import { toast } from 'sonner';

export function UserMenu() {
  const { data: user } = useCurrentUser();
  const logoutMutation = useLogout();
  const [isOpen, setIsOpen] = useState(false);

  const handleLogout = async () => {
    try {
      const refreshToken = getStoredRefreshToken();
      if (refreshToken) {
        await logoutMutation.mutateAsync(refreshToken);
      }
      toast.success('Logged out successfully');
      window.location.href = '/auth/login';
    } catch {
      window.location.href = '/auth/login';
    }
  };

  if (!user) {
    return (
      <Link
        href="/auth/login"
        className="rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white shadow-md hover:bg-primary/90"
      >
        Sign In
      </Link>
    );
  }

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2.5 rounded-xl border border-border/60 bg-card p-1.5 pr-3 transition-all hover:bg-muted"
      >
        <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs font-bold text-white">
          {user.firstName?.[0] || 'U'}
        </div>
        <span className="text-xs font-semibold text-foreground">{user.firstName}</span>
        <ChevronDown className="h-3.5 w-3.5 text-muted-foreground" />
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-48 rounded-2xl border border-border/80 bg-card p-2 shadow-2xl z-50 animate-in fade-in zoom-in-95">
          <div className="px-3 py-2 border-b border-border/40 mb-1">
            <p className="text-xs font-bold text-foreground">{user.firstName} {user.lastName}</p>
            <p className="text-[10px] text-muted-foreground truncate">{user.email}</p>
          </div>

          <Link
            href={user.role === 'Teacher' || user.role === 'Admin' ? '/teacher/dashboard' : '/dashboard'}
            onClick={() => setIsOpen(false)}
            className="flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <User className="h-3.5 w-3.5" />
            Dashboard
          </Link>
          <Link
            href="/profile"
            onClick={() => setIsOpen(false)}
            className="flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <User className="h-3.5 w-3.5" />
            Profile
          </Link>
          <Link
            href="/settings"
            onClick={() => setIsOpen(false)}
            className="flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <Settings className="h-3.5 w-3.5" />
            Settings
          </Link>
          <Link
            href="/billing"
            onClick={() => setIsOpen(false)}
            className="flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <CreditCard className="h-3.5 w-3.5" />
            Billing
          </Link>

          <button
            onClick={handleLogout}
            className="flex w-full items-center gap-2 rounded-xl px-3 py-2 text-xs font-medium text-destructive hover:bg-destructive/10 mt-1 border-t border-border/40"
          >
            <LogOut className="h-3.5 w-3.5" />
            Sign Out
          </button>
        </div>
      )}
    </div>
  );
}

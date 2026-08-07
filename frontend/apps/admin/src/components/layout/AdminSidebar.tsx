'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Code2, LayoutDashboard, Users, BookOpen, FileText, CreditCard, ShieldAlert, Settings, X } from 'lucide-react';

const ADMIN_NAV = [
  { href: '/', label: 'Overview', icon: LayoutDashboard },
  { href: '/users', label: 'User Management', icon: Users },
  { href: '/courses', label: 'Courses', icon: BookOpen },
  { href: '/subscriptions', label: 'Subscriptions', icon: CreditCard },
  { href: '/audit-logs', label: 'Audit Logs', icon: FileText },
  { href: '/settings', label: 'Platform Settings', icon: Settings },
];

interface AdminSidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
}

export function AdminSidebar({ isOpen = true, onClose }: AdminSidebarProps) {
  const pathname = usePathname();

  return (
    <>
      {isOpen && (
        <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm lg:hidden" onClick={onClose} />
      )}

      <aside
        className={`fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-border/60 bg-card shadow-2xl transition-transform duration-300 lg:static lg:translate-x-0 lg:shadow-none ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex h-16 items-center justify-between border-b border-border/40 px-5">
          <Link href="/" className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-destructive text-white shadow-md">
              <ShieldAlert className="h-5 w-5" />
            </div>
            <span className="text-base font-extrabold tracking-tight">Admin Panel</span>
          </Link>
          {onClose && (
            <button onClick={onClose} className="flex lg:hidden h-7 w-7 items-center justify-center rounded-lg hover:bg-muted">
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        <nav className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
          {ADMIN_NAV.map(({ href, label, icon: Icon }) => {
            const isActive = pathname === href;
            return (
              <Link key={href} href={href} onClick={onClose}
                className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                  isActive ? 'bg-destructive text-white shadow-md shadow-destructive/20' : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                }`}
              >
                <Icon className="h-4 w-4 shrink-0" />
                <span>{label}</span>
              </Link>
            );
          })}
        </nav>

        <div className="border-t border-border/40 p-4">
          <p className="text-[10px] text-muted-foreground text-center">Platform Admin v1.0</p>
        </div>
      </aside>
    </>
  );
}

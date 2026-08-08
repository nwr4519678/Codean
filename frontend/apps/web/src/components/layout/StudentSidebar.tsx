'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Code2, BookOpen, Trophy, Video, Bell, User, Settings, CreditCard, BarChart2, X, FileText, Megaphone, Award } from 'lucide-react';

const NAV_ITEMS = [
  { href: '/dashboard', label: 'Dashboard', icon: BarChart2 },
  { href: '/courses', label: 'My Courses', icon: BookOpen },
  { href: '/student/exams', label: 'Exams & Homework', icon: FileText },
  { href: '/judge', label: 'Code Judge', icon: Trophy },
  { href: '/live', label: 'Live Sessions', icon: Video },
  { href: '/student/announcements', label: 'Announcements', icon: Megaphone },
  { href: '/student/certificates', label: 'My Certificates', icon: Award },
  { href: '/profile', label: 'Profile', icon: User },
  { href: '/settings', label: 'Settings', icon: Settings },
  { href: '/billing', label: 'Billing', icon: CreditCard },
];

interface StudentSidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
}

export function StudentSidebar({ isOpen = true, onClose }: StudentSidebarProps) {
  const pathname = usePathname();

  return (
    <>
      {/* Mobile overlay */}
      {isOpen && (
        <div suppressHydrationWarning className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm lg:hidden" onClick={onClose} />
      )}

      <aside
        suppressHydrationWarning
        className={`fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-border/60 bg-card shadow-2xl transition-transform duration-300 lg:static lg:translate-x-0 lg:shadow-none ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {/* Logo */}
        <div suppressHydrationWarning className="flex h-16 items-center justify-between border-b border-border/40 px-5">
          <Link href="/dashboard" className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-primary text-white shadow-md shadow-primary/25">
              <Code2 className="h-5 w-5" />
            </div>
            <span className="text-base font-extrabold tracking-tight">Platform</span>
          </Link>
          {onClose && (
            <button onClick={onClose} className="flex lg:hidden h-7 w-7 items-center justify-center rounded-lg hover:bg-muted">
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        {/* Navigation */}
        <nav suppressHydrationWarning className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
          {NAV_ITEMS.map(({ href, label, icon: Icon }) => {
            const isActive = pathname.includes(href);
            return (
              <Link
                key={href}
                href={href}
                onClick={onClose}
                className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                  isActive
                    ? 'bg-primary text-white shadow-md shadow-primary/20'
                    : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                }`}
              >
                <Icon className="h-4 w-4 shrink-0" />
                <span>{label}</span>
              </Link>
            );
          })}
        </nav>

        {/* Footer */}
        <div suppressHydrationWarning className="border-t border-border/40 p-4">
          <p className="text-[10px] text-muted-foreground text-center">Platform v1.0 · Software Engineering</p>
        </div>
      </aside>
    </>
  );
}

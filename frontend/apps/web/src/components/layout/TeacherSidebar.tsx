'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Code2, BookOpen, PlusCircle, Users, BarChart2, Video, X } from 'lucide-react';

const TEACHER_NAV = [
  { href: '/teacher/dashboard', label: 'Dashboard', icon: BarChart2 },
  { href: '/teacher/courses', label: 'My Courses', icon: BookOpen },
  { href: '/teacher/courses/new', label: 'Create Course', icon: PlusCircle },
  { href: '/teacher/students', label: 'Students', icon: Users },
  { href: '/teacher/live', label: 'Live Sessions', icon: Video },
];

interface TeacherSidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
}

export function TeacherSidebar({ isOpen = true, onClose }: TeacherSidebarProps) {
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
          <Link href="/teacher/dashboard" className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-accent text-white shadow-md shadow-accent/25">
              <Code2 className="h-5 w-5" />
            </div>
            <span className="text-base font-extrabold tracking-tight">Teacher Hub</span>
          </Link>
          {onClose && (
            <button onClick={onClose} className="flex lg:hidden h-7 w-7 items-center justify-center rounded-lg hover:bg-muted">
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        <nav className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
          {TEACHER_NAV.map(({ href, label, icon: Icon }) => {
            const isActive = pathname === href;
            return (
              <Link key={href} href={href} onClick={onClose}
                className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                  isActive ? 'bg-accent text-white shadow-md shadow-accent/20' : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                }`}
              >
                <Icon className="h-4 w-4 shrink-0" />
                <span>{label}</span>
              </Link>
            );
          })}
        </nav>
      </aside>
    </>
  );
}

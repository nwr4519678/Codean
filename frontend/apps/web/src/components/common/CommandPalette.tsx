'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Search, BookOpen, Code2, Video, CreditCard, Settings, User } from 'lucide-react';

interface CommandPaletteProps {
  isOpen?: boolean;
  onClose?: () => void;
}

export function CommandPalette({ isOpen = false, onClose }: CommandPaletteProps) {
  const [query, setQuery] = useState('');
  const router = useRouter();

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && onClose) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  if (!isOpen) return null;

  const navItems = [
    { label: 'Browse Courses', href: '/courses', icon: BookOpen },
    { label: 'Code Judge Challenges', href: '/judge', icon: Code2 },
    { label: 'Live Sessions', href: '/live', icon: Video },
    { label: 'Billing & Plans', href: '/billing', icon: CreditCard },
    { label: 'Profile Settings', href: '/settings', icon: Settings },
    { label: 'My Profile', href: '/profile', icon: User },
  ];

  const filtered = navItems.filter((i) => i.label.toLowerCase().includes(query.toLowerCase()));

  const handleSelect = (href: string) => {
    if (onClose) onClose();
    router.push(href);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-20 bg-background/80 backdrop-blur-md animate-in fade-in" onClick={onClose}>
      <div className="w-full max-w-xl rounded-3xl border border-border/80 bg-card p-4 shadow-2xl space-y-3" onClick={(e) => e.stopPropagation()}>
        <div className="flex items-center gap-3 border-b border-border/60 pb-3 px-2">
          <Search className="h-5 w-5 text-muted-foreground" />
          <input
            type="text"
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Type a command or search platform (Ctrl + K)..."
            className="w-full bg-transparent text-sm text-foreground focus:outline-none"
          />
          <kbd className="rounded-md bg-muted px-2 py-0.5 text-[10px] font-mono text-muted-foreground cursor-pointer" onClick={onClose}>ESC</kbd>
        </div>

        <div className="max-h-64 overflow-y-auto space-y-1">
          {filtered.map((item) => (
            <button
              key={item.href}
              onClick={() => handleSelect(item.href)}
              className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-xs font-semibold text-foreground transition-all hover:bg-primary/10 hover:text-primary"
            >
              <item.icon className="h-4 w-4 text-muted-foreground" />
              <span>{item.label}</span>
            </button>
          ))}
          {filtered.length === 0 && (
            <div className="py-6 text-center text-xs text-muted-foreground">No matching commands found.</div>
          )}
        </div>
      </div>
    </div>
  );
}

'use client';

import React, { useState, useEffect } from 'react';
import { Menu, Search } from 'lucide-react';
import { ThemeToggle } from '@/components/common/ThemeToggle';
import { LanguageSwitcher } from '@/components/common/LanguageSwitcher';
import { UserMenu } from '@/components/common/UserMenu';
import { NotificationPopover } from '@/components/common/NotificationPopover';
import { CommandPalette } from '@/components/common/CommandPalette';
import { Breadcrumbs } from '@/components/layout/Breadcrumbs';

interface DashboardHeaderProps {
  onMenuToggle?: () => void;
}

export function DashboardHeader({ onMenuToggle }: DashboardHeaderProps) {
  const [showSearch, setShowSearch] = useState(false);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        setShowSearch((prev) => !prev);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  return (
    <>
      <header className="sticky top-0 z-30 w-full border-b border-border/40 bg-background/90 backdrop-blur-md h-16 flex items-center px-4 sm:px-6">
        <div className="flex w-full items-center gap-3">
          {/* Mobile menu toggle */}
          <button
            onClick={onMenuToggle}
            className="flex lg:hidden h-8 w-8 items-center justify-center rounded-xl hover:bg-muted"
            aria-label="Toggle sidebar"
          >
            <Menu className="h-5 w-5" />
          </button>

          {/* Breadcrumbs */}
          <div className="flex-1 hidden sm:block">
            <Breadcrumbs />
          </div>

          {/* Right controls */}
          <div className="flex items-center gap-2 ml-auto">
            <button
              onClick={() => setShowSearch(true)}
              className="flex h-8 items-center gap-2 rounded-xl border border-border/60 bg-card px-3 text-xs text-muted-foreground hover:bg-muted transition-all"
              aria-label="Search"
            >
              <Search className="h-3.5 w-3.5" />
              <span className="hidden md:inline">Search...</span>
              <kbd className="hidden md:inline-flex h-4 items-center rounded border border-border bg-muted px-1 text-[10px] font-mono">
                ⌘K
              </kbd>
            </button>
            <NotificationPopover />
            <LanguageSwitcher />
            <ThemeToggle />
            <UserMenu />
          </div>
        </div>
      </header>

      {/* Command Palette Modal */}
      {showSearch && <CommandPalette isOpen={showSearch} onClose={() => setShowSearch(false)} />}
    </>
  );
}

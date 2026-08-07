'use client';

import React, { useState } from 'react';
import { Menu, Bell, Search } from 'lucide-react';
import { ThemeToggle } from '@/components/common/ThemeToggle';
import { LanguageSwitcher } from '@/components/common/LanguageSwitcher';
import { UserMenu } from '@/components/common/UserMenu';
import { Breadcrumbs } from '@/components/layout/Breadcrumbs';

interface DashboardHeaderProps {
  onMenuToggle?: () => void;
}

export function DashboardHeader({ onMenuToggle }: DashboardHeaderProps) {
  const [showSearch, setShowSearch] = useState(false);

  return (
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
            onClick={() => setShowSearch(!showSearch)}
            className="flex h-8 w-8 items-center justify-center rounded-xl hover:bg-muted text-muted-foreground"
            aria-label="Search"
          >
            <Search className="h-4 w-4" />
          </button>
          <button className="relative flex h-8 w-8 items-center justify-center rounded-xl hover:bg-muted text-muted-foreground" aria-label="Notifications">
            <Bell className="h-4 w-4" />
            <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-primary shadow" />
          </button>
          <LanguageSwitcher />
          <ThemeToggle />
          <UserMenu />
        </div>
      </div>
    </header>
  );
}

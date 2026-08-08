'use client';

import React, { useState } from 'react';
import { Bell, Code2, Award, Clock, Check } from 'lucide-react';
import { useNotifications, useMarkNotificationRead } from '@platform/api';
import Link from 'next/link';

export function NotificationPopover() {
  const [isOpen, setIsOpen] = useState(false);
  const { data, isLoading } = useNotifications({ pageSize: 5 });
  const markRead = useMarkNotificationRead();

  const unreadCount = data?.items?.filter((i) => !i.isRead).length || 0;

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative flex h-8 w-8 items-center justify-center rounded-xl hover:bg-muted text-muted-foreground transition-colors"
        aria-label="Notifications"
      >
        <Bell className="h-4 w-4" />
        {unreadCount > 0 && (
          <span className="absolute right-1.5 top-1.5 flex h-2 w-2">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary opacity-75"></span>
            <span className="relative inline-flex rounded-full h-2 w-2 bg-primary"></span>
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 rounded-2xl border border-border/80 bg-card p-3 shadow-2xl z-50 animate-in fade-in zoom-in-95">
          <div className="flex items-center justify-between border-b border-border/40 pb-2 px-2">
            <span className="text-xs font-bold">Notifications</span>
            <Link
              href="/notifications"
              onClick={() => setIsOpen(false)}
              className="text-[11px] font-semibold text-primary hover:underline"
            >
              View All
            </Link>
          </div>

          <div className="divide-y divide-border/40 max-h-80 overflow-y-auto my-1">
            {isLoading ? (
              <p className="p-4 text-center text-xs text-muted-foreground">Loading notifications...</p>
            ) : data?.items?.length ? (
              data.items.map((item) => (
                <div
                  key={item.id}
                  className={`p-2.5 text-xs transition-colors rounded-xl ${
                    item.isRead ? 'opacity-75' : 'bg-primary/5 font-medium'
                  }`}
                >
                  <p className="font-bold text-foreground">{item.title}</p>
                  <p className="text-[11px] text-muted-foreground line-clamp-2 mt-0.5">{item.body}</p>
                </div>
              ))
            ) : (
              <p className="p-4 text-center text-xs text-muted-foreground">No notifications yet.</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

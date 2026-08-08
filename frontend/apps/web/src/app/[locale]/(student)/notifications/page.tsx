'use client';

import React from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Bell, Code2, Award, Clock, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { useMarkNotificationRead, useNotifications } from '@platform/api';

export default function NotificationsPage() {
  const { data, isLoading, isError } = useNotifications({ pageSize: 20 });
  const markRead = useMarkNotificationRead();

  const handleMarkRead = async (id: number) => {
    try {
      await markRead.mutateAsync(id);
      toast.success('Notification marked as read');
    } catch {
      toast.error('Unable to update this notification.');
    }
  };

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 max-w-4xl">
      <PageHeader
        title="Notifications"
        description="Stay updated with your course progress, judge submissions, and platform alerts."
      />

      {isLoading ? <div className="flex min-h-48 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div> : isError ? (
        <p role="alert" className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">Notifications could not be loaded. Please try again.</p>
      ) : data?.items.length ? <div className="space-y-4">
        {data.items.map((item) => (
          <div
            key={item.id}
            className={`flex items-start justify-between rounded-2xl border p-5 transition-all ${
              item.isRead ? 'border-border/60 bg-card/60' : 'border-primary/40 bg-primary/5 ring-1 ring-primary/20 shadow-sm'
            }`}
          >
            <div className="flex items-start gap-4">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                {item.type === 'challenge' ? <Code2 className="h-5 w-5" /> : item.type === 'course' ? <Award className="h-5 w-5" /> : <Bell className="h-5 w-5" />}
              </div>
              <div className="space-y-1">
                <h3 className="text-sm font-bold text-foreground">{item.title}</h3>
                <p className="text-xs text-muted-foreground">{item.body}</p>
                <span className="inline-flex items-center gap-1 text-[10px] text-muted-foreground pt-1">
                  <Clock className="h-3 w-3" /> {new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(item.createdAt))}
                </span>
              </div>
            </div>

            {!item.isRead && (
              <button
                onClick={() => handleMarkRead(item.id)}
                disabled={markRead.isPending}
                className="text-xs font-semibold text-primary hover:underline shrink-0"
              >
                Mark Read
              </button>
            )}
          </div>
        ))}
      </div> : <div className="rounded-2xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">You are all caught up—there are no notifications yet.</div>}
    </div>
  );
}

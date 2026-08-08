'use client';

import React from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Megaphone, Calendar, Bell, Loader2 } from 'lucide-react';
import { useAnnouncements } from '@platform/api';

export default function StudentAnnouncementsPage() {
  const { data: announcementsData, isLoading } = useAnnouncements();

  const announcements = announcementsData?.items ?? [];

  return (
    <div className="container mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Course Announcements"
        description="Important news, live class schedules, and updates from your teachers."
      />

      {isLoading ? (
        <div className="flex min-h-[300px] items-center justify-center rounded-2xl border border-border bg-card">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : announcements.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border p-12 text-center text-muted-foreground">
          <Megaphone className="mx-auto h-10 w-10 opacity-40 mb-3" />
          <p className="text-sm font-semibold">No announcements posted yet.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {announcements.map((ann: any) => (
            <div key={ann.id} className="rounded-2xl border border-border/80 bg-card p-6 shadow-sm space-y-2">
              <div className="flex items-center justify-between">
                <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-3 py-1 text-xs font-bold text-primary">
                  <Bell className="h-3.5 w-3.5" /> Announcement
                </span>
                <span className="text-xs text-muted-foreground flex items-center gap-1">
                  <Calendar className="h-3.5 w-3.5" /> {new Date(ann.createdAt || Date.now()).toLocaleDateString()}
                </span>
              </div>
              <h3 className="text-lg font-bold pt-1">{ann.title}</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">{ann.content}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

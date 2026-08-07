'use client';

import React from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Bell, Code2, Award, Clock } from 'lucide-react';
import { toast } from 'sonner';

interface NotificationItem {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export default function NotificationsPage() {
  const dummyNotifications: NotificationItem[] = [
    {
      id: 1,
      title: 'Code Judge Challenge Passed!',
      message: 'You scored 100 XP on the "Two Sum Problem". Keep it up!',
      type: 'challenge',
      isRead: false,
      createdAt: '10 mins ago',
    },
    {
      id: 2,
      title: 'New Lesson Released',
      message: 'Module 4: CQRS Pattern with MediatR is now available in C# Clean Architecture.',
      type: 'course',
      isRead: true,
      createdAt: '2 hours ago',
    },
    {
      id: 3,
      title: 'Live Q&A Session Tomorrow',
      message: 'Join instructor Sarah Connor live at 8:00 PM UTC for System Design Q&A.',
      type: 'live',
      isRead: true,
      createdAt: '1 day ago',
    },
  ];

  const handleMarkRead = (id: number) => {
    toast.success('Notification marked as read');
  };

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 max-w-4xl">
      <PageHeader
        title="Notifications"
        description="Stay updated with your course progress, judge submissions, and platform alerts."
      />

      <div className="space-y-4">
        {dummyNotifications.map((item: NotificationItem) => (
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
                <p className="text-xs text-muted-foreground">{item.message}</p>
                <span className="inline-flex items-center gap-1 text-[10px] text-muted-foreground pt-1">
                  <Clock className="h-3 w-3" /> {item.createdAt}
                </span>
              </div>
            </div>

            {!item.isRead && (
              <button
                onClick={() => handleMarkRead(item.id)}
                className="text-xs font-semibold text-primary hover:underline shrink-0"
              >
                Mark Read
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

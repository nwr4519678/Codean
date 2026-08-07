'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { Video, Users, Calendar, ExternalLink } from 'lucide-react';

export default function LiveSessionsPage() {
  const sessions = [
    {
      id: 1,
      title: 'System Design: Design Twitter at Scale',
      instructor: 'Sarah Connor',
      scheduledAt: 'Aug 10, 2026 · 20:00 UTC',
      status: 'upcoming',
      attendees: 128,
    },
    {
      id: 2,
      title: 'C# Clean Architecture Live Code Review',
      instructor: 'John Doe',
      scheduledAt: 'Aug 14, 2026 · 19:00 UTC',
      status: 'upcoming',
      attendees: 85,
    },
    {
      id: 3,
      title: 'Python Async Patterns Deep Dive',
      instructor: 'Sarah Connor',
      scheduledAt: 'Aug 5, 2026 · 18:00 UTC',
      status: 'recorded',
      attendees: 312,
    },
  ];

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Live Q&A Sessions"
        description="Join instructor-led live sessions and access past recordings."
      />

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {sessions.map((session) => (
          <div
            key={session.id}
            className="flex flex-col justify-between rounded-2xl border border-border/70 bg-card p-6 shadow-sm hover:border-primary/40 hover:shadow-lg transition-all"
          >
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span
                  className={`rounded-full px-3 py-1 text-xs font-semibold ${
                    session.status === 'upcoming'
                      ? 'bg-emerald-500/10 text-emerald-500'
                      : 'bg-muted text-muted-foreground'
                  }`}
                >
                  {session.status === 'upcoming' ? '🔴 Live Upcoming' : '📹 Recorded'}
                </span>
              </div>

              <h3 className="text-base font-bold leading-snug">{session.title}</h3>

              <div className="space-y-1.5 text-xs text-muted-foreground">
                <p className="flex items-center gap-1.5"><Users className="h-3.5 w-3.5" />{session.instructor}</p>
                <p className="flex items-center gap-1.5"><Calendar className="h-3.5 w-3.5" />{session.scheduledAt}</p>
                <p className="flex items-center gap-1.5"><Video className="h-3.5 w-3.5" />{session.attendees} {session.status === 'upcoming' ? 'enrolled' : 'attended'}</p>
              </div>
            </div>

            <Link
              href={`/live/${session.id}`}
              className="mt-6 flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90 transition-all"
            >
              {session.status === 'upcoming' ? 'Join Session' : 'Watch Recording'}
              <ExternalLink className="h-3.5 w-3.5" />
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}

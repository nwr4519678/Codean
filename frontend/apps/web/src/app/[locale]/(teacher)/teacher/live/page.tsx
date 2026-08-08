'use client';

import React from 'react';
import { Video, Calendar, PlusCircle, ArrowLeft } from 'lucide-react';
import Link from 'next/link';

export default function TeacherLiveSessionsPage() {
  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/teacher/dashboard" className="flex h-9 w-9 items-center justify-center rounded-xl border border-border hover:bg-muted text-muted-foreground">
            <ArrowLeft className="h-4 w-4" />
          </Link>
          <div>
            <h1 className="text-2xl font-extrabold tracking-tight">Live Sessions</h1>
            <p className="text-sm text-muted-foreground mt-0.5">Schedule and manage live interactive classes with your students</p>
          </div>
        </div>

        <button
          onClick={() => alert('Live session scheduling feature coming soon!')}
          className="inline-flex items-center gap-2 rounded-xl bg-accent px-4 py-2.5 text-xs font-semibold text-white shadow-lg hover:bg-accent/90"
        >
          <PlusCircle className="h-4 w-4" />
          Schedule Live Class
        </button>
      </div>

      {/* Placeholder / Empty State */}
      <div className="flex flex-col items-center justify-center py-20 rounded-3xl border border-dashed border-border bg-card/50 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-accent/10 text-accent mb-4">
          <Video className="h-8 w-8" />
        </div>
        <h3 className="text-lg font-bold">No Upcoming Live Sessions</h3>
        <p className="text-sm text-muted-foreground max-w-md mt-1 mb-6">
          You haven&apos;t scheduled any live classes yet. Broadcast interactive video sessions and interact with your students in real-time.
        </p>
        <button
          onClick={() => alert('Live session scheduling feature coming soon!')}
          className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90"
        >
          <Calendar className="h-4 w-4" />
          Schedule Your First Session
        </button>
      </div>
    </div>
  );
}

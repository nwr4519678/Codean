'use client';

import React from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCurrentUser } from '@platform/api';
import { User, Mail, Shield, Trophy, BookOpen, Calendar } from 'lucide-react';

export default function ProfilePage() {
  const { data: user } = useCurrentUser();

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 max-w-4xl">
      <PageHeader title="Student Profile" description="Your personal information, enrolled courses, and earned badges." />

      <div className="rounded-3xl border border-border/70 bg-card p-8 shadow-xl space-y-8">
        <div className="flex items-center gap-6 pb-6 border-b border-border/60">
          <div className="flex h-20 w-20 items-center justify-center rounded-3xl bg-primary text-2xl font-black text-white shadow-xl shadow-primary/25">
            {user?.firstName?.[0] || 'U'}
          </div>
          <div>
            <h2 className="text-2xl font-bold">{user?.firstName} {user?.lastName}</h2>
            <p className="text-sm text-muted-foreground">{user?.email}</p>
            <span className="mt-2 inline-block rounded-full bg-primary/10 px-3 py-0.5 text-xs font-semibold text-primary">
              {user?.role || 'Student Learner'}
            </span>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          <div className="flex items-center gap-3 rounded-2xl border border-border/60 bg-muted/30 p-4">
            <Trophy className="h-6 w-6 text-amber-500" />
            <div>
              <span className="text-xs text-muted-foreground">Total XP</span>
              <p className="text-lg font-bold">1,450 XP</p>
            </div>
          </div>
          <div className="flex items-center gap-3 rounded-2xl border border-border/60 bg-muted/30 p-4">
            <BookOpen className="h-6 w-6 text-primary" />
            <div>
              <span className="text-xs text-muted-foreground">Enrolled Courses</span>
              <p className="text-lg font-bold">4 Courses</p>
            </div>
          </div>
          <div className="flex items-center gap-3 rounded-2xl border border-border/60 bg-muted/30 p-4">
            <Shield className="h-6 w-6 text-emerald-500" />
            <div>
              <span className="text-xs text-muted-foreground">Certificates</span>
              <p className="text-lg font-bold">2 Earned</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

'use client';

import React from 'react';
import { BookOpen, Code2, Trophy, Clock, PlayCircle } from 'lucide-react';
import { useCurrentUser } from '@platform/api';
import Link from 'next/link';

export default function StudentDashboard() {
  const { data: user, isLoading } = useCurrentUser();

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header Greeting */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Welcome back, {isLoading ? 'Student' : user?.firstName || 'Student'}! 👋
          </h1>
          <p className="text-muted-foreground">Continue where you left off and complete your learning goals today.</p>
        </div>
        <Link
          href="/courses"
          className="inline-flex items-center justify-center rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90"
        >
          Browse Courses
        </Link>
      </div>

      {/* Overview Stat Cards */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-muted-foreground">Enrolled Courses</span>
            <BookOpen className="h-5 w-5 text-primary" />
          </div>
          <p className="mt-4 text-3xl font-bold">4</p>
          <span className="mt-1 text-xs text-emerald-500">2 in progress</span>
        </div>

        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-muted-foreground">Challenges Solved</span>
            <Code2 className="h-5 w-5 text-accent" />
          </div>
          <p className="mt-4 text-3xl font-bold">28</p>
          <span className="mt-1 text-xs text-muted-foreground">+5 this week</span>
        </div>

        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-muted-foreground">Learning Time</span>
            <Clock className="h-5 w-5 text-purple-500" />
          </div>
          <p className="mt-4 text-3xl font-bold">18.5 hrs</p>
          <span className="mt-1 text-xs text-emerald-500">Top 10% learner</span>
        </div>

        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-muted-foreground">XP Points</span>
            <Trophy className="h-5 w-5 text-amber-500" />
          </div>
          <p className="mt-4 text-3xl font-bold">1,450</p>
          <span className="mt-1 text-xs text-amber-500">Level 4 Engineer</span>
        </div>
      </div>

      {/* In Progress Courses */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold tracking-tight">Active Learning</h2>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          <div className="flex flex-col justify-between rounded-2xl border border-border bg-card p-6">
            <div className="space-y-2">
              <span className="inline-block rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                C# & .NET 10
              </span>
              <h3 className="text-lg font-bold">Clean Architecture & DDD in .NET 10</h3>
              <p className="text-sm text-muted-foreground">Module 4: CQRS & MediatR Implementation</p>
            </div>
            <div className="mt-6 space-y-3">
              <div className="flex justify-between text-xs font-medium">
                <span>Progress</span>
                <span>65%</span>
              </div>
              <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
                <div className="h-full bg-primary transition-all duration-300" style={{ width: '65%' }} />
              </div>
              <Link
                href="/learn/1"
                className="mt-4 inline-flex w-full items-center justify-center gap-2 rounded-xl border border-primary text-primary py-2 text-sm font-semibold hover:bg-primary hover:text-white transition-colors"
              >
                <PlayCircle className="h-4 w-4" />
                Continue Lesson
              </Link>
            </div>
          </div>

          <div className="flex flex-col justify-between rounded-2xl border border-border bg-card p-6">
            <div className="space-y-2">
              <span className="inline-block rounded-full bg-accent/10 px-3 py-1 text-xs font-semibold text-accent">
                Python
              </span>
              <h3 className="text-lg font-bold">Algorithms & Data Structures in Python</h3>
              <p className="text-sm text-muted-foreground">Module 2: Binary Search Trees & Graphs</p>
            </div>
            <div className="mt-6 space-y-3">
              <div className="flex justify-between text-xs font-medium">
                <span>Progress</span>
                <span>40%</span>
              </div>
              <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
                <div className="h-full bg-accent transition-all duration-300" style={{ width: '40%' }} />
              </div>
              <Link
                href="/learn/2"
                className="mt-4 inline-flex w-full items-center justify-center gap-2 rounded-xl border border-accent text-accent py-2 text-sm font-semibold hover:bg-accent hover:text-white transition-colors"
              >
                <PlayCircle className="h-4 w-4" />
                Continue Lesson
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { PlusCircle, GraduationCap, Users, DollarSign, BookOpen } from 'lucide-react';
import { useTeacherProfile } from '@platform/api';

export default function TeacherDashboardPage() {
  const { data: profile } = useTeacherProfile();

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Teacher Dashboard"
        description="Manage your published courses, student enrollments, and earnings."
        action={
          <Link
            href="/teacher/courses/new"
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90"
          >
            <PlusCircle className="h-4 w-4" />
            Create New Course
          </Link>
        }
      />

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase text-muted-foreground">Total Students</span>
            <Users className="h-5 w-5 text-primary" />
          </div>
          <p className="mt-4 text-3xl font-extrabold">{profile?.totalStudents || 1240}</p>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase text-muted-foreground">Active Courses</span>
            <GraduationCap className="h-5 w-5 text-accent" />
          </div>
          <p className="mt-4 text-3xl font-extrabold">{profile?.totalCourses || 3}</p>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase text-muted-foreground">Total Revenue</span>
            <DollarSign className="h-5 w-5 text-emerald-500" />
          </div>
          <p className="mt-4 text-3xl font-extrabold">$14,850</p>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase text-muted-foreground">Average Rating</span>
            <BookOpen className="h-5 w-5 text-purple-500" />
          </div>
          <p className="mt-4 text-3xl font-extrabold">{profile?.averageRating || 4.9} ★</p>
        </div>
      </div>
    </div>
  );
}

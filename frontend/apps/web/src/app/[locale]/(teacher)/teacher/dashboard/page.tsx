'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  PlusCircle, BookOpen, Users, Star, Video,
  Clock, ArrowRight, Edit, Eye, PlayCircle, BarChart2,
  ChevronUp, Award, Zap, MessageSquare, Loader2, AlertCircle
} from 'lucide-react';
import { useCurrentUser } from '@platform/api';
import { useCourses } from '@platform/api';

export default function TeacherDashboardPage() {
  const { data: user } = useCurrentUser();

  const { data: coursesData, isLoading: coursesLoading, isError: coursesError } =
    useCourses({ teacherId: user?.id ? Number(user.id) : undefined });

  const courses = coursesData?.items ?? [];
  const totalStudents = courses.reduce((sum, c) => sum + (c.enrollmentCount ?? 0), 0);
  const publishedCourses = courses.filter(c => c.isPublished).length;
  const avgRating =
    courses.length > 0
      ? (courses.reduce((sum, c) => sum + (c.rating ?? 0), 0) / courses.length).toFixed(1)
      : '—';

  const firstName = user?.firstName ?? 'Teacher';

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="flex flex-col gap-4 rounded-3xl border border-border/80 bg-gradient-to-r from-card via-card to-accent/5 p-6 shadow-sm lg:flex-row lg:items-center lg:justify-between">
        <div className="space-y-1">
          <div className="inline-flex items-center gap-2 rounded-full border border-accent/20 bg-accent/10 px-3 py-1 text-xs font-semibold text-accent">
            <Zap className="h-3.5 w-3.5" />
            Teacher Hub
          </div>
          <h1 className="text-3xl font-extrabold tracking-tight">
            Welcome back, {firstName}! 👋
          </h1>
          <p className="text-sm text-muted-foreground">Manage your courses and track your students' progress.</p>
        </div>
        <div className="flex flex-wrap gap-3">
          <Link
            href="/teacher/courses/new"
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-colors"
          >
            <PlusCircle className="h-4 w-4" />
            Create Course
          </Link>
          <Link
            href="/teacher/live"
            className="inline-flex items-center gap-2 rounded-xl border border-border bg-card px-5 py-2.5 text-sm font-semibold hover:bg-muted transition-colors"
          >
            <Video className="h-4 w-4 text-emerald-500" />
            Schedule Live
          </Link>
        </div>
      </div>

      {/* KPI Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Total Students</span>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
              <Users className="h-4 w-4 text-primary" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{coursesLoading ? '—' : totalStudents.toLocaleString()}</p>
          <p className="mt-1 text-xs text-muted-foreground">Across all your courses</p>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Active Courses</span>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-accent/10">
              <BookOpen className="h-4 w-4 text-accent" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{coursesLoading ? '—' : publishedCourses}</p>
          <p className="mt-1 text-xs text-muted-foreground">{courses.length - publishedCourses} in draft</p>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Average Rating</span>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-500/10">
              <Star className="h-4 w-4 text-amber-500" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{coursesLoading ? '—' : `${avgRating} ★`}</p>
          <p className="mt-1 text-xs text-muted-foreground">From student reviews</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Courses Table */}
        <div className="space-y-4 lg:col-span-2">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold tracking-tight flex items-center gap-2">
              <BarChart2 className="h-5 w-5 text-primary" />
              My Courses
            </h2>
            <Link href="/teacher/courses" className="text-xs font-bold text-primary hover:underline flex items-center gap-1">
              View All <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          <div className="rounded-2xl border border-border/70 bg-card shadow-sm overflow-hidden">
            {coursesLoading ? (
              <div className="flex items-center justify-center py-16">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
              </div>
            ) : coursesError ? (
              <div className="flex flex-col items-center justify-center py-16 gap-2 text-destructive">
                <AlertCircle className="h-6 w-6" />
                <p className="text-sm font-medium">Could not load courses</p>
              </div>
            ) : courses.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-16 gap-3 text-muted-foreground">
                <BookOpen className="h-8 w-8 opacity-40" />
                <p className="text-sm font-medium">No courses yet</p>
                <Link
                  href="/teacher/courses/new"
                  className="inline-flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90"
                >
                  <PlusCircle className="h-3.5 w-3.5" /> Create your first course
                </Link>
              </div>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border/60 bg-muted/30">
                    <th className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-muted-foreground">Course</th>
                    <th className="px-4 py-3 text-center text-xs font-bold uppercase tracking-wider text-muted-foreground">Students</th>
                    <th className="px-4 py-3 text-center text-xs font-bold uppercase tracking-wider text-muted-foreground">Status</th>
                    <th className="px-4 py-3"></th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/40">
                  {courses.slice(0, 8).map((c) => (
                    <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-4 py-4">
                        <p className="font-semibold text-sm line-clamp-1">{c.title}</p>
                        <p className="text-xs text-muted-foreground mt-0.5">{c.category}</p>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="inline-flex items-center gap-1 text-sm font-bold">
                          <Users className="h-3.5 w-3.5 text-muted-foreground" />
                          {c.enrollmentCount ?? 0}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className={`inline-block rounded-full px-2 py-0.5 text-[10px] font-bold uppercase ${c.isPublished ? 'bg-emerald-500/10 text-emerald-500' : 'bg-amber-500/10 text-amber-600'}`}>
                          {c.isPublished ? 'Published' : 'Draft'}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-right">
                        <Link href={`/teacher/courses/${c.id}`} className="inline-flex items-center gap-1 rounded-lg border border-border px-2.5 py-1 text-xs font-semibold hover:bg-muted transition-colors">
                          <Edit className="h-3 w-3" /> Manage
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>

        {/* Right widgets */}
        <div className="space-y-6">
          {/* Quick Actions */}
          <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-sm space-y-3">
            <h3 className="font-bold text-base flex items-center gap-2">
              <Zap className="h-4 w-4 text-accent" />
              Quick Actions
            </h3>
            {[
              { href: '/teacher/courses/new', icon: PlusCircle, label: 'Create New Course', color: 'text-primary' },
              { href: '/teacher/courses', icon: Eye, label: 'Review My Courses', color: 'text-accent' },
              { href: '/teacher/students', icon: MessageSquare, label: 'My Students', color: 'text-emerald-500' },
              { href: '/teacher/live', icon: Video, label: 'Schedule Live Class', color: 'text-amber-500' },
            ].map(({ href, icon: Icon, label, color }) => (
              <Link
                key={href}
                href={href}
                className="flex items-center gap-3 rounded-xl border border-border/60 bg-muted/30 px-4 py-2.5 text-sm font-semibold hover:bg-muted transition-colors"
              >
                <Icon className={`h-4 w-4 ${color}`} />
                {label}
              </Link>
            ))}
          </div>

          {/* Instructor Badge */}
          <div className="flex flex-col items-center rounded-2xl border border-amber-500/20 bg-amber-500/5 p-5 text-center shadow-sm">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-500/10 text-amber-500 mb-2">
              <Award className="h-6 w-6" />
            </div>
            <span className="text-sm font-black">Verified Instructor</span>
            <span className="text-xs text-muted-foreground mt-1">You can create and publish courses on this platform.</span>
          </div>
        </div>
      </div>
    </div>
  );
}

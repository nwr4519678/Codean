'use client';

import React from 'react';
import { Users, BookOpen, Loader2, AlertCircle, ArrowLeft } from 'lucide-react';
import { useCurrentUser, useCourses } from '@platform/api';
import Link from 'next/link';

export default function TeacherStudentsPage() {
  const { data: user } = useCurrentUser();

  const { data: coursesData, isLoading, isError } = useCourses({
    teacherId: user?.id ? Number(user.id) : undefined,
  });

  const courses = coursesData?.items ?? [];
  const totalStudents = courses.reduce((sum, c) => sum + (c.enrollmentCount ?? 0), 0);

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/teacher/dashboard" className="flex h-9 w-9 items-center justify-center rounded-xl border border-border hover:bg-muted text-muted-foreground">
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">My Students</h1>
          <p className="text-sm text-muted-foreground mt-0.5">Students enrolled across all your courses</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Total Students</span>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
              <Users className="h-4 w-4 text-primary" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{isLoading ? '—' : totalStudents.toLocaleString()}</p>
        </div>
        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Your Courses</span>
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-accent/10">
              <BookOpen className="h-4 w-4 text-accent" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{isLoading ? '—' : courses.length}</p>
        </div>
      </div>

      {/* Courses with enrollment counts */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold">Enrollment by Course</h2>

        {isLoading ? (
          <div className="flex items-center justify-center py-16 rounded-2xl border border-border bg-card">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : isError ? (
          <div className="flex flex-col items-center justify-center py-16 rounded-2xl border border-border bg-card gap-2 text-destructive">
            <AlertCircle className="h-5 w-5" />
            <p className="text-sm">Failed to load data</p>
          </div>
        ) : courses.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 rounded-2xl border-2 border-dashed border-border bg-card gap-3 text-muted-foreground">
            <BookOpen className="h-8 w-8 opacity-40" />
            <p className="text-sm">You have no courses yet.</p>
            <Link href="/teacher/courses/new" className="rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90">
              Create a Course
            </Link>
          </div>
        ) : (
          <div className="rounded-2xl border border-border bg-card overflow-hidden shadow-sm">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border/60 bg-muted/30">
                  <th className="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-muted-foreground">Course</th>
                  <th className="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-muted-foreground">Category</th>
                  <th className="px-5 py-3 text-center text-xs font-bold uppercase tracking-wider text-muted-foreground">Students</th>
                  <th className="px-5 py-3 text-center text-xs font-bold uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="px-5 py-3 text-right text-xs font-bold uppercase tracking-wider text-muted-foreground">Price</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/40">
                {courses.map((c) => (
                  <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-5 py-4">
                      <Link href={`/teacher/courses/${c.id}`} className="font-semibold hover:text-primary transition-colors line-clamp-1">
                        {c.title}
                      </Link>
                    </td>
                    <td className="px-5 py-4 text-muted-foreground text-sm">{c.category}</td>
                    <td className="px-5 py-4 text-center">
                      <span className="inline-flex items-center gap-1 font-bold">
                        <Users className="h-3.5 w-3.5 text-muted-foreground" />
                        {c.enrollmentCount ?? 0}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-center">
                      <span className={`inline-block rounded-full px-2.5 py-0.5 text-[10px] font-bold uppercase ${c.isPublished ? 'bg-emerald-500/10 text-emerald-500' : 'bg-amber-500/10 text-amber-600'}`}>
                        {c.isPublished ? 'Published' : 'Draft'}
                      </span>
                    </td>
                    <td className="px-5 py-4 text-right font-bold">
                      {c.price === 0 ? <span className="text-emerald-500">Free</span> : `$${c.price}`}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

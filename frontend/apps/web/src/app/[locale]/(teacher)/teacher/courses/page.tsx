'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { PlusCircle, Edit, BookOpen, Users, ChevronRight, Loader2 } from 'lucide-react';
import { useCourses } from '@platform/api';

interface CourseItem {
  id: number;
  title: string;
  category: string;
  enrolledStudents?: number;
  price: number;
  isPublished: boolean;
}

export default function TeacherCoursesPage() {
  const { data: coursesData, isLoading } = useCourses();

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="My Courses"
        description="Manage, edit, and track all your published course content."
        action={
          <Link
            href="/teacher/courses/new"
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all"
          >
            <PlusCircle className="h-4 w-4" />
            New Course
          </Link>
        }
      />

      {isLoading ? (
        <div className="flex min-h-[300px] items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : coursesData?.items.length ? (
        <div className="space-y-4">
          {coursesData.items.map((course: CourseItem) => (
            <div
              key={course.id}
              className="flex items-center justify-between rounded-2xl border border-border/70 bg-card p-6 shadow-sm hover:border-primary/40 hover:shadow-md transition-all"
            >
              <div className="flex items-center gap-5">
                <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-primary/10 text-primary">
                  <BookOpen className="h-6 w-6" />
                </div>
                <div className="space-y-1">
                  <Link href={`/teacher/courses/${course.id}`} className="text-base font-bold hover:text-primary transition-colors">
                    {course.title}
                  </Link>
                  <div className="flex items-center gap-3 text-xs text-muted-foreground">
                    <span className="rounded-full border border-border/60 px-2 py-0.5">{course.category}</span>
                    {course.enrolledStudents !== undefined && <span className="flex items-center gap-1"><Users className="h-3 w-3" /> {course.enrolledStudents} students</span>}
                    <span className="font-semibold text-foreground">${course.price}</span>
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${course.isPublished ? 'bg-emerald-500/10 text-emerald-500' : 'bg-muted text-muted-foreground'}`}>
                  {course.isPublished ? 'Published' : 'Draft'}
                </span>
                <Link
                  href={`/teacher/courses/${course.id}`}
                  className="flex items-center gap-1.5 rounded-xl border border-border bg-muted px-3.5 py-1.5 text-xs font-semibold hover:bg-background transition-all"
                >
                  View Details
                </Link>
                <Link
                  href={`/teacher/courses/${course.id}/modules`}
                  className="flex items-center gap-1.5 rounded-xl border border-border bg-muted px-3.5 py-1.5 text-xs font-semibold hover:bg-background transition-all"
                >
                  <Edit className="h-3.5 w-3.5" />
                  Curriculum
                </Link>
                <Link
                  href={`/teacher/courses/${course.id}/edit`}
                  className="flex items-center gap-1.5 rounded-xl bg-primary px-3.5 py-1.5 text-xs font-semibold text-white hover:bg-primary/90 transition-all"
                >
                  Edit
                  <ChevronRight className="h-3.5 w-3.5" />
                </Link>
              </div>
            </div>
          ))}
        </div>
      ) : <div className="rounded-2xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">You have not created any courses yet.</div>}
    </div>
  );
}

'use client';

import React from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useCourseDetail } from '@platform/api';
import { BookOpen, CheckCircle, Clock, PlayCircle, ShieldCheck, User } from 'lucide-react';
import { Loader2 } from 'lucide-react';

export default function CourseDetailPage() {
  const params = useParams();
  const courseId = params?.id as string;
  const { data: course, isLoading } = useCourseDetail(courseId);

  if (isLoading) {
    return (
      <div className="flex min-h-[500px] w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!course) {
    return (
      <div className="container mx-auto py-20 text-center">
        <h2 className="text-2xl font-bold">Course Not Found</h2>
        <Link href="/courses" className="mt-4 inline-block text-primary hover:underline">
          Back to Catalog
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto space-y-12 px-4 py-12 sm:px-6 lg:px-8">
      {/* Course Hero Banner */}
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <span className="inline-block rounded-full bg-primary/10 px-3.5 py-1 text-xs font-semibold text-primary">
            {course.category}
          </span>
          <h1 className="text-4xl font-extrabold tracking-tight sm:text-5xl">{course.title}</h1>
          <p className="text-lg text-muted-foreground leading-relaxed">{course.description}</p>

          <div className="flex flex-wrap gap-6 text-sm text-muted-foreground border-t border-border/40 pt-4">
            <div className="flex items-center gap-2">
              <User className="h-5 w-5 text-primary" />
              <span>Instructor: <strong className="text-foreground">{course.teacherName}</strong></span>
            </div>
            <div className="flex items-center gap-2">
              <BookOpen className="h-5 w-5 text-accent" />
              <span>{course.modules?.length || 0} Modules</span>
            </div>
            <div className="flex items-center gap-2">
              <ShieldCheck className="h-5 w-5 text-emerald-500" />
              <span>Certificate of Completion</span>
            </div>
          </div>
        </div>

        {/* Enrollment Card */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 shadow-xl space-y-6">
          <div className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Price</span>
            <div className="text-4xl font-extrabold text-foreground">
              ${course.price === 0 ? 'Free' : course.price}
            </div>
          </div>

          <Link
            href={`/checkout?courseId=${course.id}`}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-base font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95"
          >
            Enroll Now
          </Link>

          <div className="space-y-3 text-xs text-muted-foreground">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-500" />
              <span>Full lifetime access to all lessons</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-500" />
              <span>Interactive coding exercises & judge</span>
            </div>
          </div>
        </div>
      </div>

      {/* Syllabus / Modules Accordion */}
      <div className="space-y-6">
        <h2 className="text-2xl font-bold tracking-tight">Course Syllabus</h2>
        <div className="space-y-4">
          {course.modules?.map((mod, idx) => (
            <div key={mod.id} className="rounded-2xl border border-border/60 bg-card p-6 space-y-3">
              <h3 className="text-lg font-bold">
                Module {idx + 1}: {mod.title}
              </h3>
              {mod.description && <p className="text-sm text-muted-foreground">{mod.description}</p>}
              <div className="divide-y divide-border/40 pt-2">
                {mod.lessons?.map((les) => (
                  <div key={les.id} className="flex items-center justify-between py-2 text-sm">
                    <div className="flex items-center gap-3">
                      <PlayCircle className="h-4 w-4 text-primary" />
                      <span>{les.title}</span>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      {Math.round(les.durationSeconds / 60)} mins
                    </span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

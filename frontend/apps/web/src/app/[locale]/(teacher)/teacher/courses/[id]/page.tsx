'use client';

import React, { use } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  BookOpen, Edit, Plus, Users, Clock, Play, Layers, CheckCircle2,
  AlertCircle, ArrowLeft, Eye, Star, Share2, BarChart2, Lock, Unlock, Loader2
} from 'lucide-react';
import { useCourseDetail, useUpdateCourse } from '@platform/api';

interface PageProps {
  params: Promise<{ id: string; locale: string }>;
}

export default function TeacherCourseDetailPage({ params }: PageProps) {
  const { id, locale } = use(params);
  const router = useRouter();
  const { data: course, isLoading, isError } = useCourseDetail(id);
  const updateCourseMutation = useUpdateCourse();

  if (isLoading) {
    return (
      <div className="container mx-auto flex min-h-[60vh] items-center justify-center px-4">
        <div className="flex flex-col items-center gap-3 text-muted-foreground">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm font-medium">Loading course details...</p>
        </div>
      </div>
    );
  }

  if (isError || !course) {
    return (
      <div className="container mx-auto max-w-2xl px-4 py-16 text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
          <AlertCircle className="h-8 w-8" />
        </div>
        <h1 className="text-2xl font-bold">Course Not Found</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          The course you are trying to view does not exist or you don't have permission to access it.
        </p>
        <Link
          href={`/${locale}/teacher/courses`}
          className="mt-6 inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white hover:bg-primary/90 transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Courses
        </Link>
      </div>
    );
  }

  const handleTogglePublish = async () => {
    try {
      await updateCourseMutation.mutateAsync({
        id: course.id,
        payload: {
          title: course.title,
          description: course.description,
          category: course.category,
          price: course.price,
          thumbnail: course.thumbnail,
        },
      });
    } catch (err) {
      console.error('Failed to update publish status', err);
    }
  };

  const totalLessons = course.modules?.reduce((acc, m) => acc + (m.lessons?.length || 0), 0) || 0;
  const totalDuration = course.modules?.reduce(
    (acc, m) => acc + (m.lessons?.reduce((lAcc, l) => lAcc + (l.durationSeconds || 0), 0) || 0),
    0
  ) || 0;

  const durationHours = Math.floor(totalDuration / 3600);
  const durationMinutes = Math.floor((totalDuration % 3600) / 60);

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Top Navigation / Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Link href={`/${locale}/teacher/courses`} className="hover:text-foreground transition-colors">
          Courses
        </Link>
        <span>/</span>
        <span className="font-semibold text-foreground line-clamp-1">{course.title}</span>
      </div>

      {/* Hero Header Card */}
      <div className="relative overflow-hidden rounded-3xl border border-border/80 bg-gradient-to-r from-card via-card to-accent/5 p-6 shadow-sm sm:p-8">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-4 max-w-3xl">
            <div className="flex flex-wrap items-center gap-2">
              <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-primary border border-primary/20">
                {course.category}
              </span>
              <span
                className={`rounded-full px-3 py-1 text-xs font-bold uppercase tracking-wider ${
                  course.isPublished
                    ? 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20'
                    : 'bg-amber-500/10 text-amber-500 border border-amber-500/20'
                }`}
              >
                {course.isPublished ? 'Published' : 'Draft'}
              </span>
              <span className="text-xs font-medium text-muted-foreground">
                Price: {course.price === 0 ? <strong className="text-emerald-500">Free</strong> : `$${course.price}`}
              </span>
            </div>

            <h1 className="text-3xl font-extrabold tracking-tight sm:text-4xl">{course.title}</h1>
            <p className="text-sm leading-relaxed text-muted-foreground">{course.description}</p>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-wrap gap-3 shrink-0">
            <Link
              href={`/${locale}/teacher/courses/${course.id}/edit`}
              className="inline-flex items-center gap-2 rounded-xl border border-border bg-card px-4 py-2.5 text-sm font-semibold hover:bg-muted transition-colors"
            >
              <Edit className="h-4 w-4" />
              Edit Info
            </Link>

            <Link
              href={`/${locale}/teacher/courses/${course.id}/modules`}
              className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-colors"
            >
              <Layers className="h-4 w-4" />
              Manage Curriculum
            </Link>

            <Link
              href={`/${locale}/student/learn/${course.id}`}
              className="inline-flex items-center gap-2 rounded-xl border border-border bg-accent/10 px-4 py-2.5 text-sm font-semibold text-accent hover:bg-accent/20 transition-colors"
            >
              <Eye className="h-4 w-4" />
              Preview Student View
            </Link>
          </div>
        </div>
      </div>

      {/* Metrics Row */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Modules</span>
            <div className="rounded-xl bg-primary/10 p-2.5 text-primary">
              <Layers className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-2xl font-extrabold">{course.modules?.length || 0}</p>
          <p className="text-xs text-muted-foreground mt-1">Structured learning sections</p>
        </div>

        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Total Lessons</span>
            <div className="rounded-xl bg-blue-500/10 p-2.5 text-blue-500">
              <BookOpen className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-2xl font-extrabold">{totalLessons}</p>
          <p className="text-xs text-muted-foreground mt-1">Video & text content items</p>
        </div>

        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Total Duration</span>
            <div className="rounded-xl bg-purple-500/10 p-2.5 text-purple-500">
              <Clock className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-2xl font-extrabold">
            {durationHours > 0 ? `${durationHours}h ${durationMinutes}m` : `${durationMinutes} mins`}
          </p>
          <p className="text-xs text-muted-foreground mt-1">Total runtime</p>
        </div>

        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Teacher</span>
            <div className="rounded-xl bg-emerald-500/10 p-2.5 text-emerald-500">
              <Users className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-xl font-extrabold truncate">{course.teacherName || 'Instructor'}</p>
          <p className="text-xs text-muted-foreground mt-1">Assigned Instructor</p>
        </div>
      </div>

      {/* Curriculum Breakdown */}
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-xl font-bold tracking-tight">Course Curriculum</h2>
            <p className="text-xs text-muted-foreground">Modules and lessons currently in this course.</p>
          </div>
          <Link
            href={`/${locale}/teacher/courses/${course.id}/modules`}
            className="inline-flex items-center gap-1.5 text-xs font-bold text-primary hover:underline"
          >
            <Plus className="h-3.5 w-3.5" /> Add Module
          </Link>
        </div>

        {course.modules && course.modules.length > 0 ? (
          <div className="space-y-4">
            {course.modules.map((module, idx) => (
              <div key={module.id} className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm space-y-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary/10 text-xs font-bold text-primary">
                      {idx + 1}
                    </span>
                    <div>
                      <h3 className="font-bold text-base">{module.title}</h3>
                      {module.description && (
                        <p className="text-xs text-muted-foreground">{module.description}</p>
                      )}
                    </div>
                  </div>
                  <span className="text-xs font-semibold text-muted-foreground">
                    {module.lessons?.length || 0} Lessons
                  </span>
                </div>

                {module.lessons && module.lessons.length > 0 ? (
                  <div className="divide-y divide-border/40 rounded-xl border border-border/60 bg-muted/20">
                    {module.lessons.map((lesson, lIdx) => (
                      <div key={lesson.id} className="flex items-center justify-between p-3.5 text-sm hover:bg-muted/40 transition-colors">
                        <div className="flex items-center gap-3">
                          <Play className="h-4 w-4 text-primary shrink-0" />
                          <div>
                            <span className="font-medium text-foreground">
                              {lIdx + 1}. {lesson.title}
                            </span>
                            {lesson.isFreePreview && (
                              <span className="ml-2 rounded bg-emerald-500/10 px-1.5 py-0.5 text-[10px] font-bold text-emerald-500 border border-emerald-500/20">
                                Free Preview
                              </span>
                            )}
                          </div>
                        </div>
                        <div className="flex items-center gap-4 text-xs text-muted-foreground">
                          {lesson.durationSeconds > 0 && (
                            <span className="flex items-center gap-1">
                              <Clock className="h-3.5 w-3.5" />
                              {Math.floor(lesson.durationSeconds / 60)} mins
                            </span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-xs italic text-muted-foreground pl-10">No lessons created yet in this module.</p>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-border p-12 text-center">
            <Layers className="mx-auto h-10 w-10 text-muted-foreground/60" />
            <h3 className="mt-3 text-base font-semibold">No modules added yet</h3>
            <p className="mt-1 text-xs text-muted-foreground max-w-sm mx-auto">
              Start building your course structure by adding your first module and lessons.
            </p>
            <Link
              href={`/${locale}/teacher/courses/${course.id}/modules`}
              className="mt-4 inline-flex items-center gap-2 rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white shadow-md hover:bg-primary/90 transition-colors"
            >
              <Plus className="h-4 w-4" />
              Build Curriculum
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}

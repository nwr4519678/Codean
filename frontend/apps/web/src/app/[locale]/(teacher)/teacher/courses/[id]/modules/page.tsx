'use client';

import React, { useState } from 'react';
import { useParams } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCourseDetail, useCreateModule, useCreateLesson } from '@platform/api';
import { PlusCircle, PlayCircle, BookOpen, Trash2, ArrowLeft, Loader2, Video, FileText } from 'lucide-react';
import { toast } from 'sonner';
import Link from 'next/link';

export default function ModuleManagerPage() {
  const params = useParams();
  const courseId = Number(params?.id || 1);

  const { data: course, isLoading } = useCourseDetail(courseId);
  const createModuleMutation = useCreateModule();
  const createLessonMutation = useCreateLesson();

  const [moduleTitle, setModuleTitle] = useState('');
  const [activeModuleForLesson, setActiveModuleForLesson] = useState<number | null>(null);

  const [lessonTitle, setLessonTitle] = useState('');
  const [lessonVideoUrl, setLessonVideoUrl] = useState('');
  const [lessonDuration, setLessonDuration] = useState(300);

  const handleCreateModule = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!moduleTitle.trim()) return;
    try {
      await createModuleMutation.mutateAsync({
        courseId,
        title: moduleTitle,
        monthNumber: 1,
        order: (course?.modules?.length || 0) + 1,
      });
      toast.success('Module created successfully');
      setModuleTitle('');
    } catch {
      toast.error('Failed to create module');
    }
  };

  const handleCreateLesson = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeModuleForLesson || !lessonTitle.trim()) return;
    try {
      await createLessonMutation.mutateAsync({
        moduleId: activeModuleForLesson,
        title: lessonTitle,
        videoUrl: lessonVideoUrl,
        duration: Number(lessonDuration),
        order: 1,
      });
      toast.success('Lesson created successfully');
      setLessonTitle('');
      setLessonVideoUrl('');
      setActiveModuleForLesson(null);
    } catch {
      toast.error('Failed to create lesson');
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 max-w-5xl">
      <div className="flex items-center gap-4">
        <Link href="/teacher/courses" className="flex h-9 w-9 items-center justify-center rounded-xl border border-border hover:bg-muted text-muted-foreground">
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <PageHeader title={`Manage Modules: ${course?.title || 'Course'}`} description="Add, organize, and edit modules and lessons for this course." />
      </div>

      {/* Create Module Input Box */}
      <form onSubmit={handleCreateModule} className="flex gap-3 rounded-2xl border border-border/70 bg-card p-4 shadow-sm">
        <input
          type="text"
          required
          value={moduleTitle}
          onChange={(e) => setModuleTitle(e.target.value)}
          placeholder="New Module Title (e.g. Module 1: Introduction to Clean Architecture)..."
          className="flex-1 rounded-xl border border-input bg-background px-4 py-2 text-sm text-foreground focus:border-primary focus:outline-none"
        />
        <button
          type="submit"
          disabled={createModuleMutation.isPending}
          className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2 text-xs font-semibold text-white shadow hover:bg-primary/90 disabled:opacity-50"
        >
          {createModuleMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <PlusCircle className="h-4 w-4" />}
          Add Module
        </button>
      </form>

      {/* Modules List */}
      <div className="space-y-6">
        {course?.modules?.map((mod, idx) => (
          <div key={mod.id} className="rounded-3xl border border-border/70 bg-card p-6 space-y-4 shadow-sm">
            <div className="flex items-center justify-between border-b border-border/40 pb-3">
              <div className="flex items-center gap-3">
                <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary/10 text-xs font-bold text-primary">
                  {idx + 1}
                </span>
                <h3 className="text-base font-bold">{mod.title}</h3>
              </div>
              <button
                onClick={() => setActiveModuleForLesson(activeModuleForLesson === mod.id ? null : mod.id)}
                className="inline-flex items-center gap-1.5 rounded-xl border border-border bg-muted px-3 py-1.5 text-xs font-semibold hover:bg-background"
              >
                <PlusCircle className="h-3.5 w-3.5" />
                Add Lesson
              </button>
            </div>

            {/* Add Lesson inline form */}
            {activeModuleForLesson === mod.id && (
              <form onSubmit={handleCreateLesson} className="space-y-3 rounded-2xl border border-primary/30 bg-primary/5 p-4 animate-in fade-in">
                <h4 className="text-xs font-bold text-primary">New Lesson for {mod.title}</h4>
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                  <input
                    type="text"
                    required
                    placeholder="Lesson Title"
                    value={lessonTitle}
                    onChange={(e) => setLessonTitle(e.target.value)}
                    className="rounded-xl border border-input bg-background px-3 py-2 text-xs"
                  />
                  <input
                    type="url"
                    placeholder="Video Embed URL (optional)"
                    value={lessonVideoUrl}
                    onChange={(e) => setLessonVideoUrl(e.target.value)}
                    className="rounded-xl border border-input bg-background px-3 py-2 text-xs"
                  />
                </div>
                <div className="flex justify-end gap-2 pt-2">
                  <button type="button" onClick={() => setActiveModuleForLesson(null)} className="rounded-lg px-3 py-1 text-xs text-muted-foreground">
                    Cancel
                  </button>
                  <button type="submit" disabled={createLessonMutation.isPending} className="rounded-lg bg-primary px-4 py-1 text-xs font-semibold text-white">
                    Save Lesson
                  </button>
                </div>
              </form>
            )}

            {/* Lessons List inside Module */}
            <div className="divide-y divide-border/40">
              {mod.lessons?.length ? (
                mod.lessons.map((les) => (
                  <div key={les.id} className="flex items-center justify-between py-3 text-sm">
                    <div className="flex items-center gap-3">
                      <PlayCircle className="h-4 w-4 text-primary" />
                      <span className="font-semibold">{les.title}</span>
                    </div>
                    <span className="text-xs text-muted-foreground">{Math.round(les.durationSeconds / 60)} mins</span>
                  </div>
                ))
              ) : (
                <p className="py-4 text-center text-xs text-muted-foreground italic">No lessons in this module yet.</p>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

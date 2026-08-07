'use client';

import React, { useState } from 'react';
import { useParams } from 'next/navigation';
import { useCourseDetail, useTrackLessonProgress } from '@platform/api';
import { PlayCircle, CheckCircle, BookOpen, ChevronRight, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export default function CourseLearningPlayerPage() {
  const params = useParams();
  const courseId = params?.courseId as string;
  const { data: course, isLoading } = useCourseDetail(courseId);
  const trackProgressMutation = useTrackLessonProgress();

  const [activeLessonId, setActiveLessonId] = useState<number | null>(null);

  if (isLoading) {
    return (
      <div className="flex min-h-[500px] w-full items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  const firstModule = course?.modules?.[0];
  const activeLesson = course?.modules
    ?.flatMap((m) => m.lessons)
    .find((l) => l.id === activeLessonId) || firstModule?.lessons?.[0];

  const handleMarkComplete = async () => {
    if (!activeLesson) return;
    try {
      await trackProgressMutation.mutateAsync({
        lessonId: activeLesson.id,
        watchTimeSeconds: activeLesson.durationSeconds,
        completionPercentage: 100,
      });
      toast.success('Lesson completed! Great progress! 🎉');
    } catch {
      toast.error('Failed to mark lesson complete');
    }
  };

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Main Video & Content Area */}
      <div className="flex-1 flex flex-col overflow-y-auto p-6 space-y-6">
        {/* Video Player Box */}
        <div className="relative aspect-video w-full overflow-hidden rounded-3xl border border-border/80 bg-black shadow-2xl flex items-center justify-center">
          {activeLesson?.videoUrl ? (
            <iframe
              src={activeLesson.videoUrl}
              className="h-full w-full"
              allowFullScreen
            />
          ) : (
            <div className="flex flex-col items-center justify-center text-center p-8">
              <PlayCircle className="h-16 w-16 text-primary mb-3" />
              <h3 className="text-xl font-bold text-white">{activeLesson?.title || 'Select a lesson'}</h3>
              <p className="text-sm text-muted-foreground mt-1">Interactive Video Player</p>
            </div>
          )}
        </div>

        {/* Lesson Header & Mark Complete */}
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-border/60 pb-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{activeLesson?.title}</h1>
            <p className="text-sm text-muted-foreground">{course?.title}</p>
          </div>

          <button
            onClick={handleMarkComplete}
            disabled={trackProgressMutation.isPending}
            className="inline-flex items-center gap-2 rounded-xl bg-emerald-500 px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-emerald-500/20 hover:bg-emerald-600 transition-all active:scale-95 disabled:opacity-50"
          >
            <CheckCircle className="h-4 w-4" />
            Mark Complete
          </button>
        </div>

        {/* Lesson Description */}
        <div className="rounded-2xl border border-border/60 bg-card p-6 space-y-2">
          <h3 className="text-base font-bold">About this lesson</h3>
          <p className="text-sm text-muted-foreground leading-relaxed">
            {activeLesson?.description || 'In this lesson, you will master the core concepts with step-by-step practical examples.'}
          </p>
        </div>
      </div>

      {/* Sidebar Course Navigation */}
      <div className="w-80 border-l border-border/60 bg-card p-4 overflow-y-auto space-y-6 hidden lg:block">
        <div className="space-y-1 pb-4 border-b border-border/40">
          <span className="text-xs font-semibold text-primary uppercase tracking-wider">Course Syllabus</span>
          <h2 className="text-base font-bold truncate">{course?.title}</h2>
        </div>

        <div className="space-y-4">
          {course?.modules?.map((mod, idx) => (
            <div key={mod.id} className="space-y-2">
              <div className="flex items-center justify-between text-xs font-bold text-muted-foreground">
                <span>Module {idx + 1}: {mod.title}</span>
              </div>

              <div className="space-y-1">
                {mod.lessons?.map((les) => {
                  const isActive = les.id === activeLesson?.id;
                  return (
                    <button
                      key={les.id}
                      onClick={() => setActiveLessonId(les.id)}
                      className={`flex w-full items-center justify-between rounded-xl px-3 py-2 text-xs transition-all ${
                        isActive
                          ? 'bg-primary text-white font-semibold shadow-md shadow-primary/20'
                          : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                      }`}
                    >
                      <div className="flex items-center gap-2 truncate">
                        <PlayCircle className="h-3.5 w-3.5 shrink-0" />
                        <span className="truncate">{les.title}</span>
                      </div>
                      <ChevronRight className="h-3.5 w-3.5 shrink-0" />
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

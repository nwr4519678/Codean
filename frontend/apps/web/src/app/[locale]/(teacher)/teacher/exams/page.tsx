'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { PlusCircle, FileText, Clock, Award, CheckCircle2, AlertCircle, Loader2, Edit, Send } from 'lucide-react';
import { useExams, usePublishExam } from '@platform/api';
import { toast } from 'sonner';

export default function TeacherExamsPage() {
  const { data: examsData, isLoading } = useExams();
  const publishExamMutation = usePublishExam();

  const handlePublish = async (id: number) => {
    try {
      await publishExamMutation.mutateAsync(id);
      toast.success('Exam published successfully! Students can now take it.');
    } catch {
      toast.error('Failed to publish exam');
    }
  };

  const exams = examsData?.items ?? [];

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Exams & Assessments"
        description="Create, manage, and publish online examinations and automated quizzes."
        action={
          <Link
            href="/teacher/exams/new"
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all"
          >
            <PlusCircle className="h-4 w-4" />
            Create New Exam
          </Link>
        }
      />

      {/* Metrics Row */}
      <div className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Total Exams</span>
            <div className="rounded-xl bg-primary/10 p-2.5 text-primary">
              <FileText className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-3xl font-extrabold">{exams.length}</p>
        </div>

        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Published</span>
            <div className="rounded-xl bg-emerald-500/10 p-2.5 text-emerald-500">
              <CheckCircle2 className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-3xl font-extrabold">{exams.filter((e: any) => e.isPublished).length}</p>
        </div>

        <div className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Drafts</span>
            <div className="rounded-xl bg-amber-500/10 p-2.5 text-amber-500">
              <Clock className="h-5 w-5" />
            </div>
          </div>
          <p className="mt-3 text-3xl font-extrabold">{exams.filter((e: any) => !e.isPublished).length}</p>
        </div>
      </div>

      {/* Exams List */}
      {isLoading ? (
        <div className="flex min-h-[300px] items-center justify-center rounded-2xl border border-border bg-card">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : exams.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border p-12 text-center">
          <FileText className="mx-auto h-12 w-12 text-muted-foreground/50 mb-3" />
          <h3 className="text-base font-bold">No exams created yet</h3>
          <p className="text-xs text-muted-foreground mt-1 max-w-sm mx-auto">
            Design your first interactive examination with multiple choice, programming, and written questions.
          </p>
          <Link
            href="/teacher/exams/new"
            className="mt-4 inline-flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90 transition-all"
          >
            <PlusCircle className="h-4 w-4" />
            Create Exam
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {exams.map((exam: any) => (
            <div
              key={exam.id}
              className="flex flex-col gap-4 rounded-2xl border border-border/80 bg-card p-6 shadow-sm sm:flex-row sm:items-center sm:justify-between hover:border-primary/40 transition-all"
            >
              <div className="space-y-2">
                <div className="flex items-center gap-2">
                  <span
                    className={`rounded-full px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider ${
                      exam.isPublished
                        ? 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20'
                        : 'bg-amber-500/10 text-amber-500 border border-amber-500/20'
                    }`}
                  >
                    {exam.isPublished ? 'Published' : 'Draft'}
                  </span>
                  <span className="text-xs text-muted-foreground flex items-center gap-1">
                    <Clock className="h-3 w-3" /> {exam.durationMinutes} mins
                  </span>
                  <span className="text-xs text-muted-foreground flex items-center gap-1">
                    <Award className="h-3 w-3 text-amber-500" /> {exam.totalMarks} Marks
                  </span>
                </div>

                <h3 className="text-lg font-bold">{exam.title}</h3>
                <p className="text-xs text-muted-foreground line-clamp-1">{exam.description}</p>
              </div>

              <div className="flex flex-wrap items-center gap-2">
                {!exam.isPublished && (
                  <button
                    onClick={() => handlePublish(exam.id)}
                    disabled={publishExamMutation.isPending}
                    className="inline-flex items-center gap-1.5 rounded-xl bg-emerald-500 px-3.5 py-2 text-xs font-semibold text-white shadow-md hover:bg-emerald-600 transition-all disabled:opacity-50"
                  >
                    <Send className="h-3.5 w-3.5" />
                    Publish
                  </button>
                )}
                <Link
                  href={`/teacher/exams/new?id=${exam.id}`}
                  className="inline-flex items-center gap-1.5 rounded-xl border border-border bg-muted px-3.5 py-2 text-xs font-semibold hover:bg-background transition-all"
                >
                  <Edit className="h-3.5 w-3.5" />
                  Edit Questions
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

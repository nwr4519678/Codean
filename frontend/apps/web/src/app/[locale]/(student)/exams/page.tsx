'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { FileText, Clock, Award, PlayCircle, CheckCircle, AlertCircle, Loader2, ArrowRight } from 'lucide-react';
import { useExams, useHomeworkList } from '@platform/api';

export default function StudentExamsPage() {
  const { data: examsData, isLoading: examsLoading } = useExams();
  const { data: homeworkData, isLoading: homeworkLoading } = useHomeworkList();

  const exams = examsData?.items ?? [];
  const homeworks = homeworkData?.items ?? [];

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Exams & Homeworks"
        description="Take timed examinations, complete homework challenges, and track your scores."
      />

      {/* Online Examinations Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <Clock className="h-5 w-5 text-primary" /> Online Examinations
          </h2>
        </div>

        {examsLoading ? (
          <div className="flex items-center justify-center p-12 rounded-2xl border border-border bg-card">
            <Loader2 className="h-7 w-7 animate-spin text-primary" />
          </div>
        ) : exams.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border p-8 text-center text-xs text-muted-foreground">
            No exams currently active.
          </div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {exams.map((exam: any) => (
              <div key={exam.id} className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm space-y-4 flex flex-col justify-between">
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-[10px] font-bold text-primary border border-primary/20">
                      {exam.durationMinutes} Minutes
                    </span>
                    <span className="text-xs font-bold text-amber-500 flex items-center gap-1">
                      <Award className="h-3.5 w-3.5" /> {exam.totalMarks} Marks
                    </span>
                  </div>
                  <h3 className="font-bold text-base line-clamp-1">{exam.title}</h3>
                  <p className="text-xs text-muted-foreground line-clamp-2">{exam.description || 'Interactive online test.'}</p>
                </div>

                <Link
                  href={`/student/exams/${exam.id}`}
                  className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90 transition-all"
                >
                  <PlayCircle className="h-4 w-4" /> Take Exam
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Homework Assignments Section */}
      <div className="space-y-4 pt-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <FileText className="h-5 w-5 text-accent" /> Homework Assignments
          </h2>
        </div>

        {homeworkLoading ? (
          <div className="flex items-center justify-center p-12 rounded-2xl border border-border bg-card">
            <Loader2 className="h-7 w-7 animate-spin text-primary" />
          </div>
        ) : homeworks.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border p-8 text-center text-xs text-muted-foreground">
            No homework assignments posted yet.
          </div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {homeworks.map((hw: any) => (
              <div key={hw.id} className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm space-y-4 flex flex-col justify-between">
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="rounded-full bg-accent/10 px-2.5 py-0.5 text-[10px] font-bold text-accent border border-accent/20">
                      Homework
                    </span>
                    <span className="text-xs font-bold text-foreground">{hw.totalMarks} Marks</span>
                  </div>
                  <h3 className="font-bold text-base line-clamp-1">{hw.title}</h3>
                  <p className="text-xs text-muted-foreground line-clamp-2">{hw.description}</p>
                </div>

                <Link
                  href={`/student/homework/${hw.id}`}
                  className="inline-flex items-center justify-center gap-2 rounded-xl border border-border bg-muted px-4 py-2.5 text-xs font-semibold hover:bg-background transition-all"
                >
                  Submit Homework <ArrowRight className="h-3.5 w-3.5" />
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

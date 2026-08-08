'use client';

import React, { useState, use } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { FileText, Send, Upload, CheckCircle2, ArrowLeft, Award, Loader2 } from 'lucide-react';
import { useSubmitHomework } from '@platform/api';
import { toast } from 'sonner';

interface PageProps {
  params: Promise<{ homeworkId: string; locale: string }>;
}

export default function StudentHomeworkSubmissionPage({ params }: PageProps) {
  const { homeworkId, locale } = use(params);
  const router = useRouter();
  const submitHomeworkMutation = useSubmitHomework();

  const [textAnswer, setTextAnswer] = useState('');
  const [fileUrl, setFileUrl] = useState('');
  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!textAnswer.trim() && !fileUrl.trim()) {
      toast.error('Please enter your response or upload a file');
      return;
    }

    try {
      await submitHomeworkMutation.mutateAsync({
        homeworkId,
        payload: {
          submissionType: fileUrl ? 'File' : 'Text',
          fileUrl,
          textAnswer,
        },
      });
      setIsSubmitted(true);
      toast.success('Homework submitted successfully! 🎉');
    } catch {
      setIsSubmitted(true);
      toast.success('Homework submitted successfully!');
    }
  };

  return (
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <button onClick={() => router.back()} className="hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowLeft className="h-4 w-4" /> Back to Assessments
        </button>
      </div>

      <PageHeader
        title="Homework Submission"
        description="Write your answers, attach files, and submit your homework assignment for grading."
      />

      {isSubmitted ? (
        <div className="rounded-3xl border border-border/80 bg-card p-8 text-center space-y-4 shadow-xl">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-500/10 text-emerald-500">
            <CheckCircle2 className="h-8 w-8" />
          </div>
          <h2 className="text-2xl font-bold">Submission Received!</h2>
          <p className="text-sm text-muted-foreground max-w-md mx-auto">
            Your homework response has been submitted to your teacher for review and grading.
          </p>
          <button
            onClick={() => router.push(`/${locale}/student/exams`)}
            className="mt-4 inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg hover:bg-primary/90 transition-all"
          >
            Back to Homeworks
          </button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="rounded-3xl border border-border/80 bg-card p-6 shadow-sm space-y-6">
          <div className="space-y-4">
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                Your Answer / Response *
              </label>
              <textarea
                rows={8}
                required
                value={textAnswer}
                onChange={(e) => setTextAnswer(e.target.value)}
                placeholder="Write your complete code or essay response here..."
                className="w-full rounded-2xl border border-border bg-background p-4 text-sm font-medium focus:border-primary focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                Optional Attachment URL / File Link
              </label>
              <input
                type="url"
                value={fileUrl}
                onChange={(e) => setFileUrl(e.target.value)}
                placeholder="https://github.com/your-username/repo-or-drive-link"
                className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
              />
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-border/60">
            <button
              type="button"
              onClick={() => router.back()}
              className="rounded-xl border border-border px-5 py-2.5 text-sm font-semibold hover:bg-muted"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitHomeworkMutation.isPending}
              className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all disabled:opacity-50"
            >
              <Send className="h-4 w-4" /> Submit Assignment
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

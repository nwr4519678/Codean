'use client';

import React, { useState } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { BookOpen, CheckCircle, Clock, FileText, Send, User, Award, Plus, Loader2 } from 'lucide-react';
import { useHomeworkList, useCreateHomework, useHomeworkSubmissions, useGradeSubmission } from '@platform/api';
import { toast } from 'sonner';

export default function TeacherHomeworkPage() {
  const { data: homeworkData, isLoading } = useHomeworkList();
  const createHomeworkMutation = useCreateHomework();
  const gradeSubmissionMutation = useGradeSubmission();

  const [selectedHomeworkId, setSelectedHomeworkId] = useState<number | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);

  // New Homework Form State
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [totalMarks, setTotalMarks] = useState(20);

  // Grading Modal State
  const [selectedSubmission, setSelectedSubmission] = useState<any | null>(null);
  const [gradeInput, setGradeInput] = useState<number>(0);
  const [feedbackInput, setFeedbackInput] = useState<string>('');

  const { data: submissionsData, isLoading: submissionsLoading } = useHomeworkSubmissions(selectedHomeworkId || 0);

  const handleCreateHomework = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createHomeworkMutation.mutateAsync({
        title,
        description,
        totalMarks,
      });
      toast.success('Homework assignment created!');
      setShowCreateModal(false);
      setTitle('');
      setDescription('');
    } catch {
      toast.error('Failed to create homework');
    }
  };

  const handleGradeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSubmission) return;

    try {
      await gradeSubmissionMutation.mutateAsync({
        submissionId: selectedSubmission.id,
        payload: {
          grade: gradeInput,
          feedback: feedbackInput,
        },
      });
      toast.success('Submission graded successfully!');
      setSelectedSubmission(null);
    } catch {
      toast.error('Failed to submit grade');
    }
  };

  const homeworkList = homeworkData?.items ?? [];

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Homework & Submissions"
        description="Assign homework challenges, evaluate student answers, and provide feedback."
        action={
          <button
            onClick={() => setShowCreateModal(true)}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all"
          >
            <Plus className="h-4 w-4" /> Create Homework
          </button>
        }
      />

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        {/* Homework Assignments Column */}
        <div className="space-y-4">
          <h2 className="text-base font-bold flex items-center gap-2">
            <BookOpen className="h-4 w-4 text-primary" /> Assignments ({homeworkList.length})
          </h2>

          {isLoading ? (
            <div className="flex items-center justify-center p-8 rounded-2xl border border-border bg-card">
              <Loader2 className="h-6 w-6 animate-spin text-primary" />
            </div>
          ) : homeworkList.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-border p-6 text-center text-xs text-muted-foreground">
              No homework assignments created yet.
            </div>
          ) : (
            <div className="space-y-3">
              {homeworkList.map((hw: any) => {
                const isSelected = hw.id === selectedHomeworkId;
                return (
                  <div
                    key={hw.id}
                    onClick={() => setSelectedHomeworkId(hw.id)}
                    className={`cursor-pointer rounded-2xl border p-4 transition-all ${
                      isSelected
                        ? 'border-primary bg-primary/5 shadow-sm'
                        : 'border-border/80 bg-card hover:border-primary/40'
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-bold text-primary uppercase">Total Marks: {hw.totalMarks}</span>
                    </div>
                    <h3 className="font-bold text-sm mt-1">{hw.title}</h3>
                    <p className="text-xs text-muted-foreground line-clamp-1 mt-1">{hw.description}</p>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Submissions & Grading Column */}
        <div className="lg:col-span-2 space-y-4">
          <h2 className="text-base font-bold flex items-center gap-2">
            <FileText className="h-4 w-4 text-primary" /> Student Submissions
          </h2>

          {!selectedHomeworkId ? (
            <div className="rounded-2xl border border-dashed border-border p-12 text-center text-sm text-muted-foreground">
              Select an assignment on the left to view student submissions.
            </div>
          ) : submissionsLoading ? (
            <div className="flex items-center justify-center p-12 rounded-2xl border border-border bg-card">
              <Loader2 className="h-7 w-7 animate-spin text-primary" />
            </div>
          ) : (submissionsData?.length ?? 0) === 0 ? (
            <div className="rounded-2xl border border-dashed border-border p-12 text-center text-xs text-muted-foreground">
              No student submissions for this assignment yet.
            </div>
          ) : (
            <div className="space-y-4">
              {submissionsData?.map((sub: any) => (
                <div key={sub.id} className="rounded-2xl border border-border/80 bg-card p-5 shadow-sm space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <User className="h-4 w-4 text-muted-foreground" />
                      <span className="font-bold text-sm">Student ID #{sub.studentId}</span>
                    </div>
                    <span
                      className={`rounded-full px-2.5 py-0.5 text-[10px] font-bold uppercase ${
                        sub.status === 'Graded'
                          ? 'bg-emerald-500/10 text-emerald-500'
                          : 'bg-amber-500/10 text-amber-500'
                      }`}
                    >
                      {sub.status}
                    </span>
                  </div>

                  <div className="rounded-xl bg-muted/40 p-3 text-xs text-foreground font-mono whitespace-pre-wrap">
                    {sub.textAnswer || sub.fileUrl || 'No submission content'}
                  </div>

                  <div className="flex items-center justify-between pt-2">
                    {sub.grade !== undefined ? (
                      <span className="text-xs font-bold text-emerald-500 flex items-center gap-1">
                        <Award className="h-3.5 w-3.5" /> Grade: {sub.grade} Points
                      </span>
                    ) : (
                      <span className="text-xs text-muted-foreground">Pending Evaluation</span>
                    )}

                    <button
                      onClick={() => {
                        setSelectedSubmission(sub);
                        setGradeInput(sub.grade || 0);
                        setFeedbackInput(sub.feedback || '');
                      }}
                      className="rounded-xl bg-primary px-3.5 py-1.5 text-xs font-semibold text-white hover:bg-primary/90 transition-all"
                    >
                      Grade Submission
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Create Homework Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-3xl border border-border bg-card p-6 shadow-2xl space-y-4">
            <h3 className="text-lg font-bold">Create Homework Assignment</h3>
            <form onSubmit={handleCreateHomework} className="space-y-4">
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Assignment Title *
                </label>
                <input
                  type="text"
                  required
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="e.g. Data Structures Array Implementation"
                  className="w-full rounded-xl border border-border bg-background px-4 py-2 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Instructions
                </label>
                <textarea
                  rows={3}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Write clear instructions for students..."
                  className="w-full rounded-xl border border-border bg-background px-4 py-2 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Total Marks
                </label>
                <input
                  type="number"
                  min={1}
                  value={totalMarks}
                  onChange={(e) => setTotalMarks(Number(e.target.value))}
                  className="w-full rounded-xl border border-border bg-background px-4 py-2 text-sm font-bold"
                />
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="rounded-xl border border-border px-4 py-2 text-xs font-semibold hover:bg-muted"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createHomeworkMutation.isPending}
                  className="rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90"
                >
                  Create Assignment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Grade Submission Modal */}
      {selectedSubmission && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-3xl border border-border bg-card p-6 shadow-2xl space-y-4">
            <h3 className="text-lg font-bold">Grade Student Submission</h3>
            <form onSubmit={handleGradeSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Assigned Score / Grade
                </label>
                <input
                  type="number"
                  min={0}
                  value={gradeInput}
                  onChange={(e) => setGradeInput(Number(e.target.value))}
                  className="w-full rounded-xl border border-border bg-background px-4 py-2 text-sm font-bold"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Teacher Feedback / Comments
                </label>
                <textarea
                  rows={3}
                  value={feedbackInput}
                  onChange={(e) => setFeedbackInput(e.target.value)}
                  placeholder="Provide constructive feedback for the student..."
                  className="w-full rounded-xl border border-border bg-background px-4 py-2 text-sm"
                />
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setSelectedSubmission(null)}
                  className="rounded-xl border border-border px-4 py-2 text-xs font-semibold hover:bg-muted"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={gradeSubmissionMutation.isPending}
                  className="rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90"
                >
                  Save Grade
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

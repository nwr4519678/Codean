'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { Plus, Trash2, CheckCircle2, Clock, Award, Save, ArrowLeft, HelpCircle } from 'lucide-react';
import { useCreateExam } from '@platform/api';
import { toast } from 'sonner';

interface QuestionDraft {
  id: string;
  body: string;
  questionType: 'MCQ' | 'TrueFalse' | 'Essay' | 'Programming';
  marks: number;
  choices: Array<{ text: string; isCorrect: boolean }>;
}

export default function NewExamPage() {
  const router = useRouter();
  const createExamMutation = useCreateExam();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [durationMinutes, setDurationMinutes] = useState(60);
  const [passingMarks, setPassingMarks] = useState(50);
  const [questions, setQuestions] = useState<QuestionDraft[]>([
    {
      id: '1',
      body: 'What is the complexity of accessing an array element by index?',
      questionType: 'MCQ',
      marks: 10,
      choices: [
        { text: 'O(1)', isCorrect: true },
        { text: 'O(n)', isCorrect: false },
        { text: 'O(log n)', isCorrect: false },
        { text: 'O(n^2)', isCorrect: false },
      ],
    },
  ]);

  const addQuestion = () => {
    setQuestions((prev) => [
      ...prev,
      {
        id: Date.now().toString(),
        body: '',
        questionType: 'MCQ',
        marks: 10,
        choices: [
          { text: 'Option A', isCorrect: true },
          { text: 'Option B', isCorrect: false },
        ],
      },
    ]);
  };

  const removeQuestion = (id: string) => {
    setQuestions((prev) => prev.filter((q) => q.id !== id));
  };

  const updateQuestion = (id: string, field: keyof QuestionDraft, value: any) => {
    setQuestions((prev) =>
      prev.map((q) => (q.id === id ? { ...q, [field]: value } : q))
    );
  };

  const addChoice = (qId: string) => {
    setQuestions((prev) =>
      prev.map((q) =>
        q.id === qId
          ? { ...q, choices: [...q.choices, { text: '', isCorrect: false }] }
          : q
      )
    );
  };

  const updateChoice = (qId: string, index: number, text: string, isCorrect: boolean) => {
    setQuestions((prev) =>
      prev.map((q) => {
        if (q.id !== qId) return q;
        const newChoices = [...q.choices];
        newChoices[index] = { text, isCorrect };
        return { ...q, choices: newChoices };
      })
    );
  };

  const removeChoice = (qId: string, index: number) => {
    setQuestions((prev) =>
      prev.map((q) => {
        if (q.id !== qId) return q;
        return { ...q, choices: q.choices.filter((_, i) => i !== index) };
      })
    );
  };

  const totalMarks = questions.reduce((sum, q) => sum + (q.marks || 0), 0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      toast.error('Please enter an exam title');
      return;
    }
    if (questions.length === 0) {
      toast.error('Please add at least one question');
      return;
    }

    try {
      await createExamMutation.mutateAsync({
        title,
        description,
        durationMinutes,
        totalMarks,
        passingMarks,
      });

      toast.success('Exam created! Add questions from the exam management page. 🎉');
      router.push('/teacher/exams');
    } catch {
      toast.error('Failed to create exam');
    }
  };

  return (
    <div className="container mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <button onClick={() => router.back()} className="hover:text-foreground transition-colors flex items-center gap-1">
          <ArrowLeft className="h-4 w-4" /> Back to Exams
        </button>
      </div>

      <PageHeader
        title="Create New Examination"
        description="Configure exam rules and populate questions for automated grading."
      />

      <form onSubmit={handleSubmit} className="space-y-8">
        {/* Basic Info Card */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 shadow-sm space-y-6">
          <h2 className="text-lg font-bold border-b border-border/60 pb-3">Exam Settings</h2>

          <div className="space-y-4">
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                Exam Title *
              </label>
              <input
                type="text"
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. Midterm Computer Science & AI Assessment"
                className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                Instructions / Description
              </label>
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Guidelines, allowed materials, and instructions for students..."
                className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
              />
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                  Duration (Minutes)
                </label>
                <div className="relative">
                  <Clock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <input
                    type="number"
                    min={5}
                    value={durationMinutes}
                    onChange={(e) => setDurationMinutes(Number(e.target.value))}
                    className="w-full rounded-xl border border-border bg-background pl-9 pr-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
                  />
                </div>
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                  Passing Score (%)
                </label>
                <input
                  type="number"
                  min={1}
                  max={100}
                  value={passingMarks}
                  onChange={(e) => setPassingMarks(Number(e.target.value))}
                  className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1.5">
                  Total Marks Calculated
                </label>
                <div className="flex h-10 items-center justify-center rounded-xl bg-primary/10 font-bold text-primary text-sm">
                  {totalMarks} Marks ({questions.length} Questions)
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Questions Section */}
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold">Question Bank ({questions.length})</h2>
            <button
              type="button"
              onClick={addQuestion}
              className="inline-flex items-center gap-1.5 rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90 transition-all"
            >
              <Plus className="h-4 w-4" /> Add Question
            </button>
          </div>

          {questions.map((q, idx) => (
            <div key={q.id} className="rounded-3xl border border-border/80 bg-card p-6 shadow-sm space-y-4">
              <div className="flex items-center justify-between border-b border-border/60 pb-3">
                <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary/10 text-xs font-bold text-primary">
                  Q{idx + 1}
                </span>

                <div className="flex items-center gap-3">
                  <select
                    value={q.questionType}
                    onChange={(e) => updateQuestion(q.id, 'questionType', e.target.value)}
                    className="rounded-lg border border-border bg-background px-3 py-1 text-xs font-semibold"
                  >
                    <option value="MCQ">Multiple Choice (MCQ)</option>
                    <option value="TrueFalse">True / False</option>
                    <option value="Essay">Written Essay</option>
                    <option value="Programming">Programming Task</option>
                  </select>

                  <div className="flex items-center gap-1">
                    <span className="text-xs text-muted-foreground">Marks:</span>
                    <input
                      type="number"
                      min={1}
                      value={q.marks}
                      onChange={(e) => updateQuestion(q.id, 'marks', Number(e.target.value))}
                      className="w-16 rounded-lg border border-border bg-background px-2 py-1 text-xs font-bold"
                    />
                  </div>

                  {questions.length > 1 && (
                    <button
                      type="button"
                      onClick={() => removeQuestion(q.id)}
                      className="text-destructive hover:bg-destructive/10 p-1.5 rounded-lg transition-colors"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  )}
                </div>
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Question Prompt
                </label>
                <textarea
                  rows={2}
                  value={q.body}
                  onChange={(e) => updateQuestion(q.id, 'body', e.target.value)}
                  placeholder="Enter the question text here..."
                  className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm font-medium focus:border-primary focus:outline-none"
                />
              </div>

              {/* Choices for MCQ / TrueFalse */}
              {(q.questionType === 'MCQ' || q.questionType === 'TrueFalse') && (
                <div className="space-y-3 pt-2">
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                      Answer Choices (Check correct answer)
                    </span>
                    {q.questionType === 'MCQ' && (
                      <button
                        type="button"
                        onClick={() => addChoice(q.id)}
                        className="text-xs font-bold text-primary hover:underline"
                      >
                        + Add Choice
                      </button>
                    )}
                  </div>

                  <div className="space-y-2">
                    {q.choices.map((choice, cIdx) => (
                      <div key={cIdx} className="flex items-center gap-3">
                        <input
                          type="checkbox"
                          checked={choice.isCorrect}
                          onChange={(e) => updateChoice(q.id, cIdx, choice.text, e.target.checked)}
                          className="h-4 w-4 rounded border-border text-primary focus:ring-primary"
                        />
                        <input
                          type="text"
                          value={choice.text}
                          onChange={(e) => updateChoice(q.id, cIdx, e.target.value, choice.isCorrect)}
                          placeholder={`Choice ${cIdx + 1}`}
                          className="flex-1 rounded-xl border border-border bg-background px-3.5 py-1.5 text-sm focus:border-primary focus:outline-none"
                        />
                        {q.choices.length > 2 && (
                          <button
                            type="button"
                            onClick={() => removeChoice(q.id, cIdx)}
                            className="text-muted-foreground hover:text-destructive text-xs"
                          >
                            Remove
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Submit */}
        <div className="flex items-center justify-end gap-4 border-t border-border/60 pt-6">
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded-xl border border-border px-5 py-2.5 text-sm font-semibold hover:bg-muted"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={createExamMutation.isPending}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all disabled:opacity-50"
          >
            <Save className="h-4 w-4" /> Save Exam
          </button>
        </div>
      </form>
    </div>
  );
}

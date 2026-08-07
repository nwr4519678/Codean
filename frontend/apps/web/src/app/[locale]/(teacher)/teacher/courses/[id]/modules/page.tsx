'use client';

import React, { useState } from 'react';
import { useParams } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import dynamic from 'next/dynamic';
import { PlusCircle, GripVertical, Trash2, ChevronDown, ChevronUp, Save, Loader2, Video, FileText } from 'lucide-react';
import { toast } from 'sonner';

// Lazy-load Tiptap editor
const TiptapEditor = dynamic(
  () => import('@/components/editor/TiptapEditor').then((mod) => ({ default: mod.TiptapEditor })),
  { ssr: false, loading: () => <div className="h-48 rounded-xl border border-border bg-muted/30 animate-pulse" /> }
);

interface Lesson {
  id: number;
  title: string;
  type: 'video' | 'text';
  content: string;
}

interface Module {
  id: number;
  title: string;
  lessons: Lesson[];
  isExpanded: boolean;
}

export default function CourseModulesPage() {
  const params = useParams();
  const courseId = params?.id;
  const [isSaving, setIsSaving] = useState(false);

  const [modules, setModules] = useState<Module[]>([
    {
      id: 1,
      title: 'Module 1: Introduction & Project Setup',
      lessons: [
        { id: 1, title: 'Course Overview', type: 'video', content: '' },
        { id: 2, title: 'Environment Setup', type: 'text', content: '<p>Configure your development environment...</p>' },
      ],
      isExpanded: true,
    },
  ]);

  const addModule = () => {
    const newId = Date.now();
    setModules((prev) => [
      ...prev,
      { id: newId, title: `Module ${prev.length + 1}: New Module`, lessons: [], isExpanded: true },
    ]);
  };

  const toggleModule = (moduleId: number) => {
    setModules((prev) =>
      prev.map((m) => (m.id === moduleId ? { ...m, isExpanded: !m.isExpanded } : m))
    );
  };

  const updateModuleTitle = (moduleId: number, title: string) => {
    setModules((prev) => prev.map((m) => (m.id === moduleId ? { ...m, title } : m)));
  };

  const addLesson = (moduleId: number) => {
    setModules((prev) =>
      prev.map((m) =>
        m.id === moduleId
          ? { ...m, lessons: [...m.lessons, { id: Date.now(), title: 'New Lesson', type: 'video', content: '' }] }
          : m
      )
    );
  };

  const updateLesson = (moduleId: number, lessonId: number, updates: Partial<Lesson>) => {
    setModules((prev) =>
      prev.map((m) =>
        m.id === moduleId
          ? { ...m, lessons: m.lessons.map((l) => (l.id === lessonId ? { ...l, ...updates } : l)) }
          : m
      )
    );
  };

  const removeLesson = (moduleId: number, lessonId: number) => {
    setModules((prev) =>
      prev.map((m) =>
        m.id === moduleId ? { ...m, lessons: m.lessons.filter((l) => l.id !== lessonId) } : m
      )
    );
  };

  const handleSave = async () => {
    setIsSaving(true);
    setTimeout(() => {
      setIsSaving(false);
      toast.success('Course modules saved successfully!');
    }, 1000);
  };

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Course Module Editor"
        description={`Organize lessons, upload videos, and add rich text content for Course #${courseId}`}
        action={
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="inline-flex items-center gap-2 rounded-xl bg-emerald-500 px-5 py-2.5 text-sm font-semibold text-white shadow-lg hover:bg-emerald-600 transition-all"
          >
            {isSaving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
            Save All Changes
          </button>
        }
      />

      {/* Module List */}
      <div className="space-y-4">
        {modules.map((module, idx) => (
          <div key={module.id} className="rounded-2xl border border-border/70 bg-card shadow-sm overflow-hidden">
            {/* Module Header */}
            <div className="flex items-center gap-4 border-b border-border/40 bg-muted/30 px-5 py-3.5">
              <GripVertical className="h-5 w-5 text-muted-foreground shrink-0 cursor-grab" />
              <input
                value={module.title}
                onChange={(e) => updateModuleTitle(module.id, e.target.value)}
                className="flex-1 bg-transparent text-sm font-bold text-foreground focus:outline-none"
              />
              <button onClick={() => toggleModule(module.id)} className="text-muted-foreground hover:text-foreground">
                {module.isExpanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
              </button>
            </div>

            {/* Lessons */}
            {module.isExpanded && (
              <div className="divide-y divide-border/40">
                {module.lessons.map((lesson, lessonIdx) => (
                  <div key={lesson.id} className="p-5 space-y-4">
                    <div className="flex items-center gap-3">
                      <GripVertical className="h-4 w-4 text-muted-foreground cursor-grab" />
                      <input
                        value={lesson.title}
                        onChange={(e) => updateLesson(module.id, lesson.id, { title: e.target.value })}
                        placeholder="Lesson title..."
                        className="flex-1 rounded-xl border border-input bg-background px-3 py-2 text-xs font-semibold text-foreground focus:border-primary focus:outline-none"
                      />
                      <select
                        value={lesson.type}
                        onChange={(e) => updateLesson(module.id, lesson.id, { type: e.target.value as 'video' | 'text' })}
                        className="rounded-lg border border-input bg-background px-2 py-1.5 text-xs font-semibold"
                      >
                        <option value="video">📹 Video</option>
                        <option value="text">📝 Rich Text</option>
                      </select>
                      <button
                        onClick={() => removeLesson(module.id, lesson.id)}
                        className="text-destructive/60 hover:text-destructive transition-colors"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>

                    {lesson.type === 'video' ? (
                      <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-border p-6 text-center bg-background">
                        <Video className="h-8 w-8 text-primary mb-2" />
                        <p className="text-xs font-semibold">Drag & drop video, or click to upload</p>
                        <p className="text-[10px] text-muted-foreground mt-1">MP4 supported up to 5GB</p>
                      </div>
                    ) : (
                      <div className="rounded-xl border border-border overflow-hidden">
                        <TiptapEditor
                          content={lesson.content}
                          onChange={(html) => updateLesson(module.id, lesson.id, { content: html })}
                        />
                      </div>
                    )}
                  </div>
                ))}

                <div className="px-5 py-3">
                  <button
                    onClick={() => addLesson(module.id)}
                    className="inline-flex items-center gap-2 text-xs font-semibold text-primary hover:underline"
                  >
                    <PlusCircle className="h-3.5 w-3.5" />
                    Add Lesson
                  </button>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>

      <button
        onClick={addModule}
        className="flex w-full items-center justify-center gap-2 rounded-2xl border-2 border-dashed border-border py-4 text-sm font-semibold text-muted-foreground hover:border-primary hover:text-primary transition-all"
      >
        <PlusCircle className="h-4 w-4" />
        Add New Module
      </button>
    </div>
  );
}

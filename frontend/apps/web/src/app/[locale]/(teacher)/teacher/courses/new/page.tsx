'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCreateCourse } from '@platform/api';
import { PlusCircle, Loader2, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import Link from 'next/link';

export default function CreateCoursePage() {
  const router = useRouter();
  const createCourseMutation = useCreateCourse();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('C# & .NET');
  const [price, setPrice] = useState(0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim() || !description.trim()) {
      toast.error('Please enter a title and description');
      return;
    }
    try {
      const course = await createCourseMutation.mutateAsync({
        title,
        description,
        category,
        price: Number(price),
      });
      toast.success('Course created successfully!');
      router.push(`/teacher/courses/${course.id}/modules`);
    } catch {
      toast.error('Failed to create course');
    }
  };

  return (
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <div className="flex items-center gap-4">
        <Link href="/teacher/courses" className="flex h-9 w-9 items-center justify-center rounded-xl border border-border hover:bg-muted text-muted-foreground">
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <PageHeader title="Create New Course" description="Set up basic course details before adding modules and lessons." />
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 rounded-3xl border border-border/70 bg-card p-8 shadow-xl">
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Course Title</label>
            <input
              type="text"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Clean Architecture in .NET 10"
              className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>

          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Description</label>
            <textarea
              rows={4}
              required
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Detailed overview of what students will learn in this course..."
              className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Category</label>
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              >
                <option value="C# & .NET">C# & .NET</option>
                <option value="Python">Python</option>
                <option value="Web Development">Web Development</option>
                <option value="System Design">System Design</option>
                <option value="DevOps">DevOps</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Price ($ USD)</label>
              <input
                type="number"
                min={0}
                required
                value={price}
                onChange={(e) => setPrice(Number(e.target.value))}
                className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              />
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3 border-t border-border/40 pt-6">
          <Link href="/teacher/courses" className="rounded-xl border border-border bg-muted px-5 py-2.5 text-xs font-semibold hover:bg-background">
            Cancel
          </Link>
          <button
            type="submit"
            disabled={createCourseMutation.isPending}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-xs font-semibold text-white shadow-lg hover:bg-primary/90 disabled:opacity-50"
          >
            {createCourseMutation.isPending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <PlusCircle className="h-4 w-4" />
                Create Course &amp; Add Modules
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}

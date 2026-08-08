'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCourseDetail, useUpdateCourse } from '@platform/api';
import { Save, Loader2, ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import Link from 'next/link';

export default function EditCoursePage() {
  const params = useParams();
  const courseId = params?.id as string;
  const router = useRouter();

  const { data: course, isLoading } = useCourseDetail(courseId);
  const updateCourseMutation = useUpdateCourse();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('C# & .NET');
  const [price, setPrice] = useState(0);

  useEffect(() => {
    if (course) {
      setTitle(course.title);
      setDescription(course.description);
      setCategory(course.category);
      setPrice(course.price);
    }
  }, [course]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await updateCourseMutation.mutateAsync({
        id: courseId,
        payload: { title, description, category, price: Number(price) },
      });
      toast.success('Course updated successfully!');
      router.push('/teacher/courses');
    } catch {
      toast.error('Failed to update course');
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
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <div className="flex items-center gap-4">
        <Link href="/teacher/courses" className="flex h-9 w-9 items-center justify-center rounded-xl border border-border hover:bg-muted text-muted-foreground">
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <PageHeader title={`Edit: ${course?.title || 'Course'}`} description="Update course metadata and settings." />
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
            disabled={updateCourseMutation.isPending}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-xs font-semibold text-white shadow-lg hover:bg-primary/90 disabled:opacity-50"
          >
            {updateCourseMutation.isPending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <Save className="h-4 w-4" />
                Save Changes
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}

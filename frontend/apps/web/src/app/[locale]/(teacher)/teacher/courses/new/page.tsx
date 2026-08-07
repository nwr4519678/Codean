'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { useDropzone } from 'react-dropzone';
import { UploadCloud, ArrowRight, Loader2 } from 'lucide-react';
import { coursesApi } from '@platform/api';
import { toast } from 'sonner';

export default function CreateCoursePage() {
  const router = useRouter();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('C# & .NET');
  const [price, setPrice] = useState(49);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    accept: { 'image/*': ['.png', '.jpg', '.jpeg', '.webp'] },
    maxFiles: 1,
    onDrop: (acceptedFiles) => {
      if (acceptedFiles.length > 0) {
        setUploadedFile(acceptedFiles[0]);
        toast.success(`Selected thumbnail: ${acceptedFiles[0].name}`);
      }
    },
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      const course = await coursesApi.createCourse({ title, description, category, price: Number(price) });
      toast.success('Course created successfully!');
      router.push(`/teacher/courses/${course.id}/modules`);
    } catch {
      toast.error('Failed to create course');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader title="Create New Course" description="Build an engaging curriculum with video modules and coding challenges." />

      <form onSubmit={handleSubmit} className="space-y-6 rounded-3xl border border-border/70 bg-card p-8 shadow-xl">
        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Course Title</label>
          <input type="text" required value={title} onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. Master Clean Architecture & DDD in .NET 10"
            className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Category</label>
            <select value={category} onChange={(e) => setCategory(e.target.value)}
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
            <input type="number" min="0" required value={price} onChange={(e) => setPrice(Number(e.target.value))}
              className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Course Overview</label>
          <textarea rows={4} required value={description} onChange={(e) => setDescription(e.target.value)}
            placeholder="Provide a comprehensive summary of what students will learn..."
            className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
          />
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground mb-2">Course Thumbnail</label>
          <div {...getRootProps()} className={`flex flex-col items-center justify-center rounded-2xl border-2 border-dashed p-8 cursor-pointer transition-all ${isDragActive ? 'border-primary bg-primary/10' : 'border-border bg-background hover:border-primary/50'}`}>
            <input {...getInputProps()} />
            <UploadCloud className="h-10 w-10 text-primary mb-2" />
            <p className="text-sm font-semibold">
              {uploadedFile ? uploadedFile.name : 'Drag & drop thumbnail image here, or click to browse'}
            </p>
            <p className="text-xs text-muted-foreground mt-1">Supports PNG, JPG, WebP up to 10MB</p>
          </div>
        </div>

        <button type="submit" disabled={isSubmitting}
          className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-base font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95 disabled:opacity-50"
        >
          {isSubmitting ? <Loader2 className="h-5 w-5 animate-spin" /> : <><span>Save & Continue to Modules</span><ArrowRight className="h-4 w-4" /></>}
        </button>
      </form>
    </div>
  );
}

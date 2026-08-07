'use client';

import React, { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCourseDetail } from '@platform/api';
import { useDropzone } from 'react-dropzone';
import { UploadCloud, Save, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export default function EditCoursePage() {
  const params = useParams();
  const router = useRouter();
  const courseId = params?.id as string;
  const { data: course } = useCourseDetail(courseId);

  const [title, setTitle] = useState(course?.title || '');
  const [description, setDescription] = useState(course?.description || '');
  const [price, setPrice] = useState(course?.price || 49);
  const [isSaving, setIsSaving] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    accept: { 'image/*': ['.png', '.jpg', '.jpeg', '.webp'] },
    maxFiles: 1,
    onDrop: (acceptedFiles) => {
      if (acceptedFiles.length > 0) {
        setUploadedFile(acceptedFiles[0]);
        toast.success(`Thumbnail selected: ${acceptedFiles[0].name}`);
      }
    },
  });

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    setTimeout(() => {
      setIsSaving(false);
      toast.success('Course details saved!');
    }, 800);
  };

  return (
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader title="Edit Course Details" description={`Editing: ${course?.title || `Course #${courseId}`}`} />

      <form onSubmit={handleSave} className="space-y-6 rounded-3xl border border-border/70 bg-card p-8 shadow-xl">
        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Course Title</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder={course?.title || 'Course Title'}
            className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none"
          />
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Price ($ USD)</label>
          <input
            type="number"
            min="0"
            value={price}
            onChange={(e) => setPrice(Number(e.target.value))}
            className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none"
          />
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Course Overview</label>
          <textarea
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder={course?.description || 'Course description...'}
            className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none"
          />
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-foreground mb-2">Update Thumbnail</label>
          <div
            {...getRootProps()}
            className={`flex flex-col items-center justify-center rounded-2xl border-2 border-dashed p-8 cursor-pointer transition-all ${isDragActive ? 'border-primary bg-primary/10' : 'border-border bg-background hover:border-primary/50'}`}
          >
            <input {...getInputProps()} />
            <UploadCloud className="h-10 w-10 text-primary mb-2" />
            <p className="text-sm font-semibold">
              {uploadedFile ? uploadedFile.name : 'Drag & drop or click to replace thumbnail'}
            </p>
          </div>
        </div>

        <button
          type="submit"
          disabled={isSaving}
          className="flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg hover:bg-primary/90 transition-all active:scale-95 disabled:opacity-50"
        >
          {isSaving ? <Loader2 className="h-4 w-4 animate-spin" /> : <><Save className="h-4 w-4" /> Save Changes</>}
        </button>
      </form>
    </div>
  );
}

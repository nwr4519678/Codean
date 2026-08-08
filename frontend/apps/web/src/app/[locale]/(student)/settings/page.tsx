'use client';

import React, { useEffect, useState } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { useStudentProfile, useUpdateStudentProfile } from '@platform/api';
import { Save, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export default function SettingsPage() {
  const { data: profile, isLoading, isError } = useStudentProfile();
  const updateProfile = useUpdateStudentProfile();
  const [grade, setGrade] = useState('');
  const [school, setSchool] = useState('');
  const [parentPhone, setParentPhone] = useState('');
  const [parentPhone2, setParentPhone2] = useState('');
  const [notes, setNotes] = useState('');

  useEffect(() => {
    if (!profile) return;
    setGrade((profile as typeof profile & { grade?: string }).grade ?? '');
    setSchool((profile as typeof profile & { school?: string }).school ?? '');
    setParentPhone((profile as typeof profile & { parentPhone?: string }).parentPhone ?? '');
    setParentPhone2((profile as typeof profile & { parentPhone2?: string }).parentPhone2 ?? '');
    setNotes((profile as typeof profile & { notes?: string }).notes ?? '');
  }, [profile]);

  const handleSave = async (event: React.FormEvent) => {
    event.preventDefault();
    try {
      await updateProfile.mutateAsync({ grade, school, parentPhone, parentPhone2, notes });
      toast.success('Student profile updated.');
    } catch {
      toast.error('Unable to save profile settings.');
    }
  };

  return (
    <div className="container mx-auto max-w-3xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader title="Student profile" description="Keep your academic and parent contact information up to date." />
      {isLoading ? <div className="flex min-h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div> : isError ? <p role="alert" className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">Profile settings could not be loaded.</p> : (
      <form onSubmit={handleSave} className="space-y-6 rounded-3xl border border-border/70 bg-card p-8 shadow-xl">
        <div className="grid gap-4 sm:grid-cols-2">
          <Field id="grade" label="Grade" value={grade} onChange={setGrade} />
          <Field id="school" label="School" value={school} onChange={setSchool} />
          <Field id="parentPhone" label="Primary parent phone" value={parentPhone} onChange={setParentPhone} type="tel" />
          <Field id="parentPhone2" label="Secondary parent phone" value={parentPhone2} onChange={setParentPhone2} type="tel" />
        </div>
        <div><label htmlFor="notes" className="block text-xs font-bold uppercase tracking-wider text-foreground">Notes</label><textarea id="notes" rows={3} value={notes} onChange={(event) => setNotes(event.target.value)} className="mt-2 block w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" /></div>
        <button type="submit" disabled={updateProfile.isPending} className="inline-flex min-h-11 items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg transition-colors hover:bg-primary/90 disabled:opacity-50">{updateProfile.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />} Save changes</button>
      </form>) }
    </div>
  );
}

function Field({ id, label, value, onChange, type = 'text' }: { id: string; label: string; value: string; onChange: (value: string) => void; type?: string }) {
  return <div><label htmlFor={id} className="block text-xs font-bold uppercase tracking-wider text-foreground">{label}</label><input id={id} type={type} value={value} onChange={(event) => onChange(event.target.value)} className="mt-2 block min-h-11 w-full rounded-xl border border-input bg-background px-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" /></div>;
}

'use client';

import React, { useState } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Megaphone, Send, Bell, Plus, Calendar, Loader2 } from 'lucide-react';
import { useAnnouncements, useCreateAnnouncement } from '@platform/api';
import { toast } from 'sonner';

export default function TeacherAnnouncementsPage() {
  const { data: announcementsData, isLoading } = useAnnouncements();
  const createAnnouncementMutation = useCreateAnnouncement();

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [showModal, setShowModal] = useState(false);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await createAnnouncementMutation.mutateAsync({
        title,
        body: content,
      });
      toast.success('Announcement broadcasted to enrolled students!');
      setShowModal(false);
      setTitle('');
      setContent('');
    } catch {
      toast.error('Failed to broadcast announcement');
    }
  };

  const announcements = announcementsData?.items ?? [];

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Course Announcements"
        description="Broadcast important updates, schedule changes, and news to your enrolled students."
        action={
          <button
            onClick={() => setShowModal(true)}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all"
          >
            <Megaphone className="h-4 w-4" /> New Broadcast
          </button>
        }
      />

      {isLoading ? (
        <div className="flex min-h-[300px] items-center justify-center rounded-2xl border border-border bg-card">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : announcements.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border p-12 text-center">
          <Megaphone className="mx-auto h-12 w-12 text-muted-foreground/50 mb-3" />
          <h3 className="text-base font-bold">No announcements broadcasted yet</h3>
          <p className="text-xs text-muted-foreground mt-1 max-w-sm mx-auto">
            Keep your students informed by broadcasting course updates, exam schedules, or live session links.
          </p>
        </div>
      ) : (
        <div className="space-y-4 max-w-3xl">
          {announcements.map((ann: any) => (
            <div key={ann.id} className="rounded-2xl border border-border/80 bg-card p-6 shadow-sm space-y-2">
              <div className="flex items-center justify-between">
                <span className="inline-flex items-center gap-1.5 rounded-full bg-accent/10 px-3 py-1 text-xs font-bold text-accent">
                  <Bell className="h-3.5 w-3.5" /> Broadcast Announcement
                </span>
                <span className="text-xs text-muted-foreground flex items-center gap-1">
                  <Calendar className="h-3.5 w-3.5" /> {new Date(ann.createdAt || Date.now()).toLocaleDateString()}
                </span>
              </div>
              <h3 className="text-lg font-bold pt-1">{ann.title}</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">{ann.content}</p>
            </div>
          ))}
        </div>
      )}

      {/* Broadcast Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-sm">
          <div className="w-full max-w-lg rounded-3xl border border-border bg-card p-6 shadow-2xl space-y-4">
            <h3 className="text-lg font-bold">Create New Broadcast Announcement</h3>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Announcement Title *
                </label>
                <input
                  type="text"
                  required
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="e.g. Live Q&A Session Scheduled for Friday"
                  className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">
                  Content Body *
                </label>
                <textarea
                  rows={4}
                  required
                  value={content}
                  onChange={(e) => setContent(e.target.value)}
                  placeholder="Write message details for your students..."
                  className="w-full rounded-xl border border-border bg-background px-4 py-2.5 text-sm"
                />
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="rounded-xl border border-border px-4 py-2 text-xs font-semibold hover:bg-muted"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createAnnouncementMutation.isPending}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90"
                >
                  <Send className="h-3.5 w-3.5" /> Broadcast Now
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

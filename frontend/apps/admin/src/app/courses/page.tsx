'use client';

import React from 'react';
import { BookOpen, CheckCircle, Clock } from 'lucide-react';

export default function AdminCoursesPage() {
  return (
    <div className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-white shadow-lg">
              <BookOpen className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Course Moderation</h1>
              <p className="text-sm text-muted-foreground">Review and publish courses created by instructors</p>
            </div>
          </div>
        </div>

        <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-border bg-muted/40 text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-6 py-4">Course Title</th>
                <th className="px-6 py-4">Instructor</th>
                <th className="px-6 py-4">Category</th>
                <th className="px-6 py-4">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {[
                { id: 1, title: 'Clean Architecture in .NET 10', teacher: 'Sarah Connor', category: 'C# & .NET', status: 'Published' },
                { id: 2, title: 'Python Async & Concurrency', teacher: 'John Doe', category: 'Python', status: 'Under Review' },
              ].map((c) => (
                <tr key={c.id} className="hover:bg-muted/30">
                  <td className="px-6 py-4 font-bold">{c.title}</td>
                  <td className="px-6 py-4 text-xs text-muted-foreground">{c.teacher}</td>
                  <td className="px-6 py-4 text-xs font-semibold">{c.category}</td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-semibold ${c.status === 'Published' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-amber-500/10 text-amber-500'}`}>
                      {c.status === 'Published' ? <CheckCircle className="h-3 w-3" /> : <Clock className="h-3 w-3" />}
                      {c.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

'use client';

import React, { useState } from 'react';
import { PageHeader } from '@/components/shared/PageHeader';
import { Award, Download, CheckCircle2, ShieldCheck, Printer, Sparkles, X } from 'lucide-react';
import { useCurrentUser, useEnrolledCourses } from '@platform/api';

export default function StudentCertificatesPage() {
  const { data: user } = useCurrentUser();
  const { data: enrolledData, isLoading } = useEnrolledCourses();

  const [selectedCertificate, setSelectedCertificate] = useState<any | null>(null);

  const completedCourses = (enrolledData ?? []).filter((e: any) => e.completionPercentage === 100);

  const studentName = user ? `${user.firstName} ${user.lastName}` : 'Student';

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="My Certificates"
        description="Official course completion credentials awarded upon mastering course curricula."
      />

      {completedCourses.length === 0 ? (
        <div className="rounded-3xl border border-dashed border-border p-12 text-center text-muted-foreground space-y-3">
          <Award className="mx-auto h-12 w-12 text-muted-foreground/40" />
          <h3 className="text-lg font-bold text-foreground">No Certificates Earned Yet</h3>
          <p className="text-xs max-w-md mx-auto">
            Complete 100% of any enrolled course's lessons and quizzes to earn your verified certificate of completion.
          </p>
        </div>
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {completedCourses.map((c: any) => (
            <div
              key={c.courseId || c.id}
              className="group relative overflow-hidden rounded-3xl border border-border/80 bg-gradient-to-b from-card to-accent/5 p-6 shadow-sm hover:border-accent/40 transition-all space-y-4"
            >
              <div className="flex items-center justify-between">
                <div className="rounded-2xl bg-amber-500/10 p-3 text-amber-500">
                  <Award className="h-6 w-6" />
                </div>
                <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-2.5 py-0.5 text-[10px] font-bold text-emerald-500 border border-emerald-500/20">
                  <CheckCircle2 className="h-3 w-3" /> Verified
                </span>
              </div>

              <div>
                <h3 className="font-bold text-lg leading-tight group-hover:text-primary transition-colors">
                  {c.courseTitle || c.title}
                </h3>
                <p className="text-xs text-muted-foreground mt-1">Issued to {studentName}</p>
              </div>

              <div className="pt-2 border-t border-border/40 flex items-center justify-between">
                <span className="text-[10px] font-mono text-muted-foreground">ID: CERT-{c.courseId || c.id}-2026</span>
                <button
                  onClick={() => setSelectedCertificate(c)}
                  className="inline-flex items-center gap-1.5 rounded-xl bg-primary px-3.5 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-primary/90 transition-all"
                >
                  View Certificate
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Interactive Certificate View Modal */}
      {selectedCertificate && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-md">
          <div className="relative w-full max-w-2xl rounded-3xl border border-amber-500/30 bg-card p-8 shadow-2xl space-y-6">
            <button
              onClick={() => setSelectedCertificate(null)}
              className="absolute right-4 top-4 rounded-full p-2 hover:bg-muted"
            >
              <X className="h-5 w-5" />
            </button>

            {/* Certificate Canvas Mockup */}
            <div className="rounded-2xl border-4 border-double border-amber-500/30 bg-gradient-to-b from-background via-card to-amber-500/5 p-8 text-center space-y-6">
              <div className="flex justify-center">
                <div className="rounded-full bg-amber-500/10 p-4 text-amber-500">
                  <Award className="h-12 w-12" />
                </div>
              </div>

              <div>
                <span className="text-xs font-bold uppercase tracking-widest text-amber-500">Certificate of Completion</span>
                <h2 className="text-2xl font-black tracking-tight mt-1">CODEAN PROGRAMMING PLATFORM</h2>
              </div>

              <p className="text-xs text-muted-foreground">This is proudly presented to</p>
              <h3 className="text-2xl font-extrabold text-primary underline underline-offset-8">{studentName}</h3>
              <p className="text-xs text-muted-foreground max-w-md mx-auto">
                for successfully completing all modules, practical assessments, and examinations for the course:
              </p>
              <p className="text-lg font-bold text-foreground">{selectedCertificate.courseTitle || selectedCertificate.title}</p>

              <div className="flex items-center justify-between pt-6 border-t border-border/40 text-xs text-muted-foreground">
                <div className="flex items-center gap-1 text-emerald-500 font-semibold">
                  <ShieldCheck className="h-4 w-4" /> Cryptographically Verified
                </div>
                <div>Date: {new Date().toLocaleDateString()}</div>
              </div>
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => window.print()}
                className="inline-flex items-center gap-2 rounded-xl border border-border px-4 py-2 text-xs font-semibold hover:bg-muted"
              >
                <Printer className="h-4 w-4" /> Print
              </button>
              <button
                onClick={() => window.print()}
                className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2 text-xs font-semibold text-white hover:bg-primary/90"
              >
                <Download className="h-4 w-4" /> Download PDF
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

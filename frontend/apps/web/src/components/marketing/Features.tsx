import React from 'react';
import { Code2, Video, Zap, ShieldCheck, Cpu, Globe } from 'lucide-react';

export function Features() {
  const features = [
    {
      icon: Code2,
      title: 'Automated Code Judge',
      description: 'Run unit test suites instantly inside an isolated Docker sandbox with multi-language support (C#, Python, JS).',
      color: 'text-primary',
    },
    {
      icon: Video,
      title: 'HD Video Courses',
      description: 'Streaming video player with progress tracking, speed control, and downloadable lesson resources.',
      color: 'text-accent',
    },
    {
      icon: Cpu,
      title: 'Real-time Telemetry',
      description: 'Track execution time, memory utilization, and test case coverage for every algorithm submission.',
      color: 'text-emerald-500',
    },
    {
      icon: Globe,
      title: 'Bilingual (EN / AR)',
      description: 'Full English & Arabic localized interface with automatic RTL orientation and regional formatting.',
      color: 'text-purple-500',
    },
    {
      icon: ShieldCheck,
      title: 'Enterprise Security',
      description: 'JWT token rotation, role-based authorization, and comprehensive security audit trail.',
      color: 'text-rose-500',
    },
    {
      icon: Zap,
      title: 'Live Q&A Sessions',
      description: 'Interactive live instructor sessions with real-time chat and screen sharing.',
      color: 'text-amber-500',
    },
  ];

  return (
    <section className="py-20 bg-muted/30 border-y border-border/40">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 space-y-12">
        <div className="text-center space-y-3 max-w-2xl mx-auto">
          <span className="rounded-full bg-primary/10 px-3.5 py-1 text-xs font-semibold text-primary">
            Built for Engineers
          </span>
          <h2 className="text-3xl font-extrabold tracking-tight sm:text-4xl">Everything You Need to Excel</h2>
          <p className="text-sm text-muted-foreground">
            A comprehensive suite of tools designed to take you from junior to senior software engineer.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-3">
          {features.map(({ icon: Icon, title, description, color }) => (
            <div
              key={title}
              className="rounded-3xl border border-border/70 bg-card p-8 shadow-sm transition-all hover:border-primary/50 hover:shadow-lg space-y-4"
            >
              <div className={`flex h-12 w-12 items-center justify-center rounded-2xl bg-muted/80 ${color}`}>
                <Icon className="h-6 w-6" />
              </div>
              <h3 className="text-xl font-bold">{title}</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">{description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

import React from 'react';
import Link from 'next/link';
import { Sparkles, ArrowRight, Code2, BookOpen, Users, Trophy } from 'lucide-react';

export function Hero() {
  return (
    <section className="relative overflow-hidden py-20 text-center lg:py-32">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -top-40 left-1/2 -z-10 h-[500px] w-[800px] -translate-x-1/2 rounded-full bg-gradient-to-tr from-primary/30 to-accent/20 blur-[120px]"
      />

      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-primary/20 bg-primary/10 px-4 py-1.5 text-xs font-semibold text-primary">
          <Sparkles className="h-4 w-4" />
          <span>Interactive Software Engineering Platform</span>
        </div>

        <h1 className="max-w-4xl mx-auto text-4xl font-extrabold tracking-tight sm:text-6xl lg:text-7xl">
          Master Modern Engineering with Interactive Code Judging
        </h1>

        <p className="mt-6 max-w-2xl mx-auto text-lg text-muted-foreground sm:text-xl">
          Learn C# .NET, Python, System Design, and Algorithms with hands-on video modules, live code execution, and automated feedback.
        </p>

        <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row sm:gap-6">
          <Link
            href="/courses"
            className="inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-primary px-8 text-base font-semibold text-white shadow-xl shadow-primary/25 transition-all hover:bg-primary/90 active:scale-95"
          >
            Explore Courses
            <ArrowRight className="h-4 w-4" />
          </Link>
          <Link
            href="/auth/register"
            className="inline-flex h-12 items-center justify-center rounded-xl border border-border bg-card px-8 text-base font-semibold text-foreground transition-all hover:bg-muted active:scale-95"
          >
            Start Free Trial
          </Link>
        </div>

        {/* Stats Grid */}
        <div className="mt-20 grid grid-cols-2 gap-6 lg:grid-cols-4 max-w-5xl mx-auto">
          {[
            { icon: BookOpen, label: 'Practical Courses', value: '50+', color: 'text-primary' },
            { icon: Code2, label: 'Code Challenges', value: '500+', color: 'text-accent' },
            { icon: Users, label: 'Active Students', value: '10,000+', color: 'text-emerald-500' },
            { icon: Trophy, label: 'Completion Rate', value: '98%', color: 'text-purple-500' },
          ].map(({ icon: Icon, label, value, color }) => (
            <div
              key={label}
              className="flex flex-col items-center justify-center rounded-2xl border border-border/60 bg-card/60 p-6 backdrop-blur-sm shadow-sm"
            >
              <Icon className={`mb-3 h-8 w-8 ${color}`} />
              <span className="text-3xl font-extrabold">{value}</span>
              <span className="mt-1 text-center text-xs text-muted-foreground">{label}</span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

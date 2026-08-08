import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import Link from 'next/link';
import {
  BookOpen,
  Code2,
  Sparkles,
  Trophy,
  Users,
  Video,
  Brain,
  CheckCircle,
  ArrowRight,
  Star,
  GraduationCap,
  Zap,
  Shield,
  Globe,
  ChevronRight,
} from 'lucide-react';

export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>;
}): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'hero' });
  return {
    title: `Codean — ${t('title')}`,
    description: t('subtitle'),
    openGraph: {
      title: `Codean — ${t('title')}`,
      description: t('subtitle'),
      type: 'website',
    },
  };
}

const FEATURES = [
  {
    icon: BookOpen,
    title: 'Structured Video Lessons',
    description: 'Modular, curriculum-aligned video content designed for Egyptian Baccalaureate Programming & AI.',
    color: 'text-primary',
    bg: 'bg-primary/10',
    border: 'border-primary/20',
  },
  {
    icon: Code2,
    title: 'Automated Code Judge',
    description: 'Submit solutions and get instant feedback from our Judge0-powered evaluation engine supporting C#, Python & JavaScript.',
    color: 'text-amber-500',
    bg: 'bg-amber-500/10',
    border: 'border-amber-500/20',
  },
  {
    icon: Brain,
    title: 'AI-Powered Exams',
    description: 'Adaptive assessments with rich question banks, auto-grading and detailed result analytics.',
    color: 'text-purple-500',
    bg: 'bg-purple-500/10',
    border: 'border-purple-500/20',
  },
  {
    icon: Video,
    title: 'Live Interactive Sessions',
    description: 'Join live Q&A sessions via Google Meet or MS Teams. All sessions are recorded for later review.',
    color: 'text-emerald-500',
    bg: 'bg-emerald-500/10',
    border: 'border-emerald-500/20',
  },
  {
    icon: Shield,
    title: 'Secure & Reliable',
    description: 'JWT + Refresh Token auth with 2FA, role-based access, and a 99.9% uptime SLA.',
    color: 'text-rose-500',
    bg: 'bg-rose-500/10',
    border: 'border-rose-500/20',
  },
  {
    icon: Globe,
    title: 'Bilingual Interface',
    description: 'Full Arabic RTL and English LTR support with instant language switching.',
    color: 'text-sky-500',
    bg: 'bg-sky-500/10',
    border: 'border-sky-500/20',
  },
];

const STATS = [
  { value: '10,000+', label: 'Active Students', icon: Users, color: 'text-primary' },
  { value: '500+', label: 'Code Challenges', icon: Code2, color: 'text-amber-500' },
  { value: '50+', label: 'Practical Courses', icon: BookOpen, color: 'text-emerald-500' },
  { value: '98%', label: 'Completion Rate', icon: Trophy, color: 'text-purple-500' },
];

const STEPS = [
  { step: '01', title: 'Create an Account', description: 'Sign up for free as a Student or Teacher in under 60 seconds.' },
  { step: '02', title: 'Choose a Course', description: 'Browse the structured catalog and subscribe to the plan that fits you.' },
  { step: '03', title: 'Learn & Code', description: 'Watch lessons, solve coding challenges, and join live sessions with instructors.' },
  { step: '04', title: 'Earn Certificates', description: 'Complete courses to earn verifiable certificates and level up your engineering career.' },
];

const TESTIMONIALS = [
  {
    name: 'Ahmed Hassan',
    role: 'Baccalaureate Student — Cairo',
    quote: 'Codean transformed my understanding of programming. The code judge gave me instant feedback and helped me practice for the exam.',
    rating: 5,
  },
  {
    name: 'Sara Mahmoud',
    role: 'CS Graduate — Alexandria',
    quote: 'The live sessions with expert instructors are unbeatable. I could ask questions in real-time and actually understand Clean Architecture.',
    rating: 5,
  },
  {
    name: 'Omar Khalil',
    role: 'Software Engineer — Giza',
    quote: 'I went from knowing nothing about .NET to building production apps. The structured curriculum and exams kept me on track.',
    rating: 5,
  },
];

export default async function LandingPage() {
  const t = await getTranslations('hero');
  const tCommon = await getTranslations('common');

  return (
    <div className="relative flex min-h-screen flex-col overflow-hidden bg-background">
      {/* ── Background Effects ── */}
      <div aria-hidden="true" className="pointer-events-none fixed inset-0 -z-10">
        <div className="absolute -top-60 left-1/2 h-[700px] w-[1000px] -translate-x-1/2 rounded-full bg-gradient-to-tr from-primary/20 via-accent/10 to-transparent blur-[120px]" />
        <div className="absolute bottom-0 right-0 h-[500px] w-[700px] rounded-full bg-gradient-to-tl from-purple-500/10 to-transparent blur-[100px]" />
        <div className="bg-dots absolute inset-0 opacity-40" />
      </div>

      {/* ── Sticky Header ── */}
      <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-background/80 backdrop-blur-xl">
        <div className="container mx-auto flex h-16 items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link href="/" className="flex items-center gap-3">
            <div
              aria-hidden="true"
              className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary text-white shadow-lg shadow-primary/30"
            >
              <Code2 className="h-5 w-5" />
            </div>
            <span className="text-lg font-black tracking-tight">{tCommon('appName')}</span>
          </Link>

          <nav className="hidden items-center gap-8 md:flex" aria-label="Main navigation">
            <Link href="/courses" className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground">
              Courses
            </Link>
            <Link href="/pricing" className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground">
              Pricing
            </Link>
          </nav>

          <div className="flex items-center gap-3">
            <Link
              href="/auth/login"
              className="hidden text-sm font-medium text-muted-foreground transition-colors hover:text-foreground sm:block"
            >
              {tCommon('login')}
            </Link>
            <Link
              href="/auth/register"
              className="inline-flex items-center justify-center gap-1.5 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-primary/25 transition-all hover:bg-primary/90 hover:shadow-primary/40 active:scale-95"
            >
              {tCommon('register')}
              <ArrowRight className="h-3.5 w-3.5" />
            </Link>
          </div>
        </div>
      </header>

      <main className="flex-1">
        {/* ── Hero Section ── */}
        <section
          className="container mx-auto flex flex-col items-center justify-center px-4 py-24 text-center sm:px-6 lg:px-8 lg:py-36"
          aria-labelledby="hero-heading"
        >
          <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-primary/25 bg-primary/10 px-4 py-1.5 text-xs font-semibold text-primary">
            <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
            <span>Egyptian Baccalaureate · Programming &amp; AI Platform</span>
          </div>

          <h1
            id="hero-heading"
            className="max-w-4xl text-5xl font-black tracking-tight sm:text-6xl lg:text-7xl"
          >
            <span className="gradient-text">{t('title')}</span>
          </h1>

          <p className="mt-6 max-w-2xl text-lg leading-relaxed text-muted-foreground sm:text-xl">
            {t('subtitle')}
          </p>

          <div className="mt-10 flex flex-col gap-4 sm:flex-row sm:gap-5">
            <Link
              href="/courses"
              id="cta-explore"
              className="inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-primary px-8 text-base font-semibold text-white shadow-xl shadow-primary/30 transition-all hover:bg-primary/90 hover:shadow-primary/50 active:scale-95"
            >
              {t('ctaStart')}
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/auth/login"
              id="cta-signin"
              className="inline-flex h-12 items-center justify-center rounded-xl border border-border bg-card px-8 text-base font-semibold text-foreground transition-all hover:bg-muted active:scale-95"
            >
              {t('ctaLogin')}
            </Link>
          </div>

          {/* Platform Stats */}
          <div className="mt-20 grid w-full max-w-4xl grid-cols-2 gap-5 lg:grid-cols-4">
            {STATS.map(({ icon: Icon, label, value, color }) => (
              <div
                key={label}
                className="card-hover flex flex-col items-center justify-center rounded-2xl border border-border/60 bg-card/70 p-6 backdrop-blur-sm"
              >
                <Icon className={`mb-3 h-7 w-7 ${color}`} aria-hidden="true" />
                <span className="tabular-nums text-3xl font-extrabold">{value}</span>
                <span className="mt-1 text-center text-xs text-muted-foreground">{label}</span>
              </div>
            ))}
          </div>
        </section>

        {/* ── Features Grid ── */}
        <section className="container mx-auto px-4 py-20 sm:px-6 lg:px-8" aria-labelledby="features-heading">
          <div className="mb-14 text-center">
            <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-accent/25 bg-accent/10 px-3.5 py-1.5 text-xs font-semibold text-accent">
              <Zap className="h-3.5 w-3.5" />
              Platform Features
            </div>
            <h2 id="features-heading" className="text-3xl font-black tracking-tight sm:text-4xl">
              Everything You Need to Excel
            </h2>
            <p className="mx-auto mt-4 max-w-2xl text-muted-foreground">
              A comprehensive learning environment built specifically for Egyptian Baccalaureate students and modern software engineers.
            </p>
          </div>

          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {FEATURES.map(({ icon: Icon, title, description, color, bg, border }) => (
              <div
                key={title}
                className={`card-hover rounded-2xl border ${border} ${bg} p-6 backdrop-blur-sm`}
              >
                <div className={`mb-4 flex h-12 w-12 items-center justify-center rounded-xl ${bg} ${color} border ${border}`}>
                  <Icon className="h-6 w-6" aria-hidden="true" />
                </div>
                <h3 className="mb-2 text-base font-bold">{title}</h3>
                <p className="text-sm leading-relaxed text-muted-foreground">{description}</p>
              </div>
            ))}
          </div>
        </section>

        {/* ── How It Works ── */}
        <section className="bg-card/60 py-20" aria-labelledby="how-it-works-heading">
          <div className="container mx-auto px-4 sm:px-6 lg:px-8">
            <div className="mb-14 text-center">
              <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-primary/25 bg-primary/10 px-3.5 py-1.5 text-xs font-semibold text-primary">
                <GraduationCap className="h-3.5 w-3.5" />
                Your Learning Journey
              </div>
              <h2 id="how-it-works-heading" className="text-3xl font-black tracking-tight sm:text-4xl">
                How It Works
              </h2>
            </div>

            <div className="relative grid grid-cols-1 gap-8 md:grid-cols-4">
              {/* Connecting line (desktop only) */}
              <div
                aria-hidden="true"
                className="absolute top-9 hidden h-px w-full bg-gradient-to-r from-primary/10 via-primary/40 to-primary/10 md:block"
              />
              {STEPS.map(({ step, title, description }) => (
                <div key={step} className="relative flex flex-col items-center text-center">
                  <div className="glow-primary relative z-10 mb-5 flex h-16 w-16 items-center justify-center rounded-2xl border border-primary/30 bg-card shadow-xl">
                    <span className="text-xl font-black text-primary">{step}</span>
                  </div>
                  <h3 className="mb-2 text-base font-bold">{title}</h3>
                  <p className="text-sm leading-relaxed text-muted-foreground">{description}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* ── Testimonials ── */}
        <section className="container mx-auto px-4 py-20 sm:px-6 lg:px-8" aria-labelledby="testimonials-heading">
          <div className="mb-14 text-center">
            <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-emerald-500/25 bg-emerald-500/10 px-3.5 py-1.5 text-xs font-semibold text-emerald-500">
              <Star className="h-3.5 w-3.5" />
              Student Reviews
            </div>
            <h2 id="testimonials-heading" className="text-3xl font-black tracking-tight sm:text-4xl">
              Loved by Students &amp; Engineers
            </h2>
          </div>

          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            {TESTIMONIALS.map(({ name, role, quote, rating }) => (
              <div
                key={name}
                className="card-hover flex flex-col justify-between rounded-2xl border border-border/60 bg-card p-7"
              >
                <div className="space-y-4">
                  <div className="flex gap-1">
                    {Array.from({ length: rating }).map((_, i) => (
                      <Star key={i} className="h-4 w-4 fill-amber-400 text-amber-400" aria-hidden="true" />
                    ))}
                  </div>
                  <p className="text-sm leading-relaxed text-muted-foreground">&ldquo;{quote}&rdquo;</p>
                </div>
                <div className="mt-6 flex items-center gap-3 border-t border-border/40 pt-5">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-sm font-bold text-white">
                    {name[0]}
                  </div>
                  <div>
                    <p className="text-sm font-bold">{name}</p>
                    <p className="text-xs text-muted-foreground">{role}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* ── CTA Banner ── */}
        <section className="container mx-auto px-4 pb-24 sm:px-6 lg:px-8">
          <div className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-primary to-primary/80 p-12 text-center shadow-2xl shadow-primary/20">
            <div aria-hidden="true" className="pointer-events-none absolute inset-0">
              <div className="absolute -left-20 -top-20 h-64 w-64 rounded-full bg-white/5 blur-3xl" />
              <div className="absolute -bottom-20 -right-20 h-64 w-64 rounded-full bg-white/5 blur-3xl" />
            </div>
            <div className="relative space-y-6">
              <div className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm font-semibold text-white backdrop-blur-sm">
                <CheckCircle className="h-4 w-4" />
                Join 10,000+ learners today
              </div>
              <h2 className="text-3xl font-black text-white sm:text-4xl">
                Ready to Master Programming &amp; AI?
              </h2>
              <p className="mx-auto max-w-xl text-white/80">
                Create your free account and start learning with structured lessons, automated code evaluation, and live instructor sessions.
              </p>
              <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
                <Link
                  href="/auth/register"
                  className="inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-white px-8 text-base font-semibold text-primary shadow-xl transition-all hover:bg-white/90 active:scale-95"
                >
                  Get Started Free
                  <ChevronRight className="h-4 w-4" />
                </Link>
                <Link
                  href="/courses"
                  className="inline-flex h-12 items-center justify-center rounded-xl border border-white/30 bg-white/10 px-8 text-base font-semibold text-white backdrop-blur-sm transition-all hover:bg-white/20 active:scale-95"
                >
                  Browse Courses
                </Link>
              </div>
            </div>
          </div>
        </section>
      </main>

      {/* ── Footer ── */}
      <footer className="border-t border-border/40 bg-card/50 py-12">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 gap-10 md:grid-cols-4">
            <div className="space-y-4 md:col-span-2">
              <div className="flex items-center gap-3">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary text-white">
                  <Code2 className="h-5 w-5" />
                </div>
                <span className="text-lg font-black">{tCommon('appName')}</span>
              </div>
              <p className="max-w-xs text-sm leading-relaxed text-muted-foreground">
                A production-grade e-learning platform for Egyptian Baccalaureate students and software engineers.
              </p>
            </div>
            <div className="space-y-3">
              <h4 className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Platform</h4>
              <ul className="space-y-2">
                {[
                  { label: 'Courses', href: '/courses' },
                  { label: 'Pricing', href: '/pricing' },
                  { label: 'Login', href: '/auth/login' },
                  { label: 'Register', href: '/auth/register' },
                ].map(({ label, href }) => (
                  <li key={label}>
                    <Link href={href} className="text-sm text-muted-foreground transition-colors hover:text-foreground">
                      {label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
            <div className="space-y-3">
              <h4 className="text-xs font-bold uppercase tracking-widest text-muted-foreground">Technology</h4>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>.NET 10 · Clean Architecture</li>
                <li>Next.js 15 · React 19</li>
                <li>PostgreSQL · Redis</li>
                <li>Judge0 Code Engine</li>
              </ul>
            </div>
          </div>

          <div className="mt-10 flex flex-col items-center justify-between gap-4 border-t border-border/40 pt-8 sm:flex-row">
            <p className="text-xs text-muted-foreground">
              © {new Date().getFullYear()} {tCommon('appName')} · Egyptian Baccalaureate Programming &amp; AI Platform
            </p>
            <p className="text-xs text-muted-foreground">Built with ❤️ using .NET 10 + Next.js 15</p>
          </div>
        </div>
      </footer>
    </div>
  );
}

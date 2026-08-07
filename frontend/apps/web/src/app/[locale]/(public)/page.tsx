import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import Link from 'next/link';
import { BookOpen, Code2, Sparkles, Trophy, Users } from 'lucide-react';

export async function generateMetadata({
  params,
}: {
  params: Promise<{ locale: string }>;
}): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'hero' });
  return {
    title: `Platform — ${t('title')}`,
    description: t('subtitle'),
    openGraph: {
      title: `Platform — ${t('title')}`,
      description: t('subtitle'),
      type: 'website',
    },
  };
}

export default async function LandingPage() {
  const t = await getTranslations('hero');
  const tCommon = await getTranslations('common');

  return (
    <div className="relative flex min-h-screen flex-col overflow-hidden">
      {/* Background Gradient Spotlights */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -top-40 left-1/2 -z-10 h-[500px] w-[800px] -translate-x-1/2 rounded-full bg-gradient-to-tr from-primary/30 to-accent/20 blur-[120px]"
      />

      {/* Header Navigation */}
      <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-background/80 backdrop-blur-md">
        <div className="container mx-auto flex h-16 items-center justify-between px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-3">
            <div
              aria-hidden="true"
              className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-white shadow-lg shadow-primary/25"
            >
              <Code2 className="h-6 w-6" />
            </div>
            <span className="text-xl font-bold tracking-tight">{tCommon('appName')}</span>
          </div>

          <nav className="flex items-center gap-4" aria-label="Main navigation">
            <Link
              href="/auth/login"
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              {tCommon('login')}
            </Link>
            <Link
              href="/auth/register"
              className="inline-flex items-center justify-center rounded-xl bg-primary px-4 py-2 text-sm font-medium text-white shadow-lg shadow-primary/25 transition-all hover:bg-primary/90 hover:shadow-primary/40 active:scale-95"
            >
              {tCommon('register')}
            </Link>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <main className="flex-1">
        <section
          className="container mx-auto flex flex-col items-center justify-center px-4 py-20 text-center sm:px-6 lg:px-8 lg:py-32"
          aria-labelledby="hero-heading"
        >
          <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-primary/20 bg-primary/10 px-4 py-1.5 text-xs font-semibold text-primary">
            <Sparkles className="h-4 w-4" aria-hidden="true" />
            <span>Interactive Software Engineering Platform</span>
          </div>

          <h1
            id="hero-heading"
            className="max-w-4xl text-4xl font-extrabold tracking-tight sm:text-6xl lg:text-7xl"
          >
            {t('title')}
          </h1>

          <p className="mt-6 max-w-2xl text-lg text-muted-foreground sm:text-xl">
            {t('subtitle')}
          </p>

          <div className="mt-10 flex flex-col gap-4 sm:flex-row sm:gap-6">
            <Link
              href="/courses"
              id="cta-explore"
              className="inline-flex h-12 items-center justify-center rounded-xl bg-primary px-8 text-base font-semibold text-white shadow-xl shadow-primary/25 transition-all hover:bg-primary/90 hover:shadow-primary/40 active:scale-95"
            >
              {t('ctaStart')}
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
          <div className="mt-20 grid w-full max-w-5xl grid-cols-2 gap-6 lg:grid-cols-4">
            {[
              { icon: BookOpen, label: 'Practical Courses', value: '50+', color: 'text-primary' },
              { icon: Code2, label: 'Code Challenges', value: '500+', color: 'text-accent' },
              { icon: Users, label: 'Active Students', value: '10,000+', color: 'text-emerald-500' },
              { icon: Trophy, label: 'Completion Rate', value: '98%', color: 'text-purple-500' },
            ].map(({ icon: Icon, label, value, color }) => (
              <div
                key={label}
                className="flex flex-col items-center justify-center rounded-2xl border border-border/60 bg-card/60 p-6 backdrop-blur-sm"
              >
                <Icon className={`mb-3 h-8 w-8 ${color}`} aria-hidden="true" />
                <span className="text-3xl font-extrabold">{value}</span>
                <span className="mt-1 text-center text-sm text-muted-foreground">{label}</span>
              </div>
            ))}
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t border-border/40 bg-background/50 py-8 text-center text-sm text-muted-foreground">
        <p>© {new Date().getFullYear()} {tCommon('appName')} — Programming Education Platform. All rights reserved.</p>
      </footer>
    </div>
  );
}

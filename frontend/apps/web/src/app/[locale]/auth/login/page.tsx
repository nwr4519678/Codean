'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Code2, ArrowRight, Loader2, Lock, Mail, Shield, CheckCircle } from 'lucide-react';
import { useLogin } from '@platform/api';
import { toast } from 'sonner';

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const loginMutation = useLogin();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await loginMutation.mutateAsync({ email, password });
      toast.success('Logged in successfully!');
      const role = res.user?.role;
      if (role === 'Teacher' || role === 'Admin') {
        router.push('/teacher/dashboard');
      } else {
        router.push('/student/dashboard');
      }
    } catch (err: any) {
      const errors = err?.response?.data?.errors;
      const detail = errors
        ? (Object.values(errors).flat() as string[]).join(' ')
        : err?.response?.data?.detail || err?.response?.data?.title || 'Invalid email or password';
      toast.error(detail);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4 sm:p-6 lg:p-8">
      <div className="grid w-full max-w-4xl overflow-hidden rounded-3xl border border-border/60 bg-card shadow-2xl lg:grid-cols-2">
        {/* Left Branding Panel */}
        <div className="relative hidden flex-col justify-between bg-gradient-to-br from-primary/90 via-primary to-primary/80 p-10 text-white lg:flex">
          <div aria-hidden="true" className="pointer-events-none absolute inset-0">
            <div className="absolute -left-20 -top-20 h-64 w-64 rounded-full bg-white/10 blur-3xl" />
            <div className="absolute -bottom-20 -right-20 h-64 w-64 rounded-full bg-white/10 blur-3xl" />
          </div>
          <div className="relative z-10 flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-white/20 backdrop-blur-md">
              <Code2 className="h-6 w-6 text-white" />
            </div>
            <span className="text-xl font-extrabold tracking-tight">Codean Platform</span>
          </div>

          <div className="relative z-10 space-y-4">
            <span className="inline-block rounded-full bg-white/10 px-3 py-1 text-xs font-semibold backdrop-blur-md">
              Egyptian Baccalaureate · Programming & AI
            </span>
            <h2 className="text-3xl font-black leading-tight">Welcome Back to Your Learning Workspace</h2>
            <p className="text-sm text-white/80">
              Access your enrolled courses, submit solutions to automated judge challenges, and track your progress in real time.
            </p>
          </div>

          <div className="relative z-10 space-y-2 border-t border-white/20 pt-6 text-xs text-white/70">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-400" />
              <span>Instant evaluation with Judge0 engine</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-400" />
              <span>Live virtual classes &amp; Q&amp;A sessions</span>
            </div>
          </div>
        </div>

        {/* Right Login Form */}
        <div className="flex flex-col justify-center p-8 sm:p-12">
          <div className="text-center lg:text-left">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-primary text-white shadow-lg shadow-primary/30 lg:mx-0">
              <Code2 className="h-7 w-7" />
            </div>
            <h2 className="mt-4 text-2xl font-bold tracking-tight">Sign In</h2>
            <p className="mt-1 text-sm text-muted-foreground">Enter your credentials to access your account</p>
          </div>

          <form onSubmit={handleSubmit} className="mt-8 space-y-5">
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Email Address</label>
              <div className="relative mt-2">
                <Mail className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="block w-full rounded-xl border border-input bg-background pl-10 pr-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between">
                <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Password</label>
                <Link href="/auth/forgot-password" className="text-xs font-semibold text-primary hover:underline">
                  Forgot password?
                </Link>
              </div>
              <div className="relative mt-2">
                <Lock className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="block w-full rounded-xl border border-input bg-background pl-10 pr-4 py-3 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loginMutation.isPending}
              className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary py-3.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 transition-all hover:bg-primary/90 hover:shadow-primary/40 active:scale-95 disabled:opacity-50"
            >
              {loginMutation.isPending ? (
                <Loader2 className="h-5 w-5 animate-spin" />
              ) : (
                <>
                  Sign In
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </button>
          </form>

          <p className="mt-8 text-center text-sm text-muted-foreground lg:text-left">
            Don&apos;t have an account?{' '}
            <Link href="/auth/register" className="font-semibold text-primary hover:underline">
              Create an account
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

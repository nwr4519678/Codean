'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Code2, ArrowRight, Loader2, Mail, Lock, User, CheckCircle, AlertCircle } from 'lucide-react';
import { useRegister } from '@platform/api';
import { toast } from 'sonner';

export default function RegisterPage() {
  const router = useRouter();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Student');
  const registerMutation = useRegister();

  // Simple password strength calculator
  const getPasswordStrength = (pass: string) => {
    if (!pass) return { score: 0, label: '', color: 'bg-muted' };
    let score = 0;
    if (pass.length >= 8) score++;
    if (/[A-Z]/.test(pass)) score++;
    if (/[0-9]/.test(pass)) score++;
    if (/[^A-Za-z0-9]/.test(pass)) score++;

    if (score <= 1) return { score: 25, label: 'Weak', color: 'bg-destructive' };
    if (score === 2) return { score: 50, label: 'Fair', color: 'bg-amber-500' };
    if (score === 3) return { score: 75, label: 'Good', color: 'bg-blue-500' };
    return { score: 100, label: 'Strong', color: 'bg-emerald-500' };
  };

  const strength = getPasswordStrength(password);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await registerMutation.mutateAsync({ firstName, lastName, email, password, role });
      toast.success('Account created successfully! Please sign in.');
      router.push('/auth/login');
    } catch (err: any) {
      const data = err?.response?.data;
      let detail = 'Registration failed';
      if (data?.errors && typeof data.errors === 'object') {
        detail = Object.values(data.errors).flat().join(' · ');
      } else if (data?.detail) {
        detail = data.detail;
      } else if (data?.title) {
        detail = data.title;
      }
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
              Join 10,000+ Learners
            </span>
            <h2 className="text-3xl font-black leading-tight">Start Your Software Engineering Journey</h2>
            <p className="text-sm text-white/80">
              Create your account to access Egyptian Baccalaureate Programming &amp; AI courses, interactive coding judges, and expert-led live classes.
            </p>
          </div>

          <div className="relative z-10 space-y-2 border-t border-white/20 pt-6 text-xs text-white/70">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-400" />
              <span>Full curriculum aligned with Egyptian Baccalaureate</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-emerald-400" />
              <span>Track progress with XP, certificates &amp; badges</span>
            </div>
          </div>
        </div>

        {/* Right Register Form */}
        <div className="flex flex-col justify-center p-8 sm:p-12">
          <div className="text-center lg:text-left">
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-2xl bg-primary text-white shadow-lg shadow-primary/30 lg:mx-0">
              <Code2 className="h-7 w-7" />
            </div>
            <h2 className="mt-4 text-2xl font-bold tracking-tight">Create Account</h2>
            <p className="mt-1 text-sm text-muted-foreground">Fill in your details to get started</p>
          </div>

          <form onSubmit={handleSubmit} className="mt-6 space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-foreground">First Name</label>
                <div className="relative mt-1">
                  <User className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <input
                    type="text"
                    required
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    placeholder="John"
                    className="block w-full rounded-xl border border-input bg-background pl-9 pr-3 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  />
                </div>
              </div>
              <div>
                <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Last Name</label>
                <div className="relative mt-1">
                  <User className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <input
                    type="text"
                    required
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    placeholder="Doe"
                    className="block w-full rounded-xl border border-input bg-background pl-9 pr-3 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                  />
                </div>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Email Address</label>
              <div className="relative mt-1">
                <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="block w-full rounded-xl border border-input bg-background pl-9 pr-3 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">Password</label>
              <div className="relative mt-1">
                <Lock className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="block w-full rounded-xl border border-input bg-background pl-9 pr-3 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                />
              </div>
              {password && (
                <div className="mt-2 space-y-1.5">
                  <div className="flex items-center justify-between text-[11px]">
                    <span className="text-muted-foreground">Strength:</span>
                    <span className="font-semibold">{strength.label}</span>
                  </div>
                  <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
                    <div className={`h-full transition-all duration-300 ${strength.color}`} style={{ width: `${strength.score}%` }} />
                  </div>
                  <div className="grid grid-cols-2 gap-x-2 gap-y-0.5 pt-1">
                    {[
                      { ok: password.length >= 8, label: 'Min 8 characters' },
                      { ok: /[A-Z]/.test(password), label: 'Uppercase letter' },
                      { ok: /[0-9]/.test(password), label: 'Number (0-9)' },
                      { ok: /[^a-zA-Z0-9]/.test(password), label: 'Special char (!@#...)' },
                    ].map(({ ok, label }) => (
                      <div key={label} className={`flex items-center gap-1 text-[10px] font-medium ${ok ? 'text-emerald-500' : 'text-muted-foreground'}`}>
                        <span>{ok ? '✓' : '○'}</span>
                        <span>{label}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-foreground">I am a</label>
              <select
                value={role}
                onChange={(e) => setRole(e.target.value)}
                className="mt-1 block w-full rounded-xl border border-input bg-background px-3 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              >
                <option value="Student">Student Learner</option>
                <option value="Teacher">Teacher / Instructor</option>
              </select>
            </div>

            <button
              type="submit"
              disabled={registerMutation.isPending}
              className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-primary/25 transition-all hover:bg-primary/90 hover:shadow-primary/40 active:scale-95 disabled:opacity-50 mt-4"
            >
              {registerMutation.isPending ? (
                <Loader2 className="h-5 w-5 animate-spin" />
              ) : (
                <>
                  Create Account
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-muted-foreground lg:text-left">
            Already have an account?{' '}
            <Link href="/auth/login" className="font-semibold text-primary hover:underline">
              Sign In
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

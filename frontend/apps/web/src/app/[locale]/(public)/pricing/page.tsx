'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { Check, Zap, Shield } from 'lucide-react';

export default function PricingPage() {
  const plans = [
    {
      name: 'Free Learner',
      price: '$0',
      description: 'Access to public courses and community challenges.',
      features: ['Access 5 Free Courses', 'Basic Code Judge (10 runs/day)', 'Community Q&A Forum', 'Standard Support'],
      cta: 'Get Started Free',
      href: '/auth/register',
      highlighted: false,
    },
    {
      name: 'Pro Student',
      price: '$29',
      period: '/month',
      description: 'Unlimited access to all courses, judge, and live sessions.',
      features: [
        'Unlimited Course Catalog Access',
        'Unlimited Code Judge Submissions',
        'Interactive Video Player & Certificates',
        'Live Instructor Q&A Sessions',
        'Priority Discord Support',
      ],
      cta: 'Start Pro Trial',
      href: '/checkout',
      highlighted: true,
    },
    {
      name: 'Enterprise / Team',
      price: '$99',
      period: '/month',
      description: 'Custom team seats, analytics dashboard, and private tracks.',
      features: [
        'Everything in Pro Student',
        '5 Team Member Seats Included',
        'Team Progress & Analytics Dashboard',
        'Custom Private Coding Challenges',
        'Dedicated Account Manager',
      ],
      cta: 'Contact Sales',
      href: '/auth/register',
      highlighted: false,
    },
  ];

  return (
    <div className="container mx-auto space-y-12 px-4 py-12 sm:px-6 lg:px-8">
      <PageHeader
        title="Flexible Pricing Plans"
        description="Choose the perfect tier for your learning goals. Upgrade or cancel anytime."
      />

      <div className="grid grid-cols-1 gap-8 md:grid-cols-3 max-w-6xl mx-auto">
        {plans.map((plan) => (
          <div
            key={plan.name}
            className={`flex flex-col justify-between rounded-3xl border p-8 transition-all ${
              plan.highlighted
                ? 'border-primary bg-primary/5 ring-2 ring-primary shadow-2xl scale-105'
                : 'border-border/70 bg-card shadow-sm hover:shadow-md'
            }`}
          >
            <div className="space-y-6">
              {plan.highlighted && (
                <span className="inline-flex items-center gap-1.5 rounded-full bg-primary px-3 py-1 text-xs font-bold text-white">
                  <Zap className="h-3.5 w-3.5" /> Most Popular
                </span>
              )}

              <div>
                <h3 className="text-2xl font-bold">{plan.name}</h3>
                <p className="text-xs text-muted-foreground mt-1">{plan.description}</p>
              </div>

              <div className="flex items-baseline gap-1">
                <span className="text-5xl font-extrabold">{plan.price}</span>
                {plan.period && <span className="text-sm text-muted-foreground">{plan.period}</span>}
              </div>

              <ul className="space-y-3 text-xs text-muted-foreground border-t border-border/40 pt-6">
                {plan.features.map((feat) => (
                  <li key={feat} className="flex items-center gap-2 font-medium text-foreground">
                    <Check className="h-4 w-4 text-emerald-500 shrink-0" />
                    <span>{feat}</span>
                  </li>
                ))}
              </ul>
            </div>

            <Link
              href={plan.href}
              className={`mt-8 flex w-full items-center justify-center rounded-xl px-6 py-3.5 text-sm font-semibold transition-all active:scale-95 ${
                plan.highlighted
                  ? 'bg-primary text-white shadow-lg shadow-primary/25 hover:bg-primary/90'
                  : 'bg-muted text-foreground hover:bg-muted/80'
              }`}
            >
              {plan.cta}
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}

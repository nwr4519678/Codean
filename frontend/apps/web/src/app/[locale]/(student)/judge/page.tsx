'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { useCodingChallenges } from '@platform/api';
import { Code2, Trophy, Clock, ArrowRight, Loader2 } from 'lucide-react';

export default function JudgeListPage() {
  const { data: challenges, isLoading } = useCodingChallenges();

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Automated Code Judge"
        description="Solve real-world coding challenges with automated unit test execution."
      />

      {isLoading ? (
        <div className="flex min-h-[300px] w-full items-center justify-center">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {(challenges || [
            {
              id: 1,
              title: 'Two Sum Problem',
              category: 'Algorithms',
              difficulty: 'Easy',
              points: 100,
              description: 'Find two numbers in an array that add up to a target sum.',
            },
            {
              id: 2,
              title: 'LRU Cache Implementation',
              category: 'Data Structures',
              difficulty: 'Medium',
              points: 250,
              description: 'Design and implement a Least Recently Used (LRU) cache.',
            },
            {
              id: 3,
              title: 'Distributed Rate Limiter',
              category: 'System Design',
              difficulty: 'Hard',
              points: 500,
              description: 'Implement a token bucket rate limiter with concurrency locks.',
            },
          ]).map((challenge) => (
            <div
              key={challenge.id}
              className="flex flex-col justify-between rounded-2xl border border-border/70 bg-card p-6 shadow-sm hover:border-primary/50 hover:shadow-lg transition-all"
            >
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-semibold ${
                      challenge.difficulty === 'Easy'
                        ? 'bg-emerald-500/10 text-emerald-500'
                        : challenge.difficulty === 'Medium'
                        ? 'bg-accent/10 text-accent'
                        : 'bg-destructive/10 text-destructive'
                    }`}
                  >
                    {challenge.difficulty}
                  </span>
                  <span className="flex items-center gap-1 text-xs font-bold text-amber-500">
                    <Trophy className="h-3.5 w-3.5" />
                    {challenge.points} XP
                  </span>
                </div>

                <h3 className="text-lg font-bold">{challenge.title}</h3>
                <p className="text-xs text-muted-foreground line-clamp-2">{challenge.description}</p>
              </div>

              <Link
                href={`/judge/${challenge.id}`}
                className="mt-6 flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90 transition-all"
              >
                Solve Challenge
                <ArrowRight className="h-3.5 w-3.5" />
              </Link>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { FilterBar } from '@/components/shared/FilterBar';
import { Code2, Trophy, ArrowRight, CheckCircle2, Star, Zap } from 'lucide-react';

const SAMPLE_CHALLENGES = [
  {
    id: 1,
    title: 'Two Sum Problem',
    category: 'Algorithms',
    difficulty: 'Easy',
    xp: 100,
    acceptanceRate: '78%',
    description: 'Find two numbers in an array that add up to a specific target value.',
    languages: ['C#', 'Python', 'JavaScript'],
    isSolved: true,
  },
  {
    id: 2,
    title: 'Binary Search Implementation',
    category: 'Data Structures',
    difficulty: 'Easy',
    xp: 120,
    acceptanceRate: '85%',
    description: 'Implement logarithm-time search algorithm on a sorted array.',
    languages: ['C#', 'Python'],
    isSolved: true,
  },
  {
    id: 3,
    title: 'Valid Parentheses Checker',
    category: 'Stack & Queue',
    difficulty: 'Medium',
    xp: 200,
    acceptanceRate: '62%',
    description: 'Determine if an input string containing brackets is valid using a stack data structure.',
    languages: ['C#', 'Python', 'JavaScript'],
    isSolved: false,
  },
  {
    id: 4,
    title: 'LRU Cache Design',
    category: 'System Design',
    difficulty: 'Hard',
    xp: 350,
    acceptanceRate: '41%',
    description: 'Design and implement a data structure for Least Recently Used (LRU) cache with O(1) ops.',
    languages: ['C#'],
    isSolved: false,
  },
  {
    id: 5,
    title: 'Merge K Sorted Lists',
    category: 'Algorithms',
    difficulty: 'Hard',
    xp: 400,
    acceptanceRate: '38%',
    description: 'Merge k sorted linked lists into one sorted linked list and return it.',
    languages: ['C#', 'Python'],
    isSolved: false,
  },
];

export default function JudgeListPage() {
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('all');

  const categories = ['Algorithms', 'Data Structures', 'Stack & Queue', 'System Design'];

  const filteredChallenges = SAMPLE_CHALLENGES.filter((c) => {
    const matchesSearch = c.title.toLowerCase().includes(search.toLowerCase()) || c.description.toLowerCase().includes(search.toLowerCase());
    const matchesCategory = category === 'all' || c.category === category;
    return matchesSearch && matchesCategory;
  });

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader
        title="Automated Code Judge"
        description="Solve real-world coding challenges with automated Judge0 execution engine."
      />

      <FilterBar
        searchValue={search}
        onSearchChange={setSearch}
        categories={categories}
        selectedCategory={category}
        onCategoryChange={setCategory}
        placeholder="Search coding challenges..."
      />

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
        {filteredChallenges.map((challenge) => (
          <div
            key={challenge.id}
            className="flex flex-col justify-between rounded-2xl border border-border/70 bg-card p-6 shadow-sm transition-all hover:border-primary/40 hover:shadow-lg"
          >
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span
                  className={`rounded-full px-3 py-0.5 text-xs font-bold ${
                    challenge.difficulty === 'Easy'
                      ? 'bg-emerald-500/10 text-emerald-500'
                      : challenge.difficulty === 'Medium'
                      ? 'bg-amber-500/10 text-amber-500'
                      : 'bg-rose-500/10 text-rose-500'
                  }`}
                >
                  {challenge.difficulty}
                </span>

                <div className="flex items-center gap-1.5 text-xs font-bold text-amber-500">
                  <Zap className="h-3.5 w-3.5 fill-current" />
                  <span>+{challenge.xp} XP</span>
                </div>
              </div>

              <div>
                <div className="flex items-center gap-2">
                  <h3 className="text-lg font-bold tracking-tight">{challenge.title}</h3>
                  {challenge.isSolved && <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-500" />}
                </div>
                <p className="mt-1 line-clamp-2 text-xs text-muted-foreground">{challenge.description}</p>
              </div>

              <div className="flex flex-wrap gap-1.5 pt-1">
                {challenge.languages.map((lang) => (
                  <span key={lang} className="rounded-md border border-border/60 bg-muted/50 px-2 py-0.5 text-[10px] font-mono font-medium">
                    {lang}
                  </span>
                ))}
              </div>
            </div>

            <div className="mt-6 flex items-center justify-between border-t border-border/40 pt-4">
              <span className="text-xs text-muted-foreground">Acc Rate: <strong className="text-foreground">{challenge.acceptanceRate}</strong></span>

              <Link
                href={`/judge/${challenge.id}`}
                className="inline-flex items-center gap-1.5 rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white shadow-md transition-all hover:bg-primary/90"
              >
                Solve Challenge
                <ArrowRight className="h-3.5 w-3.5" />
              </Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

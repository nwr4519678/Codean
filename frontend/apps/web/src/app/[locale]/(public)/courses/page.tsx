'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { FilterBar } from '@/components/shared/FilterBar';
import { BookOpen, Clock, Users, ArrowRight } from 'lucide-react';
import { useCourses } from '@platform/api';
import { CourseCardSkeleton } from '@/components/skeletons/CourseCardSkeleton';

export default function CourseCatalogPage() {
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('all');

  const { data: pagedCourses, isLoading } = useCourses({
    search: search || undefined,
    category: category !== 'all' ? category : undefined,
  });

  const categories = ['C# & .NET', 'Python', 'Web Development', 'System Design', 'DevOps'];

  return (
    <div className="container mx-auto space-y-8 px-4 py-12 sm:px-6 lg:px-8">
      <PageHeader
        title="Course Catalog"
        description="Master modern software engineering with hands-on, project-based video courses."
      />

      <FilterBar
        searchValue={search}
        onSearchChange={setSearch}
        categories={categories}
        selectedCategory={category}
        onCategoryChange={setCategory}
        placeholder="Search by course title or keyword..."
      />

      {isLoading ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <CourseCardSkeleton key={i} />
          ))}
        </div>
      ) : pagedCourses?.items && pagedCourses.items.length > 0 ? (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {pagedCourses.items.map((course) => (
            <div
              key={course.id}
              className="flex flex-col justify-between rounded-2xl border border-border/70 bg-card p-6 shadow-sm transition-all hover:border-primary/50 hover:shadow-lg"
            >
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
                    {course.category}
                  </span>
                  <span className="text-lg font-extrabold text-foreground">
                    ${course.price === 0 ? 'Free' : course.price}
                  </span>
                </div>

                <h3 className="text-xl font-bold tracking-tight">{course.title}</h3>
                <p className="line-clamp-2 text-sm text-muted-foreground">{course.description}</p>
              </div>

              <div className="mt-6 space-y-4 border-t border-border/40 pt-4">
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span className="flex items-center gap-1">
                    <BookOpen className="h-4 w-4 text-primary" />
                    {course.moduleCount} Modules
                  </span>
                  <span className="flex items-center gap-1">
                    <Clock className="h-4 w-4 text-accent" />
                    {course.lessonCount} Lessons
                  </span>
                  <span className="flex items-center gap-1">
                    <Users className="h-4 w-4 text-emerald-500" />
                    {course.teacherName}
                  </span>
                </div>

                <Link
                  href={`/courses/${course.id}`}
                  className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-sm font-semibold text-white shadow-md transition-all hover:bg-primary/90"
                >
                  View Details
                  <ArrowRight className="h-4 w-4" />
                </Link>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="flex min-h-[300px] flex-col items-center justify-center rounded-2xl border border-dashed border-border p-8 text-center">
          <BookOpen className="h-12 w-12 text-muted-foreground/50 mb-3" />
          <h3 className="text-lg font-bold">No courses found</h3>
          <p className="text-sm text-muted-foreground">Try searching with a different keyword or category.</p>
        </div>
      )}
    </div>
  );
}

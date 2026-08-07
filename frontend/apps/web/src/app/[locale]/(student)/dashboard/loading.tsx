import React from 'react';

export default function Loading() {
  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 animate-pulse">
      <div className="h-10 w-56 rounded-xl bg-muted" />
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-32 rounded-2xl border border-border/60 bg-card" />
        ))}
      </div>
      <div className="h-64 rounded-2xl border border-border/60 bg-card" />
    </div>
  );
}

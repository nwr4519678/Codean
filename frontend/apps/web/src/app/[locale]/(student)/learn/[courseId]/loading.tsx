import React from 'react';
import { Loader2 } from 'lucide-react';

export default function LearnLoading() {
  return (
    <div className="flex min-h-screen w-full items-center justify-center">
      <Loader2 className="h-8 w-8 animate-spin text-primary" />
    </div>
  );
}

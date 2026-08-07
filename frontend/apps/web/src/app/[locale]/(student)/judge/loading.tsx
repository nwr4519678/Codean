import React from 'react';
import { Loader2 } from 'lucide-react';

export default function JudgeLoading() {
  return (
    <div className="flex min-h-[500px] w-full flex-col items-center justify-center gap-4">
      <Loader2 className="h-8 w-8 animate-spin text-primary" />
      <p className="text-sm text-muted-foreground">Loading Judge challenges...</p>
    </div>
  );
}

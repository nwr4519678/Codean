'use client';

import React from 'react';
import { useParams } from 'next/navigation';
import { Video, Users, MessageSquare, Loader2 } from 'lucide-react';

export default function LiveSessionPlayerPage() {
  const params = useParams();
  const sessionId = params?.sessionId;

  return (
    <div className="flex h-[calc(100vh-4rem)] flex-col lg:flex-row overflow-hidden bg-background">
      {/* Main Video Area */}
      <div className="flex-1 flex flex-col space-y-4 p-6 overflow-y-auto">
        <div className="relative aspect-video w-full overflow-hidden rounded-3xl border border-border/80 bg-black shadow-2xl flex items-center justify-center">
          <div className="text-center space-y-3">
            <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/20 mx-auto">
              <Video className="h-8 w-8 text-primary" />
            </div>
            <p className="text-white text-sm font-semibold">Live Stream • Session #{sessionId}</p>
            <span className="inline-flex items-center gap-1.5 rounded-full bg-red-500 px-3 py-1 text-xs font-bold text-white">
              <span className="h-2 w-2 rounded-full bg-white animate-pulse" />
              LIVE
            </span>
          </div>
        </div>

        <div className="rounded-2xl border border-border/60 bg-card p-5 space-y-2">
          <h2 className="text-xl font-bold">System Design: Design Twitter at Scale</h2>
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span className="flex items-center gap-1"><Users className="h-3.5 w-3.5" /> 128 live attendees</span>
            <span>Instructor: Sarah Connor</span>
          </div>
        </div>
      </div>

      {/* Live Chat Sidebar */}
      <div className="w-80 border-l border-border/60 bg-card flex flex-col hidden lg:flex">
        <div className="flex items-center gap-2 border-b border-border/40 px-4 py-3">
          <MessageSquare className="h-4 w-4 text-primary" />
          <span className="text-sm font-bold">Live Chat</span>
          <span className="ml-auto text-xs text-muted-foreground">128 viewers</span>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {[
            { user: 'Alex M.', msg: 'Great explanation of sharding!', time: '2m' },
            { user: 'Aya T.', msg: 'Can you explain consistent hashing?', time: '1m' },
            { user: 'Omar K.', msg: 'This is amazing, thank you!', time: 'now' },
          ].map((chat, i) => (
            <div key={i} className="space-y-0.5">
              <div className="flex items-center gap-2">
                <span className="text-xs font-bold text-primary">{chat.user}</span>
                <span className="text-[10px] text-muted-foreground">{chat.time}</span>
              </div>
              <p className="text-xs text-foreground">{chat.msg}</p>
            </div>
          ))}
        </div>

        <div className="border-t border-border/40 p-3">
          <input
            type="text"
            placeholder="Ask a question..."
            className="w-full rounded-xl border border-input bg-background px-3 py-2 text-xs focus:border-primary focus:outline-none"
          />
        </div>
      </div>
    </div>
  );
}

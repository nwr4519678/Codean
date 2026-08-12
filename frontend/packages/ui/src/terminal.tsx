"use client";

import * as React from "react";
import { Terminal as TerminalIcon, CheckCircle2, XCircle, Clock, AlertTriangle, Play, RefreshCw, Copy, Check } from "lucide-react";
import { cn } from "./utils";
import { Badge } from "./badge";
import { Button } from "./button";

export type JudgeState = "Idle" | "Running" | "Accepted" | "WrongAnswer" | "TimeLimitExceeded" | "RuntimeError" | "CompilationError";

export interface TerminalProps {
  status?: JudgeState;
  stdout?: string;
  stderr?: string;
  executionTimeMs?: number;
  memoryKb?: number;
  onRun?: () => void;
  onSubmit?: () => void;
  className?: string;
}

export function Terminal({
  status = "Idle",
  stdout = "",
  stderr = "",
  executionTimeMs,
  memoryKb,
  onRun,
  onSubmit,
  className,
}: TerminalProps) {
  const [copied, setCopied] = React.useState(false);

  const copyOutput = () => {
    const textToCopy = stdout || stderr || "No output";
    navigator.clipboard.writeText(textToCopy);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const getStatusBadge = () => {
    switch (status) {
      case "Running":
        return (
          <Badge variant="outline" className="bg-amber-500/10 text-amber-400 border-amber-500/30 gap-1 animate-pulse">
            <RefreshCw className="w-3 h-3 animate-spin" /> Running Code...
          </Badge>
        );
      case "Accepted":
        return (
          <Badge variant="outline" className="bg-emerald-500/10 text-emerald-400 border-emerald-500/30 gap-1">
            <CheckCircle2 className="w-3.5 h-3.5" /> Accepted
          </Badge>
        );
      case "WrongAnswer":
        return (
          <Badge variant="outline" className="bg-rose-500/10 text-rose-400 border-rose-500/30 gap-1">
            <XCircle className="w-3.5 h-3.5" /> Wrong Answer
          </Badge>
        );
      case "TimeLimitExceeded":
        return (
          <Badge variant="outline" className="bg-amber-500/10 text-amber-400 border-amber-500/30 gap-1">
            <Clock className="w-3.5 h-3.5" /> Time Limit Exceeded
          </Badge>
        );
      case "RuntimeError":
      case "CompilationError":
        return (
          <Badge variant="outline" className="bg-red-500/10 text-red-400 border-red-500/30 gap-1">
            <AlertTriangle className="w-3.5 h-3.5" /> {status === "CompilationError" ? "Compilation Error" : "Runtime Error"}
          </Badge>
        );
      default:
        return (
          <Badge variant="outline" className="bg-slate-500/10 text-slate-400 border-slate-500/30">
            Ready
          </Badge>
        );
    }
  };

  return (
    <div className={cn("rounded-xl border border-border/80 bg-slate-950 font-mono text-xs overflow-hidden flex flex-col shadow-xl", className)}>
      {/* Terminal Header */}
      <div className="flex items-center justify-between px-4 py-2.5 bg-slate-900/90 border-b border-slate-800/80">
        <div className="flex items-center gap-2">
          <TerminalIcon className="w-4 h-4 text-primary" />
          <span className="text-slate-300 font-medium">Console Output</span>
          {getStatusBadge()}
        </div>

        <div className="flex items-center gap-3">
          {executionTimeMs !== undefined && (
            <span className="text-slate-400 text-[11px]">
              Time: <strong className="text-slate-200">{executionTimeMs}ms</strong>
            </span>
          )}
          {memoryKb !== undefined && (
            <span className="text-slate-400 text-[11px]">
              Mem: <strong className="text-slate-200">{(memoryKb / 1024).toFixed(1)}MB</strong>
            </span>
          )}

          <Button type="button" variant="ghost" size="icon" onClick={copyOutput} className="w-7 h-7 text-slate-400 hover:text-white hover:bg-slate-800">
            {copied ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
          </Button>

          {onRun && (
            <Button type="button" variant="secondary" size="sm" onClick={onRun} disabled={status === "Running"} className="h-7 text-xs gap-1.5 px-3 bg-slate-800 hover:bg-slate-700 text-slate-200">
              <Play className="w-3 h-3 text-emerald-400 fill-emerald-400" /> Run
            </Button>
          )}

          {onSubmit && (
            <Button type="button" variant="default" size="sm" onClick={onSubmit} disabled={status === "Running"} className="h-7 text-xs gap-1 px-3 bg-primary hover:bg-primary/90 text-white">
              Submit Solution
            </Button>
          )}
        </div>
      </div>

      {/* Terminal Body */}
      <div className="p-4 min-h-[120px] max-h-[220px] overflow-y-auto font-mono text-slate-300 space-y-2 leading-relaxed selection:bg-primary/30">
        {status === "Idle" && !stdout && !stderr && (
          <p className="text-slate-500 italic">Click "Run" to test your code against sample inputs...</p>
        )}

        {stdout && (
          <div className="space-y-1">
            <span className="text-slate-500 text-[10px] uppercase tracking-wider font-semibold">Standard Output</span>
            <pre className="text-slate-200 whitespace-pre-wrap font-mono">{stdout}</pre>
          </div>
        )}

        {stderr && (
          <div className="space-y-1 pt-1">
            <span className="text-rose-400 text-[10px] uppercase tracking-wider font-semibold">Standard Error</span>
            <pre className="text-rose-300 bg-rose-950/30 p-2.5 rounded border border-rose-900/40 whitespace-pre-wrap font-mono">{stderr}</pre>
          </div>
        )}
      </div>
    </div>
  );
}

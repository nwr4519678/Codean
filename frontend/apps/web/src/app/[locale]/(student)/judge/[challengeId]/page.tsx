'use client';

import React, { useState } from 'react';
import { useParams } from 'next/navigation';
import dynamic from 'next/dynamic';
import { useSubmitCode, useSubmissionStatus } from '@platform/api';
import { Play, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

// Lazy load Monaco Editor for zero impact on home bundle size
const MonacoEditor = dynamic(() => import('@monaco-editor/react'), {
  ssr: false,
  loading: () => (
    <div className="flex h-full w-full items-center justify-center bg-black/90 text-white text-xs">
      <Loader2 className="h-6 w-6 animate-spin text-primary mr-2" />
      Loading Code Editor...
    </div>
  ),
});

export default function JudgeChallengeRunnerPage() {
  const params = useParams();
  const challengeId = Number(params?.challengeId || 1);

  const [language, setLanguage] = useState<'csharp' | 'python' | 'javascript'>('csharp');
  const [code, setCode] = useState<string>(
    `public class Solution {\n    public int[] TwoSum(int[] nums, int target) {\n        // Write your algorithm solution here\n        return new int[0];\n    }\n}`
  );
  const [submissionId, setSubmissionId] = useState<number | null>(null);

  const submitMutation = useSubmitCode();
  const { data: executionResult, isLoading: isExecuting } = useSubmissionStatus(submissionId);

  const handleRunCode = async () => {
    try {
      toast.info('Submitting code to Judge execution sandbox...');
      const res = await submitMutation.mutateAsync({
        challengeId,
        sourceCode: code,
        language,
      });
      setSubmissionId(res.submissionId);
    } catch {
      toast.error('Code submission failed');
    }
  };

  return (
    <div className="flex h-[calc(100vh-4rem)] flex-col lg:flex-row overflow-hidden bg-background">
      {/* Problem Description Panel */}
      <div className="w-full lg:w-1/3 border-r border-border/60 p-6 overflow-y-auto space-y-6">
        <div className="space-y-2">
          <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
            Algorithms
          </span>
          <h1 className="text-2xl font-extrabold tracking-tight">Two Sum Problem</h1>
          <p className="text-xs text-muted-foreground">Difficulty: Easy • 100 XP Points</p>
        </div>

        <div className="space-y-3 text-sm text-foreground leading-relaxed border-t border-border/40 pt-4">
          <p>
            Given an array of integers <code>nums</code> and an integer <code>target</code>, return indices of the two numbers such that they add up to target.
          </p>
          <p className="text-xs text-muted-foreground">
            You may assume that each input would have exactly one solution, and you may not use the same element twice.
          </p>
        </div>

        {/* Test Cases Panel */}
        <div className="space-y-3 border-t border-border/40 pt-4">
          <h3 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Example Test Case</h3>
          <div className="rounded-xl border border-border/60 bg-muted/40 p-3 text-xs font-mono space-y-1">
            <p>Input: nums = [2,7,11,15], target = 9</p>
            <p>Output: [0,1]</p>
          </div>
        </div>
      </div>

      {/* Editor & Execution Panel */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Editor Toolbar */}
        <div className="flex items-center justify-between border-b border-border/60 bg-card px-4 py-2.5">
          <select
            value={language}
            onChange={(e: any) => setLanguage(e.target.value)}
            className="rounded-lg border border-input bg-background px-3 py-1 text-xs font-semibold"
          >
            <option value="csharp">C# (.NET 10)</option>
            <option value="python">Python 3.12</option>
            <option value="javascript">JavaScript (Node.js)</option>
          </select>

          <button
            onClick={handleRunCode}
            disabled={submitMutation.isPending || isExecuting}
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2 text-xs font-semibold text-white shadow-md hover:bg-primary/90 disabled:opacity-50"
          >
            {submitMutation.isPending || isExecuting ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <Play className="h-3.5 w-3.5 fill-current" />
                Run Code
              </>
            )}
          </button>
        </div>

        {/* Monaco Editor Container */}
        <div className="flex-1 min-h-[350px]">
          <MonacoEditor
            height="100%"
            language={language}
            theme="vs-dark"
            value={code}
            onChange={(val) => setCode(val || '')}
            options={{
              fontSize: 14,
              minimap: { enabled: false },
              scrollBeyondLastLine: false,
              automaticLayout: true,
            }}
          />
        </div>

        {/* Execution Output Result Panel */}
        <div className="h-44 border-t border-border/60 bg-card p-4 font-mono text-xs overflow-y-auto space-y-2">
          <div className="flex items-center justify-between border-b border-border/40 pb-2">
            <span className="font-bold text-muted-foreground">Console Output & Test Results</span>
            {executionResult && (
              <span className={`flex items-center gap-1 font-bold ${executionResult.status === 'Passed' ? 'text-emerald-500' : 'text-destructive'}`}>
                {executionResult.status === 'Passed' ? <CheckCircle className="h-4 w-4" /> : <AlertCircle className="h-4 w-4" />}
                {executionResult.status}
              </span>
            )}
          </div>

          {executionResult ? (
            <div className="space-y-1">
              <p>Test Cases Passed: {executionResult.passedTestCases} / {executionResult.totalTestCases}</p>
              {executionResult.compilerOutput && <p className="text-destructive">{executionResult.compilerOutput}</p>}
            </div>
          ) : (
            <p className="text-muted-foreground italic">Click &quot;Run Code&quot; to execute test cases.</p>
          )}
        </div>
      </div>
    </div>
  );
}

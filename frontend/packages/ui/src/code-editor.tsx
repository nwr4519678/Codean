"use client";

import * as React from "react";
import Editor, { OnMount } from "@monaco-editor/react";
import { cn } from "./utils";

export interface CodeEditorProps {
  value: string;
  onChange?: (value: string | undefined) => void;
  language?: string;
  readOnly?: boolean;
  theme?: "vs-dark" | "light";
  height?: string;
  className?: string;
}

export function CodeEditor({
  value,
  onChange,
  language = "python",
  readOnly = false,
  theme = "vs-dark",
  height = "400px",
  className,
}: CodeEditorProps) {
  const handleEditorMount: OnMount = (editor: any, monaco: any) => {
    // Custom Codean Monaco Dark Theme
    monaco.editor.defineTheme("codean-dark", {
      base: "vs-dark",
      inherit: true,
      rules: [
        { token: "comment", foreground: "64748B", fontStyle: "italic" },
        { token: "keyword", foreground: "818CF8", fontStyle: "bold" },
        { token: "string", foreground: "34D399" },
        { token: "number", foreground: "F59E0B" },
        { token: "function", foreground: "38BDF8" },
      ],
      colors: {
        "editor.background": "#0B0F19",
        "editor.foreground": "#F1F5F9",
        "editorCursor.foreground": "#818CF8",
        "editor.lineHighlightBackground": "#1E293B40",
        "editorLineNumber.foreground": "#475569",
        "editorLineNumber.activeForeground": "#94A3B8",
        "editor.selectionBackground": "#6366F135",
      },
    });

    monaco.editor.setTheme(theme === "vs-dark" ? "codean-dark" : "vs");
  };

  return (
    <div className={cn("overflow-hidden rounded-xl border border-border/80 shadow-lg bg-[#0B0F19]", className)}>
      <Editor
        height={height}
        language={language}
        value={value}
        onChange={onChange}
        onMount={handleEditorMount}
        options={{
          readOnly,
          minimap: { enabled: false },
          fontSize: 14,
          fontFamily: "'Fira Code', 'Cascadia Code', Consolas, monospace",
          lineNumbers: "on",
          scrollBeyondLastLine: false,
          automaticLayout: true,
          padding: { top: 12, bottom: 12 },
          smoothScrolling: true,
          cursorBlinking: "smooth",
          cursorSmoothCaretAnimation: "on",
          formatOnPaste: true,
        }}
      />
    </div>
  );
}

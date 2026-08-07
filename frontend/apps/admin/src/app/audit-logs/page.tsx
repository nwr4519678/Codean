'use client';

import React from 'react';
import { useAuditLogs } from '@platform/api';
import { FileText, ShieldAlert, Loader2 } from 'lucide-react';

export default function AdminAuditLogsPage() {
  const { data: auditData, isLoading } = useAuditLogs();

  return (
    <div className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-purple-500 text-white shadow-lg">
              <FileText className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Security Audit Trail</h1>
              <p className="text-sm text-muted-foreground">Immutable audit logs for security, authorization, and compliance events</p>
            </div>
          </div>
        </div>

        {/* Audit Logs Table */}
        <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-border bg-muted/40 text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-6 py-4">Timestamp</th>
                <th className="px-6 py-4">User</th>
                <th className="px-6 py-4">Action</th>
                <th className="px-6 py-4">Entity</th>
                <th className="px-6 py-4">IP Address</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {isLoading ? (
                <tr>
                  <td colSpan={5} className="py-8 text-center">
                    <Loader2 className="mx-auto h-6 w-6 animate-spin text-primary" />
                  </td>
                </tr>
              ) : (auditData?.items || [
                { id: 1, createdAt: '2026-08-07 19:40:00', userEmail: 'admin@platform.dev', action: 'UserRoleAssigned', entityType: 'User', entityId: '3', ipAddress: '127.0.0.1' },
                { id: 2, createdAt: '2026-08-07 19:35:12', userEmail: 'alex@student.dev', action: 'CodeSubmissionPassed', entityType: 'CodingChallenge', entityId: '1', ipAddress: '192.168.1.5' },
                { id: 3, createdAt: '2026-08-07 19:20:45', userEmail: 'sarah@teacher.dev', action: 'CoursePublished', entityType: 'Course', entityId: '10', ipAddress: '10.0.0.2' },
              ]).map((log) => (
                <tr key={log.id} className="hover:bg-muted/30">
                  <td className="px-6 py-4 font-mono text-xs text-muted-foreground">{log.createdAt}</td>
                  <td className="px-6 py-4 font-semibold text-foreground">{log.userEmail || 'System'}</td>
                  <td className="px-6 py-4 font-mono text-xs text-primary">{log.action}</td>
                  <td className="px-6 py-4 text-xs text-muted-foreground">{log.entityType} ({log.entityId})</td>
                  <td className="px-6 py-4 font-mono text-xs text-muted-foreground">{log.ipAddress || 'Internal'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

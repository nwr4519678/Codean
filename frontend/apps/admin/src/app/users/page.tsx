'use client';

import React, { useState } from 'react';
import { useAdminUsers, useAssignRole } from '@platform/api';
import { Users, Shield, CheckCircle, XCircle, Search, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export default function AdminUsersPage() {
  const [search, setSearch] = useState('');
  const { data: pagedUsers, isLoading } = useAdminUsers({ search: search || undefined });
  const assignRoleMutation = useAssignRole();

  const handleRoleChange = async (userId: number, role: string) => {
    try {
      await assignRoleMutation.mutateAsync({ userId, role });
      toast.success('User role updated successfully');
    } catch {
      toast.error('Failed to update role');
    }
  };

  return (
    <div className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-white shadow-lg">
              <Users className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">User Management</h1>
              <p className="text-sm text-muted-foreground">Manage platform access, roles, and user account status</p>
            </div>
          </div>
        </div>

        {/* Filter Input */}
        <div className="relative max-w-md">
          <Search className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search users by name or email..."
            className="w-full rounded-xl border border-input bg-card pl-10 pr-4 py-2.5 text-sm text-foreground focus:border-primary focus:outline-none"
          />
        </div>

        {/* Users Table */}
        <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-border bg-muted/40 text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-6 py-4">User</th>
                <th className="px-6 py-4">Role</th>
                <th className="px-6 py-4">Status</th>
                <th className="px-6 py-4">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="py-8 text-center">
                    <Loader2 className="mx-auto h-6 w-6 animate-spin text-primary" />
                  </td>
                </tr>
              ) : (pagedUsers?.items || [
                { id: 1, email: 'admin@platform.dev', firstName: 'System', lastName: 'Admin', role: 'Admin', isActive: true },
                { id: 2, email: 'teacher@platform.dev', firstName: 'Sarah', lastName: 'Connor', role: 'Teacher', isActive: true },
                { id: 3, email: 'student@platform.dev', firstName: 'Alex', lastName: 'Mercer', role: 'Student', isActive: true },
              ]).map((user) => (
                <tr key={user.id} className="hover:bg-muted/30">
                  <td className="px-6 py-4 font-medium">
                    <div>
                      <p className="font-bold">{user.firstName} {user.lastName}</p>
                      <p className="text-xs text-muted-foreground">{user.email}</p>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <select
                      value={user.role}
                      onChange={(e) => handleRoleChange(user.id, e.target.value)}
                      className="rounded-lg border border-input bg-background px-2.5 py-1 text-xs font-semibold"
                    >
                      <option value="Student">Student</option>
                      <option value="Teacher">Teacher</option>
                      <option value="Admin">Admin</option>
                    </select>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-semibold ${user.isActive ? 'bg-emerald-500/10 text-emerald-500' : 'bg-destructive/10 text-destructive'}`}>
                      {user.isActive ? <CheckCircle className="h-3 w-3" /> : <XCircle className="h-3 w-3" />}
                      {user.isActive ? 'Active' : 'Suspended'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <button className="text-xs font-semibold text-primary hover:underline">
                      View Profile
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

'use client';

import React, { useState } from 'react';
import { TeacherSidebar } from '@/components/layout/TeacherSidebar';
import { DashboardHeader } from '@/components/layout/DashboardHeader';
import { RoleGuard } from '@/guards/RoleGuard';
import { UserRole } from '@platform/contracts';

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <RoleGuard allowedRoles={[UserRole.Teacher, UserRole.Admin]}>
    <div className="flex min-h-screen bg-background">
      <TeacherSidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <DashboardHeader onMenuToggle={() => setSidebarOpen(true)} />
        <main className="flex-1 overflow-y-auto">{children}</main>
      </div>
    </div>
    </RoleGuard>
  );
}

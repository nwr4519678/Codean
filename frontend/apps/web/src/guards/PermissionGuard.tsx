'use client';

import React from 'react';
import { useCurrentUser } from '@platform/api';

interface PermissionGuardProps {
  permission: string;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function PermissionGuard({ permission, children, fallback = null }: PermissionGuardProps) {
  const { data: user } = useCurrentUser();

  if (!user || !user.permissions?.includes(permission)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}

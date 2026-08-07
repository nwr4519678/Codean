'use client';

import React from 'react';
import { FEATURE_FLAGS, FeatureFlags } from '@platform/config';

interface FeatureGuardProps {
  flag: keyof FeatureFlags;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function FeatureGuard({ flag, children, fallback = null }: FeatureGuardProps) {
  const isEnabled = FEATURE_FLAGS[flag];

  if (!isEnabled) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}

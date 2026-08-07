import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  reactStrictMode: true,
  transpilePackages: [
    '@platform/ui',
    '@platform/api',
    '@platform/config',
    '@platform/contracts',
    '@platform/design-system',
  ],
};

export default nextConfig;

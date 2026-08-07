import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';

const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts');

const ANALYZE = process.env.ANALYZE === 'true';

let nextConfig: NextConfig = {
  reactStrictMode: true,
  transpilePackages: [
    '@platform/ui',
    '@platform/api',
    '@platform/config',
    '@platform/contracts',
    '@platform/design-system',
  ],
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: 'images.unsplash.com' },
      { protocol: 'https', hostname: 'plus.unsplash.com' },
    ],
  },
  // Bundle size optimization — tree-shake known heavy libs
  experimental: {
    optimizePackageImports: ['lucide-react', '@radix-ui/react-icons'],
  },
};

if (ANALYZE) {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const withBundleAnalyzer = require('@next/bundle-analyzer')({ enabled: true });
  nextConfig = withBundleAnalyzer(nextConfig);
}

export default withNextIntl(nextConfig);

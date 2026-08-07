import { MetadataRoute } from 'next';

export default function robots(): MetadataRoute.Robots {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://platform.dev';

  return {
    rules: {
      userAgent: '*',
      allow: '/',
      disallow: ['/dashboard/', '/learn/', '/judge/', '/teacher/', '/checkout/'],
    },
    sitemap: `${baseUrl}/sitemap.xml`,
  };
}

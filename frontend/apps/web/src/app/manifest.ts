import { MetadataRoute } from 'next';

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: 'Platform — Software Engineering Platform',
    short_name: 'Platform',
    description: 'Master Modern Software Engineering with interactive coding, video courses, and code judging.',
    start_url: '/',
    display: 'standalone',
    background_color: '#090d16',
    theme_color: '#4f46e5',
    icons: [
      {
        src: '/favicon.ico',
        sizes: 'any',
        type: 'image/x-icon',
      },
    ],
  };
}

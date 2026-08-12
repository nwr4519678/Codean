export const TOKENS = {
  colors: {
    primary: {
      DEFAULT: 'hsl(245, 80%, 60%)',
      foreground: 'hsl(0, 0%, 100%)',
    },
    accent: {
      DEFAULT: 'hsl(38, 92%, 58%)',
      foreground: 'hsl(0, 0%, 100%)',
    },
    background: {
      dark: 'hsl(224, 50%, 5%)',
      light: 'hsl(210, 20%, 98%)',
    },
    surface: {
      dark: 'hsl(224, 50%, 8%)',
      light: 'hsl(0, 0%, 100%)',
    },
    border: {
      dark: 'hsl(224, 40%, 16%)',
      light: 'hsl(220, 13%, 91%)',
    },
  },
  typography: {
    fontSans: "'Inter', sans-serif",
    fontDisplay: "'Geist', sans-serif",
    fontArabic: "'Noto Sans Arabic', sans-serif",
  },
  borderRadius: {
    sm: '0.375rem',
    md: '0.5rem',
    lg: '0.75rem',
    xl: '1rem',
    '2xl': '1.5rem',
  },
} as const;

export const MOTION_PRESETS = {
  pageEnter: {
    initial: { opacity: 0, y: 8 },
    animate: { opacity: 1, y: 0 },
    exit: { opacity: 0, y: -8 },
    transition: { duration: 0.25, ease: 'easeOut' },
  },
  fadeIn: {
    initial: { opacity: 0 },
    animate: { opacity: 1 },
    transition: { duration: 0.2 },
  },
  fadeUp: {
    initial: { opacity: 0, y: 16 },
    animate: { opacity: 1, y: 0 },
    transition: { duration: 0.3, ease: [0.25, 0.1, 0.25, 1] },
  },
  slideIn: {
    initial: { opacity: 0, x: -16 },
    animate: { opacity: 1, x: 0 },
    transition: { duration: 0.3, ease: 'easeOut' },
  },
  scaleIn: {
    initial: { opacity: 0, scale: 0.95 },
    animate: { opacity: 1, scale: 1 },
    transition: { duration: 0.2, ease: 'easeOut' },
  },
  staggerContainer: {
    animate: {
      transition: {
        staggerChildren: 0.08,
      },
    },
  },
  cardHover: {
    rest: { scale: 1, y: 0 },
    hover: { scale: 1.015, y: -2, transition: { duration: 0.2, ease: 'easeInOut' } },
  },
  buttonPress: {
    tap: { scale: 0.97 },
  },
} as const;


export const TOKENS = {
  colors: {
    primary: {
      DEFAULT: 'hsl(244, 76%, 60%)',
      foreground: 'hsl(0, 0%, 100%)',
    },
    accent: {
      DEFAULT: 'hsl(38, 92%, 58%)',
      foreground: 'hsl(0, 0%, 100%)',
    },
    background: {
      dark: 'hsl(224, 71%, 4%)',
      light: 'hsl(0, 0%, 98%)',
    },
    surface: {
      dark: 'hsl(224, 71%, 7%)',
      light: 'hsl(0, 0%, 100%)',
    },
    border: {
      dark: 'hsl(224, 71%, 12%)',
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

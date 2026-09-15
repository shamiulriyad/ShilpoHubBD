<<<<<<< HEAD
const cssColor = (name) => `rgb(var(--color-${name}) / <alpha-value>)`;

=======
/** @type {import('tailwindcss').Config} */
>>>>>>> Riyad
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],

  theme: {
    extend: {
      colors: {
<<<<<<< HEAD
        background: cssColor('background'),
        surface: cssColor('surface'),
        border: cssColor('border'),
        muted: cssColor('muted'),
        body: cssColor('body'),
        title: cssColor('title'),
        heading: cssColor('heading'),
        primary: cssColor('primary'),
        'primary-dark': cssColor('primary-dark'),
        'primary-soft': cssColor('primary-soft'),
        link: cssColor('link'),
        success: cssColor('success'),
        error: cssColor('error'),
=======
        background: "var(--color-background)",
        surface: "var(--color-surface)",
        title: "var(--color-title)",
        heading: "var(--color-heading)",
        body: "var(--color-body)",
        primary: "var(--color-primary)",
        secondary: "var(--color-secondary)",
        link: "var(--color-link)",
        border: "var(--color-border)",
        success: "var(--color-success)",
>>>>>>> Riyad
      },
    },
  },

  plugins: [],
};

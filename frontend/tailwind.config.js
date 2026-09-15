/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],

  theme: {
    extend: {
      colors: {
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
      },
    },
  },

  plugins: [],
};
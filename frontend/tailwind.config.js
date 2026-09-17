const cssColor = (name) => `rgb(var(--color-${name}) / <alpha-value>)`;

export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
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
      },
    },
  },
  plugins: [],
};

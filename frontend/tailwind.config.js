export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        // Neutral scale — does 90% of the work (text, backgrounds, borders)
        background: '#F8F5EF',
        surface:    '#FFFEFB',
        border:     '#E7DED0',
        muted:      '#746F67',
        body:       '#373832',
        title:      '#173B35',
        heading:    '#173B35',

        // Single brand color — this IS ShilpoHub. Everything else defers to it.
        primary:       '#A84F2D',
        'primary-dark':'#843A20',
        'primary-soft':'#F8E9E2',

        // Functional colors — used sparingly, only for their specific job
        link:    '#1E6056',
        success: '#32735D',
        error:   '#B3432B',
      },
    },
  },
  plugins: [],
};
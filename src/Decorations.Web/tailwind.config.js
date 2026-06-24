/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: 'class',
    content: [
        './Views/**/*.cshtml',
        './Areas/**/*.cshtml'
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    50:  '#fdf4ff',
                    100: '#fae8ff',
                    200: '#f5d0fe',
                    300: '#f0abfc',
                    400: '#e879f9',
                    500: '#d946ef',
                    600: '#c026d3',
                    700: '#a21caf',
                    800: '#86198f',
                    900: '#701a75',
                    950: '#3b0764',
                },
                gold: {
                    100: '#fef9e7',
                    200: '#fdf0c0',
                    300: '#f7d97a',
                    400: '#e8b84b',
                    500: '#c9902a',
                    600: '#a87020',
                },
                cream: {
                    50:  '#fdf9f5',
                    100: '#faf3ec',
                    200: '#f5e8d8',
                    300: '#ecd5bc',
                },
            },
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
                display: ['"Playfair Display"', 'Georgia', 'serif'],
            },
        },
    },
    plugins: [],
}

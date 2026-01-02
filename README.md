# Insurance Loom - Landing Page

A modern, responsive landing page for Insurance Loom, built with HTML, CSS, and JavaScript.

## Features

- Dark theme with white/grey text
- Fully responsive design
- Smooth scrolling navigation
- Contact form
- Mobile-friendly menu
- Modern UI/UX

## Deployment

This project is configured for AWS Amplify deployment.

### AWS Amplify Setup

1. Push this repository to GitHub/GitLab/Bitbucket
2. Go to [AWS Amplify Console](https://console.aws.amazon.com/amplify/)
3. Click "New app" → "Host web app"
4. Connect your Git repository
5. Amplify will auto-detect the build settings from `amplify.yml`
6. Click "Save and deploy"

### Custom Domain Setup

1. In Amplify Console → App settings → Domain management
2. Click "Add domain"
3. Enter `insuranceloom.com`
4. Follow the DNS configuration steps
5. Update DNS records in Namecheap as instructed by Amplify

## Local Development

Simply open `index.html` in a web browser or use a local server:

```bash
# Using Python
python -m http.server 8000

# Using Node.js (http-server)
npx http-server
```

Then visit `http://localhost:8000`

## File Structure

```
insurance_loom/
├── index.html      # Main HTML file
├── styles.css      # Stylesheet
├── script.js       # JavaScript functionality
├── amplify.yml     # AWS Amplify build configuration
└── README.md       # This file
```

## License

© 2024 Insurance Loom. All rights reserved.


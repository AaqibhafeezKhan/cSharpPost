# CSharpPost

A modern C# microblogging platform built with .NET and Razor Pages, deployed on Vercel.

## Features

- Create posts with text (140 character limit)
- Timeline display with pagination
- Search posts by keywords
- Delete posts functionality
- Responsive design with C# branding
- No authentication required for simplicity
- **Self-sustaining**: Works immediately without any setup

## Tech Stack

- **Frontend**: .NET 9 Razor Pages
- **Backend**: .NET 8 Web API
- **Storage**: In-memory storage (no database required)
- **Deployment**: Vercel with .NET support
- **Zero dependencies**: Everything runs out of the box

## Deployment Instructions

### Vercel Deployment (One-Click Deploy)

1. **Connect Repository**: Connect your GitHub repository to Vercel
2. **Deploy**: That's it! Vercel will automatically build and deploy the application

**No environment variables needed**
**No database setup required**
**Works immediately upon deployment**

### Local Development

1. **Clone the repository**
2. **Run the application**:
   ```bash
   dotnet run --project CSharpPost.API
   dotnet run --project CSharpPost.Frontend
   ```

## Project Structure

```
CSharpPost/
├── CSharpPost.API/           # Backend API with in-memory storage
├── CSharpPost.Frontend/      # Razor Pages frontend
├── CSharpPost.Core/          # Core business logic
├── CSharpPost.Data/          # Data access layer
├── CSharpPost.Tests/         # Unit tests
├── vercel.json              # Vercel deployment config
└── CSharpPost.sln           # Solution file
```

## Environment Variables

None required! The application is completely self-sustaining.

## Features Usage

- **Create Post**: Enter text (max 140 chars)
- **Search**: Use the search bar to filter posts by keywords
- **Delete**: Click the delete button on any post to remove it
- **Pagination**: Navigate through pages of posts using Previous/Next buttons

## Deployment Ready

This application is fully configured for Vercel deployment with:
- No AWS dependencies
- No database required
- No environment variables needed
- In-memory storage that works immediately
- Production-ready configuration
- Zero setup required

# Project Hive - Web Application

A modern, responsive web application for managing and browsing movie collections with secure authentication and an intuitive dashboard.

## 🚀 Features

### Authentication System
- 🔐 **Secure Login**: JWT-based authentication with Project Hive API
- 💾 **Session Management**: Smart token storage with automatic expiration handling
- 🔄 **Auto-redirect**: Seamless navigation based on authentication status
- 🚨 **Error Handling**: Comprehensive error messages and retry mechanisms

### Movies Dashboard
- � **Movie Collection**: Browse and manage your movie library
- 📊 **Statistics**: Real-time stats on collection size and metrics
- � **Search & Filter**: Find movies quickly with live search
- 📱 **Responsive Views**: Switch between grid and list layouts
- 📄 **Pagination**: Efficient browsing of large collections
- � **Movie Details**: Detailed view with file information and metadata

### UI/UX
- 🎨 **Modern Design**: Clean interface with gradient backgrounds
- 📱 **Mobile Responsive**: Works seamlessly across all device sizes
- ♿ **Accessibility**: Keyboard navigation and screen reader support
- ⚡ **Performance**: Optimized loading and smooth transitions

## 📁 File Structure

```
/web/
├── index.html              # Main entry point (auto-redirects)
├── login.html              # Authentication page
├── dashboard.html          # Main dashboard interface
├── styles.css              # Login page styles
├── script.js               # Login page logic
├── dashboard-styles.css    # Dashboard styles
├── dashboard-script.js     # Dashboard logic and API integration
├── test-api.html          # API connectivity testing tool
├── test-login.html        # Login testing (CORS troubleshooting)
├── standalone.html        # Self-contained login page
└── README.md              # This documentation
```

## 🔧 API Integration

### Authentication API
**Endpoint**: `POST http://localhost:5188/auth/login`

**Request**:
```json
{
  "username": "string",
  "password": "string"
}
```

**Response**:
```json
{
  "accessToken": "string",
  "tokenType": "string", 
  "expiresIn": number
}
```

### Movies API
**Endpoint**: `POST http://localhost:5154/v1/api/movies`

**Headers**:
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Request**:
```json
{
  "pageNumber": 1,
  "pageSize": 25
}
```

**Response**:
```json
{
  "movies": [
    {
      "id": "uuid",
      "name": "string",
      "filePath": "string", 
      "fileSize": number,
      "subTitleFilePath": "string",
      "image": "string"
    }
  ],
  "pageSize": number,
  "pageNumber": number
}
```

## 🚀 Quick Start

1. **Start your backend services**:
   - Authentication API on `localhost:5188`
   - Movies API on `localhost:5154`

2. **Serve the web application**:
   ```bash
   cd /path/to/project-hive/src/web
   python3 -m http.server 8000
   ```

3. **Open in browser**:
   ```
   http://localhost:8000
   ```

4. **Login and explore**:
   - Enter your credentials
   - Browse your movie collection
   - Use search and filtering features

## 🔒 Authentication Flow

1. **Entry Point**: `index.html` checks authentication status
2. **Login**: Redirects to `login.html` if not authenticated  
3. **Dashboard**: Redirects to `dashboard.html` after successful login
4. **Token Management**: Automatic token refresh and expiration handling
5. **Logout**: Clears tokens and returns to login

## 🎛️ Dashboard Features

### Navigation
- **Header**: Shows app branding and user controls
- **Logout Button**: Secure session termination

### Statistics Panel
- **Total Movies**: Count of movies in collection
- **Total Size**: Combined file size of all movies  
- **Last Sync**: Timestamp of last data refresh

### Controls
- **Search Box**: Real-time movie filtering
- **Page Size**: Adjust items per page (10, 25, 50, 100)
- **Refresh Button**: Manual data reload
- **View Toggle**: Switch between grid and list views

### Movie Display
- **Grid View**: Card-based layout with movie posters
- **List View**: Compact table-like format
- **Movie Details**: Click any movie for detailed information
- **Pagination**: Navigate through large collections

## 🛠️ Development

### Prerequisites
- Backend APIs running on specified ports
- Modern web browser
- Local web server (Python, Node.js, or similar)

### CORS Configuration
For development, ensure your backend APIs have CORS enabled:

```csharp
// ASP.NET Core example
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowLocalhost");
```

### Debugging Tools
- **test-api.html**: Test API connectivity and CORS
- **test-login.html**: Troubleshoot authentication issues
- **Browser DevTools**: Check console for detailed logging

## 📱 Responsive Design

The application is fully responsive with breakpoints for:
- **Desktop**: Full feature set with side-by-side layouts
- **Tablet**: Adapted layouts with touch-friendly controls  
- **Mobile**: Stacked layouts and simplified navigation

## 🔧 Customization

### Styling
- Modify CSS variables for colors and spacing
- Update gradient backgrounds in both stylesheets
- Customize component layouts and animations

### API Endpoints
Update the base URLs in JavaScript files:
```javascript
// In dashboard-script.js
const response = await fetch('http://your-domain:port/v1/api/movies', {
```

### Branding
- Update page titles and headers
- Replace logo and favicon
- Modify welcome messages and labels

## 🚀 Production Deployment

1. **Build Process**:
   - Minify CSS and JavaScript files
   - Optimize images and assets
   - Configure proper MIME types

2. **Security**:
   - Enable HTTPS
   - Configure CSP headers
   - Remove debug logging
   - Set secure token storage

3. **Performance**:
   - Enable gzip compression
   - Set proper cache headers
   - Optimize API response sizes
   - Implement service workers for offline support

## 🐛 Troubleshooting

### Common Issues

**"Load failed" errors**:
- Check if backend APIs are running
- Verify CORS configuration
- Use test-api.html for diagnosis

**Authentication not working**:
- Confirm API endpoints are correct
- Check network tab in browser DevTools
- Verify token format and expiration

**Movies not loading**:
- Ensure both authentication and movies APIs are running
- Check authorization headers
- Verify API response format matches expected structure

### Debug Mode
Enable debug logging by opening browser console and running:
```javascript
localStorage.setItem('debug', 'true');
```

## 📄 License

This code is part of the Project Hive application.
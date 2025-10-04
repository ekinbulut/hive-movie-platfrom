# Project Hive - Web Application

A modern, responsive web application for managing and browsing movie collections with secure authentication and an intuitive dashboard.

## 📁 Project Structure

```
/web/
├── index.html                    # Main entry point (auto-redirects based on auth)
├── README.md                     # Project documentation
├── assets/                       # Static assets
│   ├── css/                      # Stylesheets
│   │   ├── styles.css            # Login page styles
│   │   └── dashboard-styles.css  # Dashboard styles
│   └── js/                       # JavaScript files
│       ├── script.js             # Login page logic
│       └── dashboard-script.js   # Dashboard logic and API integration
├── pages/                        # HTML pages
│   ├── login.html                # Authentication page
│   └── dashboard.html            # Main dashboard interface
└── tools/                        # Development and testing tools
    ├── test-api.html             # API connectivity testing tool
    ├── test-login.html           # Login testing (CORS troubleshooting)
    └── standalone.html           # Self-contained login page for testing
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

## 🔧 File Organization

### **Entry Point**
- **`index.html`**: Smart landing page that checks authentication and redirects appropriately

### **Core Pages** (`pages/`)
- **`login.html`**: User authentication interface with form validation and error handling
- **`dashboard.html`**: Main application dashboard for browsing movies

### **Assets** (`assets/`)
- **CSS (`assets/css/`)**:
  - `styles.css`: Login page styling with modern gradients and responsive design
  - `dashboard-styles.css`: Dashboard styling with grid/list views and modal components

- **JavaScript (`assets/js/`)**:
  - `script.js`: Login functionality, session management, and API authentication
  - `dashboard-script.js`: Dashboard controller, movie API integration, and UI interactions

### **Development Tools** (`tools/`)
- **`test-api.html`**: Diagnostic tool for testing API connectivity and CORS configuration
- **`test-login.html`**: Authentication testing with fallback methods for troubleshooting
- **`standalone.html`**: Self-contained login page with embedded CSS/JS for testing without server

## 🔗 File Dependencies

### **HTML Pages**
```
login.html
├── ../assets/css/styles.css
└── ../assets/js/script.js

dashboard.html
├── ../assets/css/dashboard-styles.css
└── ../assets/js/dashboard-script.js

index.html
└── pages/login.html | pages/dashboard.html (redirects)
```

### **JavaScript Modules**
```
script.js (Login)
├── SessionManager class
├── AuthAPI class
└── UI event handlers

dashboard-script.js (Dashboard)
├── SessionManager class (shared)
├── MoviesAPI class
├── Utils class
└── DashboardController class
```

## 🛠️ Development Workflow

### **Adding New Features**
1. **Pages**: Add new HTML files to `pages/` folder
2. **Styles**: Create corresponding CSS files in `assets/css/`
3. **Logic**: Add JavaScript files to `assets/js/`
4. **Update References**: Ensure correct relative paths in HTML files

### **Testing**
1. **Use Development Tools**: Leverage files in `tools/` folder for debugging
2. **API Testing**: Use `test-api.html` for connectivity issues
3. **Authentication**: Use `test-login.html` for login troubleshooting
4. **Standalone Testing**: Use `standalone.html` for isolated testing

### **Deployment**
1. **Build Process**: Minify CSS/JS files in `assets/` folders
2. **Path Verification**: Ensure all relative paths work correctly
3. **Asset Optimization**: Compress images and optimize loading

## 🔒 Security Considerations

- **Token Storage**: Implemented in `SessionManager` class (both JS files)
- **CORS Handling**: Configure backend APIs for `localhost:8000` origin
- **Path Traversal**: All paths are relative and contained within project structure
- **Sensitive Data**: No credentials stored in client-side code

## 📱 Responsive Design

All CSS files include responsive breakpoints:
- **Desktop**: Full feature set (1200px+)
- **Tablet**: Adapted layouts (768px-1199px)
- **Mobile**: Optimized for touch (< 768px)

## 🔧 Configuration

### **API Endpoints** (in JavaScript files):
- Authentication: `http://localhost:5188/auth/login`
- Movies: `http://localhost:5154/v1/api/movies`

### **Development Server**:
- Default port: `8000`
- CORS origin: `http://localhost:8000`

## 📝 Notes

- **Modular Design**: Clear separation of concerns with organized folder structure
- **Maintainable**: Easy to locate and modify specific functionality
- **Scalable**: Structure supports adding new pages and features
- **Developer Friendly**: Includes comprehensive testing and debugging tools

This organized structure makes the project more maintainable, scalable, and easier to work with for both development and deployment.
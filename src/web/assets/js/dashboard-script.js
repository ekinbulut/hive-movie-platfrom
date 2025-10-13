// API Configuration - Environment aware
const API_CONFIG = {
    AUTH_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:8082' 
        : 'http://hive-idm:8082',
    MOVIES_BASE_URL: window.location.hostname === 'localhost' 
        ? 'http://localhost:8080' 
        : 'http://hive-app:8080',
    get MOVIES_ENDPOINT() {
        return `${this.MOVIES_BASE_URL}/v1/api/movies`;
    }
};

// Override with environment variables if available (Docker)
if (typeof window !== 'undefined' && window.ENV) {
    if (window.ENV.AUTH_BASE_URL) {
        API_CONFIG.AUTH_BASE_URL = window.ENV.AUTH_BASE_URL;
    }
    if (window.ENV.MOVIES_BASE_URL) {
        API_CONFIG.MOVIES_BASE_URL = window.ENV.MOVIES_BASE_URL;
    }
}

// Import SessionManager from login page
// Session management utilities (same as login page)
class SessionManager {
    static setAccessToken(accessToken, tokenType, expiresIn) {
        const tokenData = {
            accessToken,
            tokenType,
            expiresIn,
            timestamp: Date.now()
        };
        
        sessionStorage.setItem('authToken', JSON.stringify(tokenData));
        
        const expirationTime = Date.now() + (expiresIn * 1000);
        const persistentData = {
            ...tokenData,
            expirationTime
        };
        localStorage.setItem('authToken', JSON.stringify(persistentData));
    }
    
    static getAccessToken() {
        let tokenData = sessionStorage.getItem('authToken');
        if (tokenData) {
            return JSON.parse(tokenData);
        }
        
        tokenData = localStorage.getItem('authToken');
        if (tokenData) {
            const data = JSON.parse(tokenData);
            if (Date.now() < data.expirationTime) {
                sessionStorage.setItem('authToken', JSON.stringify(data));
                return data;
            } else {
                localStorage.removeItem('authToken');
            }
        }
        
        return null;
    }
    
    static clearAccessToken() {
        sessionStorage.removeItem('authToken');
        localStorage.removeItem('authToken');
    }
    
    static isAuthenticated() {
        const tokenData = this.getAccessToken();
        if (!tokenData) return false;
        
        const expirationTime = tokenData.timestamp + (tokenData.expiresIn * 1000);
        return Date.now() < expirationTime;
    }
}

// Movies API class
class MoviesAPI {
    static async getMovies(pageNumber = 1, pageSize = 25) {
        const tokenData = SessionManager.getAccessToken();
        if (!tokenData) {
            throw new Error('No authentication token found');
        }

        console.log('Fetching movies:', { pageNumber, pageSize });

        try {
            const response = await fetch(API_CONFIG.MOVIES_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `${tokenData.tokenType} ${tokenData.accessToken}`
                },
                body: JSON.stringify({
                    pageNumber,
                    pageSize
                })
            });

            console.log('Movies API response status:', response.status);

            if (!response.ok) {
                if (response.status === 401) {
                    // Token expired or invalid
                    SessionManager.clearAccessToken();
                    throw new Error('Authentication expired. Please login again.');
                }

                let errorMessage = 'Failed to fetch movies';
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch (e) {
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(`${errorMessage} (Status: ${response.status})`);
            }

            let data;
            try {
                data = await response.json();
            } catch (jsonError) {
                console.error('Failed to parse API response as JSON:', jsonError);
                throw new Error('Server returned invalid data format. Please check your API server.');
            }
            
            // Validate API response structure
            if (!data || typeof data !== 'object') {
                throw new Error('Server returned empty or invalid data. Please check your API server and try again.');
            }
            
            // Ensure movies array exists, default to empty array if null/undefined
            if (!Array.isArray(data.movies)) {
                console.warn('API response missing movies array, defaulting to empty array');
                data.movies = [];
            }
            
            console.log('Movies fetched successfully:', {
                moviesInResponse: data.movies.length,
                totalMovies: data.total || 0,
                pageNumber: data.pageNumber || this.currentPage,
                pageSize: data.pageSize || this.pageSize,
                currentPage: this.currentPage
            });

            // Log API response structure and sample movie data
            console.log('API Response structure:', {
                hasMovies: !!data.movies,
                moviesCount: data.movies ? data.movies.length : 0,
                hasTotal: !!data.total,
                total: data.total,
                hasPageNumber: !!data.pageNumber,
                pageNumber: data.pageNumber,
                hasPageSize: !!data.pageSize,
                pageSize: data.pageSize
            });

            if (data.movies.length > 0) {
                console.log('Sample movie data:', {
                    name: data.movies[0].name,
                    hasImage: !!data.movies[0].image,
                    hasFullImageUrl: !!data.movies[0].fullImageUrl,
                    fullImageUrl: data.movies[0].fullImageUrl,
                    createdTime: data.movies[0].createdTime,
                    hasCreatedTime: !!data.movies[0].createdTime,
                    releaseDate: data.movies[0].releaseDate,
                    hasReleaseDate: !!data.movies[0].releaseDate
                });
            }

            return data;
        } catch (error) {
            console.error('Movies API error:', error);
            
            if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                throw new Error(`Cannot connect to movies API server. Please ensure the server is running on ${API_CONFIG.MOVIES_BASE_URL.replace('http://', '')}`);
            }
            
            throw error;
        }
    }

    static async getFilteredMovies(year = null, pageNumber = 1, pageSize = 25) {
        const tokenData = SessionManager.getAccessToken();
        if (!tokenData) {
            throw new Error('No authentication token found');
        }

        console.log('Fetching filtered movies:', { year, pageNumber, pageSize });

        try {
            const response = await fetch(`${API_CONFIG.MOVIES_BASE_URL}/v1/api/movies/filter`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `${tokenData.tokenType} ${tokenData.accessToken}`
                },
                body: JSON.stringify({
                    year: year || null,
                    pageNumber,
                    pageSize
                })
            });

            console.log('Filter API response status:', response.status);

            if (!response.ok) {
                if (response.status === 401) {
                    // Token expired or invalid
                    SessionManager.clearAccessToken();
                    throw new Error('Authentication expired. Please login again.');
                }

                let errorMessage = 'Failed to fetch filtered movies';
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch (e) {
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(`${errorMessage} (Status: ${response.status})`);
            }

            let data;
            try {
                data = await response.json();
            } catch (jsonError) {
                console.error('Failed to parse filter API response as JSON:', jsonError);
                throw new Error('Server returned invalid data format. Please check your API server.');
            }
            
            // Validate API response structure
            if (!data || typeof data !== 'object') {
                throw new Error('Server returned empty or invalid data. Please check your API server and try again.');
            }
            
            // Ensure movies array exists, default to empty array if null/undefined
            if (!Array.isArray(data.movies)) {
                console.warn('Filter API response missing movies array, defaulting to empty array');
                data.movies = [];
            }
            
            console.log('Filtered movies fetched successfully:', {
                moviesInResponse: data.movies.length,
                totalMovies: data.total || 0,
                pageNumber: data.pageNumber || pageNumber,
                pageSize: data.pageSize || pageSize,
                year: year
            });

            // Log API response structure and sample movie data
            console.log('Filter API Response structure:', {
                hasMovies: !!data.movies,
                moviesCount: data.movies ? data.movies.length : 0,
                hasTotal: !!data.total,
                total: data.total,
                hasPageNumber: !!data.pageNumber,
                pageNumber: data.pageNumber,
                hasPageSize: !!data.pageSize,
                pageSize: data.pageSize
            });

            if (data.movies.length > 0) {
                console.log('Sample filtered movie data:', {
                    id: data.movies[0].id,
                    name: data.movies[0].name,
                    hasImage: !!data.movies[0].image,
                    hasFullImageUrl: !!data.movies[0].fullImageUrl,
                    fullImageUrl: data.movies[0].fullImageUrl,
                    createdTime: data.movies[0].createdTime,
                    hasCreatedTime: !!data.movies[0].createdTime,
                    releaseDate: data.movies[0].releaseDate,
                    hasReleaseDate: !!data.movies[0].releaseDate
                });
            }

            return data;
        } catch (error) {
            console.error('Filter API error:', error);
            
            if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                throw new Error(`Cannot connect to movies filter API server. Please ensure the server is running on ${API_CONFIG.MOVIES_BASE_URL.replace('http://', '')}`);
            }
            
            throw error;
        }
    }

    static async getFilters() {
        const tokenData = SessionManager.getAccessToken();
        if (!tokenData) {
            throw new Error('No authentication token found');
        }

        console.log('Fetching filters...');

        try {
            const response = await fetch(`${API_CONFIG.MOVIES_BASE_URL}/v1/api/filters`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `${tokenData.tokenType} ${tokenData.accessToken}`
                }
            });

            console.log('Filters API response status:', response.status);

            if (!response.ok) {
                if (response.status === 401) {
                    // Token expired or invalid
                    SessionManager.clearAccessToken();
                    throw new Error('Authentication expired. Please login again.');
                }

                let errorMessage = 'Failed to fetch filters';
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.error || errorMessage;
                } catch (e) {
                    errorMessage = response.statusText || errorMessage;
                }
                
                throw new Error(`${errorMessage} (Status: ${response.status})`);
            }

            let data;
            try {
                data = await response.json();
            } catch (jsonError) {
                console.error('Failed to parse filters API response as JSON:', jsonError);
                throw new Error('Server returned invalid data format. Please check your API server.');
            }
            
            // Validate API response structure
            if (!data || typeof data !== 'object') {
                throw new Error('Server returned empty or invalid filters data.');
            }
            
            // Validate filters structure
            if (!data.filters || typeof data.filters !== 'object') {
                console.warn('Filters API response missing filters object, using empty filters');
                data.filters = {};
            }

            // Ensure years array exists
            if (!Array.isArray(data.filters.years)) {
                console.warn('Filters API response missing years array, defaulting to empty array');
                data.filters.years = [];
            }
            
            console.log('Filters fetched successfully:', {
                hasFilters: !!data.filters,
                yearsCount: data.filters.years ? data.filters.years.length : 0,
                yearsRange: data.filters.years && data.filters.years.length > 0 
                    ? `${Math.min(...data.filters.years)} - ${Math.max(...data.filters.years)}`
                    : 'No years available'
            });

            // Log sample years data
            if (data.filters.years && data.filters.years.length > 0) {
                console.log('Available years:', {
                    total: data.filters.years.length,
                    latest: data.filters.years.slice(0, 5),
                    oldest: data.filters.years.slice(-5),
                    allYears: data.filters.years
                });
            }

            return data;
        } catch (error) {
            console.error('Filters API error:', error);
            
            if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
                throw new Error(`Cannot connect to filters API server. Please ensure the server is running on ${API_CONFIG.MOVIES_BASE_URL.replace('http://', '')}`);
            }
            
            throw error;
        }
    }
}

// Utility functions
class Utils {
    static formatFileSize(fileSize) {
        // Handle null, undefined, empty string
        if (!fileSize || fileSize === '') return 'Unknown';
        
        // If fileSize is already a formatted string from API, return it directly
        if (typeof fileSize === 'string') {
            return fileSize.trim() || 'Unknown';
        }
        
        // Fallback for numeric values (legacy support)
        if (typeof fileSize === 'number') {
            if (fileSize === 0) return 'Unknown';
            const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
            const i = Math.floor(Math.log(fileSize) / Math.log(1024));
            return `${Math.round(fileSize / Math.pow(1024, i) * 100) / 100} ${sizes[i]}`;
        }
        
        return 'Unknown';
    }

    static formatMovieName(movie) {
        // Handle null/undefined movie object
        if (!movie || typeof movie !== 'object') {
            return 'Unknown Movie';
        }
        
        // Use movie name if available and not empty
        if (movie.name && typeof movie.name === 'string' && movie.name.trim()) {
            return movie.name.trim();
        }
        
        // Fallback to filename extraction from path
        if (movie.filePath && typeof movie.filePath === 'string' && movie.filePath.trim()) {
            try {
                const pathParts = movie.filePath.split(/[/\\]/);
                const filename = pathParts[pathParts.length - 1];
                // Remove file extension
                const nameWithoutExt = filename.replace(/\.[^/.]+$/, '');
                return nameWithoutExt.trim() || 'Unknown Movie';
            } catch (error) {
                console.warn('Error extracting movie name from file path:', movie.filePath, error);
            }
        }
        
        return 'Unknown Movie';
    }

    static debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

// Dashboard Controller
class DashboardController {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 25;
        this.currentView = 'grid';
        this.movies = [];
        this.totalMovies = 0;
        this.totalPages = 0;
        this.searchQuery = '';
        this.sortBy = 'createdTime-desc'; // Default to newest first
        this.selectedYear = ''; // Default to all years
        this.yearFilterPopulated = false; // Track if static year filter is populated
        
        this.initializeElements();
        this.attachEventListeners();
        this.initializeUI();
        
        // Initialize video player controller
        this.videoPlayerController = new VideoPlayerController();
        
        this.checkAuthentication();
    }

    initializeElements() {
        // Navigation elements
        this.logoutBtn = document.getElementById('logoutBtn');
        
        // Control elements
        this.searchInput = document.getElementById('searchInput');
        this.searchBtn = document.getElementById('searchBtn');
        this.sortSelect = document.getElementById('sortSelect');
        this.yearFilter = document.getElementById('yearFilter');
        this.pageSizeSelect = document.getElementById('pageSizeSelect');
        
        // View is always grid (list view removed)
        
        // Content containers
        this.loadingState = document.getElementById('loadingState');
        this.errorState = document.getElementById('errorState');
        this.emptyState = document.getElementById('emptyState');
        this.moviesContainer = document.getElementById('moviesContainer');
        
        // Pagination elements
        this.pagination = document.getElementById('pagination');
        this.prevBtn = document.getElementById('prevBtn');
        this.nextBtn = document.getElementById('nextBtn');
        this.pageInfo = document.getElementById('pageInfo');
        
        // Modal elements
        this.movieModal = document.getElementById('movieModal');
        this.closeModal = document.getElementById('closeModal');
        this.closeModalBtn = document.getElementById('closeModalBtn');
        
        // Error state elements
        this.retryBtn = document.getElementById('retryBtn');
        this.errorMessage = document.getElementById('errorMessage');
    }

    attachEventListeners() {
        // Navigation
        this.logoutBtn.addEventListener('click', () => this.logout());
        
        // Search
        const debouncedSearch = Utils.debounce(() => this.handleSearch(), 500);
        this.searchInput.addEventListener('input', debouncedSearch);
        this.searchBtn.addEventListener('click', () => this.handleSearch());
        
        // Controls
        this.sortSelect.addEventListener('change', () => this.handleSortChange());
        this.yearFilter.addEventListener('change', () => this.handleYearFilterChange());
        this.pageSizeSelect.addEventListener('change', () => this.handlePageSizeChange());
        
        // View is always grid (list view removed)
        
        // Pagination
        this.prevBtn.addEventListener('click', () => this.goToPreviousPage());
        this.nextBtn.addEventListener('click', () => this.goToNextPage());
        
        // Modal
        this.closeModal.addEventListener('click', () => this.closeMovieModal());
        this.closeModalBtn.addEventListener('click', () => this.closeMovieModal());
        this.movieModal.addEventListener('click', (e) => {
            if (e.target === this.movieModal) this.closeMovieModal();
        });
        
        // Error state
        this.retryBtn.addEventListener('click', () => this.loadMovies());
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.movieModal.style.display === 'flex') {
                this.closeMovieModal();
            }
        });
    }

    initializeUI() {
        // Set default sort value
        if (this.sortSelect) {
            this.sortSelect.value = this.sortBy;
        }
        
        // Set default year filter value
        if (this.yearFilter) {
            this.yearFilter.value = this.selectedYear;
        }
    }

    checkAuthentication() {
        if (!SessionManager.isAuthenticated()) {
            alert('Please login first to access the dashboard.');
            window.location.href = 'login.html';
            return;
        }
        
        // Initialize year filter independently
        this.initializeYearFilter();
        
        this.loadMovies();
    }

    async initializeYearFilter() {
        if (!this.yearFilterPopulated) {
            try {
                await this.populateYearFilter();
                this.yearFilterPopulated = true;
            } catch (error) {
                console.error('Failed to initialize year filter:', error);
                // Continue without year filter if it fails
            }
        }
    }

    async loadMovies() {
        this.showLoadingState();
        
        try {
            let data;
            
            // Use filter endpoint if year is selected, otherwise use regular endpoint
            if (this.selectedYear) {
                console.log(`Loading movies for year: ${this.selectedYear}`);
                data = await MoviesAPI.getFilteredMovies(parseInt(this.selectedYear), this.currentPage, this.pageSize);
            } else {
                console.log('Loading all movies');
                data = await MoviesAPI.getMovies(this.currentPage, this.pageSize);
            }
            
            // Validate response data
            if (!data) {
                throw new Error('API returned null or undefined response');
            }
            
            // Ensure movies array exists and is valid
            this.movies = Array.isArray(data.movies) ? data.movies : [];
            
            // Validate movie objects
            this.movies = this.movies.filter(movie => {
                if (!movie || typeof movie !== 'object') {
                    console.warn('Invalid movie object found, skipping:', movie);
                    return false;
                }
                return true;
            });
            
            this.renderMovies();
            this.updatePagination(data);
            
            if (this.movies.length === 0) {
                this.showEmptyState();
            } else {
                this.showMoviesContainer();
            }
            
        } catch (error) {
            console.error('Failed to load movies:', error);
            this.showErrorState(error.message);
            
            // Reset to empty state on error
            this.movies = [];
            
            if (error.message.includes('Authentication expired')) {
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 2000);
            }
        }
    }

    sortMovies(movies) {
        if (!Array.isArray(movies) || movies.length === 0) {
            return movies;
        }

        const [field, direction] = this.sortBy.split('-');
        const isAscending = direction === 'asc';
        
        return movies.sort((a, b) => {
            let valueA, valueB;
            
            switch (field) {
                case 'name':
                    valueA = Utils.formatMovieName(a).toLowerCase();
                    valueB = Utils.formatMovieName(b).toLowerCase();
                    break;
                    
                case 'fileSize':
                    valueA = this.getNumericFileSize(a.fileSize);
                    valueB = this.getNumericFileSize(b.fileSize);
                    break;
                    
                case 'createdTime':
                    // Use createdTime from API (ISO format: "2025-10-06T14:57:04.146903Z")
                    if (a.createdTime && b.createdTime) {
                        valueA = new Date(a.createdTime);
                        valueB = new Date(b.createdTime);
                        console.log('Sorting by createdTime:', {
                            movieA: Utils.formatMovieName(a),
                            createdTimeA: a.createdTime,
                            dateA: valueA.toISOString(),
                            movieB: Utils.formatMovieName(b),
                            createdTimeB: b.createdTime,
                            dateB: valueB.toISOString()
                        });
                    } else {
                        // Fallback to filename-based sorting if no created time
                        console.log('createdTime not available, falling back to name-based sorting');
                        valueA = Utils.formatMovieName(a).toLowerCase();
                        valueB = Utils.formatMovieName(b).toLowerCase();
                    }
                    break;
                    
                case 'releaseDate':
                    // Use releaseDate from API (year format: 2025)
                    valueA = a.releaseDate ? parseInt(a.releaseDate) : 0;
                    valueB = b.releaseDate ? parseInt(b.releaseDate) : 0;
                    console.log('Sorting by releaseDate:', {
                        movieA: Utils.formatMovieName(a),
                        releaseDateA: a.releaseDate,
                        movieB: Utils.formatMovieName(b),
                        releaseDateB: b.releaseDate
                    });
                    break;
                    
                default:
                    valueA = Utils.formatMovieName(a).toLowerCase();
                    valueB = Utils.formatMovieName(b).toLowerCase();
            }
            
            // Handle Date objects
            if (valueA instanceof Date && valueB instanceof Date) {
                return isAscending ? valueA - valueB : valueB - valueA;
            }
            
            // Handle numbers
            if (typeof valueA === 'number' && typeof valueB === 'number') {
                return isAscending ? valueA - valueB : valueB - valueA;
            }
            
            // Handle strings
            if (typeof valueA === 'string' && typeof valueB === 'string') {
                return isAscending ? valueA.localeCompare(valueB) : valueB.localeCompare(valueA);
            }
            
            return 0;
        });
    }

    getNumericFileSize(fileSize) {
        // Handle null, undefined, empty string
        if (!fileSize || fileSize === '') return 0;
        
        // If fileSize is already a number, return it
        if (typeof fileSize === 'number') {
            return fileSize;
        }
        
        // If fileSize is a formatted string like "1.2 GB", parse it
        if (typeof fileSize === 'string') {
            const match = fileSize.match(/^([\d.]+)\s*([KMGT]?B?)$/i);
            if (match) {
                const value = parseFloat(match[1]);
                const unit = match[2].toUpperCase();
                
                const multipliers = {
                    'B': 1,
                    'KB': 1024,
                    'MB': 1024 * 1024,
                    'GB': 1024 * 1024 * 1024,
                    'TB': 1024 * 1024 * 1024 * 1024
                };
                
                return value * (multipliers[unit] || 1);
            }
        }
        
        return 0;
    }

    async populateYearFilter() {
        if (!this.yearFilter) {
            return;
        }

        try {
            // Fetch available years from API
            const filtersData = await MoviesAPI.getFilters();
            const availableYears = filtersData.filters.years || [];

            // Sort years in descending order (newest first)
            const sortedYears = availableYears
                .map(year => parseInt(year))
                .filter(year => !isNaN(year))
                .sort((a, b) => b - a);

            // Clear existing options (except "All Years")
            while (this.yearFilter.children.length > 1) {
                this.yearFilter.removeChild(this.yearFilter.lastChild);
            }

            // Add year options
            sortedYears.forEach(year => {
                const option = document.createElement('option');
                option.value = year.toString();
                option.textContent = year.toString();
                this.yearFilter.appendChild(option);
            });

            console.log('Year filter populated with API years:', {
                totalYears: sortedYears.length,
                yearRange: sortedYears.length > 0 
                    ? `${sortedYears[sortedYears.length - 1]} - ${sortedYears[0]}`
                    : 'No years',
                years: sortedYears
            });

        } catch (error) {
            console.error('Failed to populate year filter:', error);
            
            // Fallback to empty filter on error
            while (this.yearFilter.children.length > 1) {
                this.yearFilter.removeChild(this.yearFilter.lastChild);
            }
            
            console.log('Year filter cleared due to API error');
        }
    }

    renderMovies() {
        this.moviesContainer.innerHTML = '';
        
        // Ensure movies array is valid
        if (!Array.isArray(this.movies)) {
            console.warn('Movies is not an array, resetting to empty array');
            this.movies = [];
            return;
        }
        
        const filteredMovies = this.movies.filter(movie => {
            // Validate movie object
            if (!movie || typeof movie !== 'object') {
                console.warn('Invalid movie object in filter:', movie);
                return false;
            }
            
            // Apply search filter (client-side)
            if (this.searchQuery) {
                const movieName = Utils.formatMovieName(movie).toLowerCase();
                const filePath = (movie.filePath || '').toLowerCase();
                const matchesSearch = movieName.includes(this.searchQuery.toLowerCase()) || 
                                    filePath.includes(this.searchQuery.toLowerCase());
                if (!matchesSearch) return false;
            }
            
            // Year filter is now handled server-side, no client-side filtering needed
            
            return true;
        });

        // Sort the filtered movies
        const sortedMovies = this.sortMovies(filteredMovies);

        sortedMovies.forEach(movie => {
            try {
                const movieCard = this.createMovieCard(movie);
                if (movieCard) {
                    this.moviesContainer.appendChild(movieCard);
                }
            } catch (error) {
                console.error('Error creating movie card for movie:', movie, error);
            }
        });
    }

    createMovieCard(movie) {
        // Validate movie object
        if (!movie || typeof movie !== 'object') {
            console.error('Invalid movie object passed to createMovieCard:', movie);
            return null;
        }

        const card = document.createElement('div');
        card.className = 'movie-card';
        card.addEventListener('click', () => this.showMovieModal(movie));

        const movieName = Utils.formatMovieName(movie);
        const fileSize = Utils.formatFileSize(movie.fileSize);
        
        // Create movie image container
        const movieImageDiv = document.createElement('div');
        movieImageDiv.className = 'movie-image';

        if (movie.fullImageUrl) {
            const img = document.createElement('img');
            img.src = movie.fullImageUrl;
            img.alt = movieName;
            img.loading = 'lazy';
            img.addEventListener('error', () => {
                movieImageDiv.innerHTML = '<div class="placeholder">🎬<br><small>No Image</small></div>';
            });
            movieImageDiv.appendChild(img);
        } else {
            movieImageDiv.innerHTML = '<div class="placeholder">🎬<br><small>No Image</small></div>';
        }

        // Movie card now only contains the poster image
        card.appendChild(movieImageDiv);

        return card;
    }

    showMovieModal(movie) {
        const movieName = Utils.formatMovieName(movie);
        const fileSize = Utils.formatFileSize(movie.fileSize);

        document.getElementById('modalTitle').textContent = movieName;
        document.getElementById('modalName').textContent = movie.name || 'N/A';
        document.getElementById('modalFileSize').textContent = fileSize;
        
        // Set subtitle availability as Yes/No
        const hasSubtitle = movie.subTitleFilePath && movie.subTitleFilePath.trim() !== '';
        document.getElementById('modalHasSubtitle').textContent = hasSubtitle ? 'Yes' : 'No';
        
        const modalImage = document.getElementById('modalImage');
        if (movie.fullImageUrl) {
            modalImage.src = movie.fullImageUrl;
            modalImage.style.display = 'block';
        } else {
            modalImage.style.display = 'none';
        }

        // Show/hide play button based on streamId availability
        const playBtn = document.getElementById('playMovieBtn');
        if (movie.streamId && movie.streamId.trim() !== '') {
            playBtn.style.display = 'inline-flex';
            playBtn.onclick = () => this.playMovie(movie);
        } else {
            playBtn.style.display = 'none';
        }

        this.movieModal.style.display = 'flex';
        setTimeout(() => {
            this.closeModalBtn.focus();
        }, 100);
    }

    playMovie(movie) {
        this.closeMovieModal();
        this.videoPlayerController.openVideoPlayer(movie);
    }

    closeMovieModal() {
        this.movieModal.style.display = 'none';
    }

    updatePagination(data) {
        // Use total count from API response for accurate pagination
        const totalMovies = data.total || 0;
        const totalPages = Math.ceil(totalMovies / this.pageSize);
        
        const hasPrevPage = this.currentPage > 1;
        const hasNextPage = this.currentPage < totalPages;

        this.prevBtn.disabled = !hasPrevPage;
        this.nextBtn.disabled = !hasNextPage;
        
        // Update page info to show current page and total pages
        this.pageInfo.textContent = `Page ${this.currentPage} of ${totalPages} (${totalMovies} movies)`;
        this.pagination.style.display = totalPages > 1 ? 'flex' : 'none';
        
        // Store total for other methods that might need it
        this.totalMovies = totalMovies;
        this.totalPages = totalPages;
        
        console.log('Pagination updated:', {
            currentPage: this.currentPage,
            totalPages: totalPages,
            totalMovies: totalMovies,
            pageSize: this.pageSize,
            hasPrevPage: hasPrevPage,
            hasNextPage: hasNextPage
        });
    }

    // View functionality removed - always uses grid view

    handleSearch() {
        this.searchQuery = this.searchInput.value.trim();
        // Search is done client-side, just re-render
        this.renderMovies();
    }

    handlePageSizeChange() {
        this.pageSize = parseInt(this.pageSizeSelect.value);
        this.currentPage = 1;
        this.loadMovies();
    }

    handleSortChange() {
        this.sortBy = this.sortSelect.value;
        this.renderMovies(); // Re-render with new sorting
    }

    handleYearFilterChange() {
        this.selectedYear = this.yearFilter.value;
        // Reset to first page when filtering
        this.currentPage = 1;
        // Reload movies using appropriate endpoint (filter or regular)
        this.loadMovies();
    }

    goToPreviousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.loadMovies();
        }
    }

    goToNextPage() {
        this.currentPage++;
        this.loadMovies();
    }

    showLoadingState() {
        this.loadingState.style.display = 'block';
        this.errorState.style.display = 'none';
        this.emptyState.style.display = 'none';
        this.moviesContainer.style.display = 'none';
        this.pagination.style.display = 'none';
    }

    showErrorState(message) {
        this.loadingState.style.display = 'none';
        this.errorState.style.display = 'block';
        this.emptyState.style.display = 'none';
        this.moviesContainer.style.display = 'none';
        this.pagination.style.display = 'none';
        this.errorMessage.textContent = message;
    }

    showEmptyState() {
        this.loadingState.style.display = 'none';
        this.errorState.style.display = 'none';
        this.emptyState.style.display = 'block';
        this.moviesContainer.style.display = 'none';
        this.pagination.style.display = 'none';
    }

    showMoviesContainer() {
        this.loadingState.style.display = 'none';
        this.errorState.style.display = 'none';
        this.emptyState.style.display = 'none';
        this.moviesContainer.style.display = 'grid';
    }

    logout() {
        if (confirm('Are you sure you want to logout?')) {
            SessionManager.clearAccessToken();
            window.location.href = 'login.html';
        }
    }
}

// Jellyfin API Configuration - Environment aware
const JELLYFIN_CONFIG = {
    baseUrl: window.location.hostname === 'localhost' 
        ? 'http://192.168.1.112:8096' 
        : 'http://jellyfin:8096',
    accessToken: null, // Must be provided via environment variables
    userId: null, // Will be set during authentication
};

// Override with environment variables if available (Docker)
if (typeof window !== 'undefined' && window.ENV) {
    if (window.ENV.JELLYFIN_BASE_URL) {
        JELLYFIN_CONFIG.baseUrl = window.ENV.JELLYFIN_BASE_URL;
    }
    if (window.ENV.JELLYFIN_ACCESS_TOKEN) {
        JELLYFIN_CONFIG.accessToken = window.ENV.JELLYFIN_ACCESS_TOKEN;
    }
}

// Jellyfin API class
class JellyfinAPI {
    static async testConnection() {
        // Validate that access token is provided
        if (!JELLYFIN_CONFIG.accessToken) {
            throw new Error('Jellyfin access token not configured. Please set JELLYFIN_ACCESS_TOKEN environment variable.');
        }
        
        try {
            // Try public endpoint first
            let testUrl = `${JELLYFIN_CONFIG.baseUrl}/System/Info/Public`;
            let response = await fetch(testUrl);
            
            if (!response.ok) {
                // Try with API key
                testUrl = `${JELLYFIN_CONFIG.baseUrl}/System/Info?api_key=${JELLYFIN_CONFIG.accessToken}`;
                response = await fetch(testUrl);
            }
            
            if (response.ok) {
                const systemInfo = await response.json();
                console.log('Connected to Jellyfin:', systemInfo.ServerName);
                
                // Try to get users
                try {
                    const usersResponse = await fetch(`${JELLYFIN_CONFIG.baseUrl}/Users?api_key=${JELLYFIN_CONFIG.accessToken}`);
                    if (usersResponse.ok) {
                        const users = await usersResponse.json();
                        if (users.length > 0) {
                            JELLYFIN_CONFIG.userId = users[0].Id;
                        }
                    }
                } catch (error) {
                    console.log('Could not get user info, continuing without user context');
                }
                
                return { success: true, info: systemInfo };
            } else {
                throw new Error(`Server responded with ${response.status}`);
            }
        } catch (error) {
            console.error('Jellyfin connection failed:', error);
            return { success: false, error: error.message };
        }
    }

    static async getMovieInfo(streamId) {
        if (!JELLYFIN_CONFIG.accessToken) {
            throw new Error('Jellyfin access token not configured');
        }
        
        try {
            const url = `${JELLYFIN_CONFIG.baseUrl}/Items/${streamId}?Fields=MediaStreams,MediaSources&api_key=${JELLYFIN_CONFIG.accessToken}`;
            console.log(`Fetching movie info from: ${url}`);
            
            const response = await fetch(url);
            
            if (response.ok) {
                const data = await response.json();
                console.log('Movie info retrieved successfully:', data.Name || 'Unknown');
                return data;
            } else {
                const errorText = await response.text();
                console.error(`Movie info request failed: ${response.status} ${response.statusText}`, errorText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('Error getting movie info:', error);
            throw error;
        }
    }

    static getDirectStreamUrl(streamId) {
        if (!JELLYFIN_CONFIG.accessToken) {
            throw new Error('Jellyfin access token not configured');
        }
        return `${JELLYFIN_CONFIG.baseUrl}/Videos/${streamId}/stream?Static=true&api_key=${JELLYFIN_CONFIG.accessToken}`;
    }

    static getTranscodeStreamUrl(streamId, quality = 'auto') {
        if (!JELLYFIN_CONFIG.accessToken) {
            throw new Error('Jellyfin access token not configured');
        }
        
        let maxBitrate = '';
        switch(quality) {
            case '480p': maxBitrate = '&VideoBitRate=1000000'; break;
            case '720p': maxBitrate = '&VideoBitRate=3000000'; break;
            case '1080p': maxBitrate = '&VideoBitRate=8000000'; break;
            default: maxBitrate = '';
        }
        
        return `${JELLYFIN_CONFIG.baseUrl}/Videos/${streamId}/stream?AudioCodec=aac&VideoCodec=h264&Container=mp4${maxBitrate}&api_key=${JELLYFIN_CONFIG.accessToken}`;
    }

    static getSubtitleUrl(streamId, subtitleIndex) {
        if (!JELLYFIN_CONFIG.accessToken) {
            throw new Error('Jellyfin access token not configured');
        }
        return `${JELLYFIN_CONFIG.baseUrl}/Videos/${streamId}/Subtitles/${subtitleIndex}/Stream.vtt?api_key=${JELLYFIN_CONFIG.accessToken}`;
    }
}

// Video Player Controller
class VideoPlayerController {
    constructor() {
        this.initializeElements();
        this.attachEventListeners();
        this.currentMovie = null;
        this.currentStreamId = null;
        this.isPlaying = false;
    }

    initializeElements() {
        // Video modal elements
        this.videoModal = document.getElementById('videoPlayerModal');
        this.videoPlayer = document.getElementById('jellyfinVideoPlayer');
        this.videoLoadingState = document.getElementById('videoLoadingState');
        this.videoErrorState = document.getElementById('videoErrorState');
        
        // Control buttons
        this.closeVideoBtn = document.getElementById('closeVideoBtn');
        this.minimizeVideoBtn = document.getElementById('minimizeVideoBtn');
        this.retryDirectBtn = document.getElementById('retryDirectBtn');
        this.retryTranscodeBtn = document.getElementById('retryTranscodeBtn');
        

    }

    attachEventListeners() {
        // Modal controls
        this.closeVideoBtn?.addEventListener('click', () => this.closeVideoPlayer());
        this.minimizeVideoBtn?.addEventListener('click', () => this.minimizeVideoPlayer());
        

        
        // Error retry buttons
        this.retryDirectBtn?.addEventListener('click', () => this.streamDirect());
        this.retryTranscodeBtn?.addEventListener('click', () => this.streamTranscode());
        

        
        // Video player events
        if (this.videoPlayer) {
            this.videoPlayer.addEventListener('loadstart', () => console.log('Loading started...'));
            this.videoPlayer.addEventListener('loadeddata', () => console.log('Video data loaded'));
            this.videoPlayer.addEventListener('loadedmetadata', () => {
                console.log(`Video loaded: ${this.videoPlayer.videoWidth}x${this.videoPlayer.videoHeight}, Duration: ${this.formatTime(this.videoPlayer.duration)}`);
                this.hideLoadingState();
                this.videoPlayer.style.display = 'block';
            });
            this.videoPlayer.addEventListener('error', (e) => {
                this.showErrorState(`Error: ${e.target.error?.message || 'Unknown error'}`);
            });
            this.videoPlayer.addEventListener('play', () => this.isPlaying = true);
            this.videoPlayer.addEventListener('pause', () => this.isPlaying = false);
        }
        
        // Modal close on background click
        this.videoModal?.addEventListener('click', (e) => {
            if (e.target === this.videoModal) this.closeVideoPlayer();
        });
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (this.videoModal?.style.display === 'flex') {
                switch(e.key) {
                    case 'Escape':
                        this.closeVideoPlayer();
                        break;
                    case ' ':
                        e.preventDefault();
                        if (this.isPlaying) {
                            this.videoPlayer.pause();
                        } else {
                            this.videoPlayer.play();
                        }
                        break;

                }
            }
        });
    }

    async openVideoPlayer(movie) {
        this.currentMovie = movie;
        this.currentStreamId = movie.streamId;
        
        console.log('Opening video player for movie:', {
            name: movie.name,
            streamId: movie.streamId,
            movie: movie
        });
        
        // Show modal
        this.videoModal.style.display = 'flex';
        
        // Show loading state initially
        this.showLoadingState();
        
        // Test Jellyfin connection and load movie
        console.log('Connecting to Jellyfin server...');
        
        const connectionResult = await JellyfinAPI.testConnection();
        if (!connectionResult.success) {
            this.showErrorState(`Failed to connect to Jellyfin: ${connectionResult.error}`);
            return;
        }
        
        console.log(`Connected to ${connectionResult.info.ServerName || 'Jellyfin'}`);
        
        // Try to get movie info from Jellyfin, but don't fail if it doesn't work
        try {
            const movieInfo = await JellyfinAPI.getMovieInfo(this.currentStreamId);
            console.log('Movie info loaded. Starting direct stream...');
        } catch (error) {
            console.warn('Could not load movie info from Jellyfin:', error.message);
            console.log('Connected to Jellyfin. Starting direct stream...');
        }
        
        // Automatically start direct streaming
        this.streamDirect();
    }

    closeVideoPlayer() {
        if (this.videoPlayer) {
            this.videoPlayer.pause();
            this.videoPlayer.src = '';
        }
        this.videoModal.style.display = 'none';
        this.currentMovie = null;
        this.currentStreamId = null;
        this.isPlaying = false;
    }

    minimizeVideoPlayer() {
        // For now, just hide the modal but keep playing
        this.videoModal.style.display = 'none';
        // Could be extended to create a mini player
    }

    showLoadingState() {
        this.videoLoadingState.style.display = 'flex';
        this.videoErrorState.style.display = 'none';
        this.videoPlayer.style.display = 'none';
    }

    hideLoadingState() {
        this.videoLoadingState.style.display = 'none';
    }

    showReadyState() {
        this.videoLoadingState.style.display = 'none';
        this.videoErrorState.style.display = 'none';
        this.videoPlayer.style.display = 'none';
        
        // Show a ready state message in the player area
        this.showReadyMessage();
    }

    showReadyMessage() {
        // Create or update ready state element
        let readyState = document.getElementById('videoReadyState');
        if (!readyState) {
            readyState = document.createElement('div');
            readyState.id = 'videoReadyState';
            readyState.className = 'video-ready-state';
            readyState.innerHTML = `
                <div class="ready-icon">🎬</div>
                <h3>Ready to Play</h3>
                <p>Choose your streaming option:</p>
                <div class="ready-actions">
                    <button class="btn-primary ready-btn" onclick="window.dashboardInstance.videoPlayerController.streamDirect()">
                        🎯 Direct Stream
                        <small>Original quality, faster start</small>
                    </button>
                    <button class="btn-primary ready-btn" onclick="window.dashboardInstance.videoPlayerController.streamTranscode()">
                        ⚙️ Transcoded Stream  
                        <small>Optimized for your device</small>
                    </button>
                </div>
            `;
            this.videoPlayer.parentNode.appendChild(readyState);
        }
        readyState.style.display = 'flex';
    }

    hideReadyState() {
        const readyState = document.getElementById('videoReadyState');
        if (readyState) {
            readyState.style.display = 'none';
        }
    }

    showErrorState(message) {
        this.videoErrorState.style.display = 'flex';
        this.videoLoadingState.style.display = 'none';
        this.videoPlayer.style.display = 'none';
        this.hideReadyState();
        document.getElementById('videoErrorMessage').textContent = message;
    }

    streamDirect() {
        if (!this.currentStreamId) return;
        
        this.hideReadyState();
        this.showLoadingState();
        console.log('Loading direct stream...');
        
        const streamUrl = JellyfinAPI.getDirectStreamUrl(this.currentStreamId);
        console.log('Starting direct stream with URL:', streamUrl);
        this.videoPlayer.src = streamUrl;
        this.videoPlayer.load();
    }











    formatTime(seconds) {
        if (isNaN(seconds)) return '00:00';
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
}

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('Dashboard initializing...');
    window.dashboardInstance = new DashboardController();
});

// Export for debugging
window.DashboardController = DashboardController;
window.SessionManager = SessionManager;
window.MoviesAPI = MoviesAPI;
window.JellyfinAPI = JellyfinAPI;
window.VideoPlayerController = VideoPlayerController;
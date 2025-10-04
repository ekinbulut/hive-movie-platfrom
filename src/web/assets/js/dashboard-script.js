// API Configuration
const API_CONFIG = {
    AUTH_BASE_URL: 'http://localhost:5188',
    MOVIES_BASE_URL: 'http://localhost:5154',
    MOVIES_ENDPOINT: 'http://localhost:5154/v1/api/movies'
};

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

            const data = await response.json();
            console.log('Movies fetched successfully:', {
                totalMovies: data.movies?.length || 0,
                pageNumber: data.pageNumber,
                pageSize: data.pageSize
            });

            // Log sample movie data to verify fullImageUrl field
            if (data.movies && data.movies.length > 0) {
                console.log('Sample movie data:', {
                    name: data.movies[0].name,
                    hasImage: !!data.movies[0].image,
                    hasFullImageUrl: !!data.movies[0].fullImageUrl,
                    fullImageUrl: data.movies[0].fullImageUrl
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
}

// Utility functions
class Utils {
    static formatFileSize(fileSize) {
        if (!fileSize) return 'Unknown';
        
        // If fileSize is already a formatted string from API, return it directly
        if (typeof fileSize === 'string') {
            return fileSize;
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
        if (movie.name) return movie.name;
        if (movie.filePath) {
            // Extract filename from path
            const pathParts = movie.filePath.split(/[/\\]/);
            const filename = pathParts[pathParts.length - 1];
            // Remove file extension
            return filename.replace(/\.[^/.]+$/, '');
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
        this.searchQuery = '';
        
        this.initializeElements();
        this.attachEventListeners();
        this.checkAuthentication();
    }

    initializeElements() {
        // Navigation elements
        this.logoutBtn = document.getElementById('logoutBtn');
        
        // Control elements
        this.searchInput = document.getElementById('searchInput');
        this.searchBtn = document.getElementById('searchBtn');
        this.pageSizeSelect = document.getElementById('pageSizeSelect');
        
        // View toggle
        this.gridViewBtn = document.getElementById('gridViewBtn');
        this.listViewBtn = document.getElementById('listViewBtn');
        
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
        this.pageSizeSelect.addEventListener('change', () => this.handlePageSizeChange());
        
        // View toggle
        this.gridViewBtn.addEventListener('click', () => this.setView('grid'));
        this.listViewBtn.addEventListener('click', () => this.setView('list'));
        
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

    checkAuthentication() {
        if (!SessionManager.isAuthenticated()) {
            alert('Please login first to access the dashboard.');
            window.location.href = 'login.html';
            return;
        }
        
        this.loadMovies();
    }

    async loadMovies() {
        this.showLoadingState();
        
        try {
            const data = await MoviesAPI.getMovies(this.currentPage, this.pageSize);
            this.movies = data.movies || [];
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
            
            if (error.message.includes('Authentication expired')) {
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 2000);
            }
        }
    }

    renderMovies() {
        this.moviesContainer.innerHTML = '';
        
        const filteredMovies = this.movies.filter(movie => {
            if (!this.searchQuery) return true;
            const movieName = Utils.formatMovieName(movie).toLowerCase();
            const filePath = (movie.filePath || '').toLowerCase();
            return movieName.includes(this.searchQuery.toLowerCase()) || 
                   filePath.includes(this.searchQuery.toLowerCase());
        });

        filteredMovies.forEach(movie => {
            const movieCard = this.createMovieCard(movie);
            this.moviesContainer.appendChild(movieCard);
        });
    }

    createMovieCard(movie) {
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

        // Insert the image container at the beginning
        card.insertBefore(movieImageDiv, card.firstChild);

        return card;
    }

    showMovieModal(movie) {
        const movieName = Utils.formatMovieName(movie);
        const fileSize = Utils.formatFileSize(movie.fileSize);

        document.getElementById('modalTitle').textContent = movieName;
        document.getElementById('modalId').textContent = movie.id || 'N/A';
        document.getElementById('modalName').textContent = movie.name || 'N/A';
        document.getElementById('modalFilePath').textContent = movie.filePath || 'N/A';
        document.getElementById('modalFileSize').textContent = fileSize;
        document.getElementById('modalSubtitlePath').textContent = movie.subTitleFilePath || 'N/A';
        
        const modalImage = document.getElementById('modalImage');
        if (movie.fullImageUrl) {
            modalImage.src = movie.fullImageUrl;
            modalImage.style.display = 'block';
        } else {
            modalImage.style.display = 'none';
        }

        this.movieModal.style.display = 'flex';
        setTimeout(() => {
            this.closeModalBtn.focus();
        }, 100);
    }

    closeMovieModal() {
        this.movieModal.style.display = 'none';
    }

    updatePagination(data) {
        const hasNextPage = data.movies && data.movies.length === this.pageSize;
        const hasPrevPage = this.currentPage > 1;

        this.prevBtn.disabled = !hasPrevPage;
        this.nextBtn.disabled = !hasNextPage;
        
        this.pageInfo.textContent = `Page ${this.currentPage}`;
        this.pagination.style.display = 'flex';
    }

    setView(view) {
        this.currentView = view;
        this.moviesContainer.className = `movies-container ${view}-view`;
        
        // Update button states
        this.gridViewBtn.classList.toggle('active', view === 'grid');
        this.listViewBtn.classList.toggle('active', view === 'list');
    }

    handleSearch() {
        this.searchQuery = this.searchInput.value.trim();
        this.renderMovies();
    }

    handlePageSizeChange() {
        this.pageSize = parseInt(this.pageSizeSelect.value);
        this.currentPage = 1;
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

// Initialize dashboard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('Dashboard initializing...');
    new DashboardController();
});

// Export for debugging
window.DashboardController = DashboardController;
window.SessionManager = SessionManager;
window.MoviesAPI = MoviesAPI;
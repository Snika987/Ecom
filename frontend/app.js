// Main JavaScript file for NovaCart e-commerce application
// Handles authentication, product display, and user interactions

(function () {
  'use strict';

  // Backend API base URL - change this if your backend runs on different port
  var API_BASE = 'https://localhost:7077/api';

  // ===== UTILITY FUNCTIONS =====

  /**
   * Display messages to user (success or error)
   * @param {HTMLElement} el - Message element to update
   * @param {string} text - Message text to display
   * @param {boolean} isError - Whether this is an error message
   */
  function setMessage(el, text, isError) {
    if (!el) return;
    if (text) {
      el.textContent = text;
      el.classList.add('show');
      el.style.borderColor = isError ? '#f85149' : '';
      el.style.color = isError ? '#f85149' : '';
    } else {
      el.textContent = '';
      el.classList.remove('show');
      el.style.borderColor = '';
      el.style.color = '';
    }
  }

  // ===== JWT TOKEN MANAGEMENT =====

  /**
   * Save JWT token to browser's local storage
   * @param {string} token - JWT token to save
   */
  function saveToken(token) { 
    try { 
      localStorage.setItem('jwt', token); 
    } catch (e) {
      console.error('Failed to save token:', e);
    } 
  }

  /**
   * Get JWT token from browser's local storage
   * @returns {string|null} JWT token or null if not found
   */
  function getToken() { 
    try { 
      return localStorage.getItem('jwt'); 
    } catch (e) { 
      console.error('Failed to get token:', e);
      return null; 
    } 
  }

  /**
   * Remove JWT token from browser's local storage
   */
  function clearToken() { 
    try { 
      localStorage.removeItem('jwt'); 
    } catch (e) {
      console.error('Failed to clear token:', e);
    } 
  }

  /**
   * Get authorization headers for API requests
   * @returns {object} Headers object with Bearer token
   */
  function authHeaders() {
    var t = getToken();
    return t ? { 'Authorization': 'Bearer ' + t } : {};
  }

  // ===== VIEW MANAGEMENT =====

  /**
   * Show authentication view (login/register forms)
   */
  function showAuth() {
    $('#auth-view').removeClass('spa-hidden');
    $('#products-view').addClass('spa-hidden');
  }

  /**
   * Show products view (product grid)
   */
  function showProducts() {
    $('#auth-view').addClass('spa-hidden');
    $('#products-view').removeClass('spa-hidden');
  }

  // ===== AUTHENTICATION PAGE FUNCTIONS =====

  /**
   * Initialize authentication page - setup login/register forms and event handlers
   */
  function initAuthPage() {
    var messageEl = document.getElementById('auth-message');
    var tabLogin = document.getElementById('tab-login');
    var tabRegister = document.getElementById('tab-register');
    var formLogin = document.getElementById('login-form');
    var formRegister = document.getElementById('register-form');

    /**
     * Switch between login and register tabs
     * @param {string} tab - 'login' or 'register'
     */
    function activate(tab) {
      if (tab === 'login') {
        tabLogin.classList.add('active');
        tabRegister.classList.remove('active');
        formLogin.classList.add('active');
        formRegister.classList.remove('active');
      } else {
        tabRegister.classList.add('active');
        tabLogin.classList.remove('active');
        formRegister.classList.add('active');
        formLogin.classList.remove('active');
      }
      setMessage(messageEl, ''); // Clear any previous messages
    }

    // Tab switching event handlers
    $('#tab-login').on('click', function () { activate('login'); });
    $('#tab-register').on('click', function () { activate('register'); });
    $('#go-register').on('click', function (e) { e.preventDefault(); activate('register'); });
    $('#go-login').on('click', function (e) { e.preventDefault(); activate('login'); });

    // Login form submission
    $('#btn-login').on('click', function () {
      setMessage(messageEl, '');
      var email = $('#login-email').val().trim();
      var password = $('#login-password').val();
      
      // Validate input
      if (!email || !password) {
        setMessage(messageEl, 'Please enter email and password', true);
        return;
      }
      
      // Send login request to backend
      $.ajax({
        method: 'POST',
        url: API_BASE + '/Auth/login',
        contentType: 'application/json',
        data: JSON.stringify({ email: email, password: password })
      }).done(function (res) {
        if (res && res.token) { 
          saveToken(res.token); 
          loadProductsAndShow(); // Switch to products view
        } else { 
          setMessage(messageEl, 'Unexpected response from server', true); 
        }
      }).fail(function (xhr) {
        var msg = (xhr && xhr.responseText) ? xhr.responseText : 'Login failed';
        setMessage(messageEl, msg.replace(/"/g, ''), true);
      });
    });

    // Registration form submission
    $('#btn-register').on('click', function () {
      setMessage(messageEl, '');
      var email = $('#reg-email').val().trim();
      var password = $('#reg-password').val();
      
      // Validate input
      if (!email || !password) {
        setMessage(messageEl, 'Please fill all fields', true);
        return;
      }
      
      // Send registration request to backend
      $.ajax({
        method: 'POST',
        url: API_BASE + '/Functions/RegisterUser',
        contentType: 'application/json',
        data: JSON.stringify({ email: email, password: password })
      }).done(function () {
        setMessage(messageEl, 'Account created. Please login.');
        activate('login'); // Switch to login tab
      }).fail(function (xhr) {
        var msg = (xhr && xhr.responseText) ? xhr.responseText : 'Registration failed';
        setMessage(messageEl, msg.replace(/"/g, ''), true);
      });
    });
  }

  // ===== PRODUCT DISPLAY FUNCTIONS =====

  /**
   * Create HTML for a product card
   * @param {object} p - Product object with name, price, description, etc.
   * @returns {string} HTML string for product card
   */
  function renderProductCard(p) {
    var imgSrc = p.image ? ('http://localhost:7077/Images/' + p.image) : 'https://via.placeholder.com/400x300?text=No+Image';
    var desc = p.description || 'No description';
    var qty = (p.stock == null ? 0 : p.stock);
    var price = (p.price != null ? p.price : 0);
    
    // Debug: log the image source
    console.log('Loading image:', imgSrc);
    
    return (
      '<div class="card" data-id="' + (p.pid || '') + '">' +
        '<img src="' + imgSrc + '" alt="' + (p.pname || 'Product') + '" onerror="console.log(\'Failed to load image: ' + imgSrc + '\')">' +
        '<div class="content">' +
          '<div class="title">' + (p.pname || 'Product') + '</div>' +
          '<div class="price">₹' + price + '</div>' +
          '<div class="desc">' + desc + '</div>' +
          '<div class="qty">Qty: ' + qty + '</div>' +
          '<div class="actions"><button class="btn primary btn-buy">Buy Now</button></div>' +
        '</div>' +
      '</div>'
    );
  }

  /**
   * Initialize products page - load products and setup event handlers
   */
  function initProductsPage() {
    var messageEl = document.getElementById('products-message');
    var token = getToken();
    
    // Check if user is logged in
    if (!token) { 
      showAuth(); 
      return; 
    }

    // Logout button handler
    $('#btn-logout').on('click', function () { 
      clearToken(); 
      showAuth(); 
    });

    // Show loading message
    setMessage(messageEl, 'Loading products...');
    
    // Fetch products from backend
    $.ajax({
      method: 'GET',
      url: API_BASE + '/Functions/ViewProducts',
      headers: authHeaders() // Include JWT token in request
    }).done(function (res) {
      setMessage(messageEl, '');
      var list = Array.isArray(res) ? res : (res && res.value ? res.value : []);
      var html = list.map(renderProductCard).join('');
      $('#products-grid').html(html);
    }).fail(function (xhr) {
      // Handle authentication errors
      if (xhr.status === 401 || xhr.status === 403) {
        clearToken(); 
        showAuth(); 
        return;
      }
      setMessage(messageEl, 'Failed to load products', true);
    });

    // Buy button click handler (shows success modal)
    $(document).on('click', '.btn-buy', function () {
      showModal('Order placed successfully 🎉', 'Thank you for your purchase.');
    });

    // Modal close button handler
    $('#modal-close').on('click', hideModal);
  }

  // ===== MODAL FUNCTIONS =====

  /**
   * Show modal dialog with title and message
   * @param {string} title - Modal title
   * @param {string} text - Modal message text
   */
  function showModal(title, text) {
    $('#modal-title').text(title);
    $('#modal-text').text(text);
    $('#modal').removeClass('hidden');
  }

  /**
   * Hide modal dialog
   */
  function hideModal() { 
    $('#modal').addClass('hidden'); 
  }

  // ===== APPLICATION INITIALIZATION =====

  /**
   * Load products and show products view
   */
  function loadProductsAndShow() {
    showProducts();
    initProductsPage();
  }

  /**
   * Initialize application when page loads
   */
  $(function () {
    initAuthPage(); // Setup authentication forms
    
    // Check if user is already logged in
    if (getToken()) {
      loadProductsAndShow(); // Show products if token exists
    } else {
      showAuth(); // Show login/register if no token
    }
  });
})();



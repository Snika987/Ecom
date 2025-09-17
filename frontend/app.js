(function () {
  'use strict';

  var API_BASE = 'https://localhost:7077/api';

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

  function saveToken(token) { try { localStorage.setItem('jwt', token); } catch (e) {} }
  function getToken() { try { return localStorage.getItem('jwt'); } catch (e) { return null; } }
  function clearToken() { try { localStorage.removeItem('jwt'); } catch (e) {} }

  function authHeaders() {
    var t = getToken();
    return t ? { 'Authorization': 'Bearer ' + t } : {};
  }

  function showAuth() {
    $('#auth-view').removeClass('spa-hidden');
    $('#products-view').addClass('spa-hidden');
  }
  function showProducts() {
    $('#auth-view').addClass('spa-hidden');
    $('#products-view').removeClass('spa-hidden');
  }

  function initAuthPage() {
    var messageEl = document.getElementById('auth-message');
    var tabLogin = document.getElementById('tab-login');
    var tabRegister = document.getElementById('tab-register');
    var formLogin = document.getElementById('login-form');
    var formRegister = document.getElementById('register-form');

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
      setMessage(messageEl, '');
    }

    $('#tab-login').on('click', function () { activate('login'); });
    $('#tab-register').on('click', function () { activate('register'); });
    $('#go-register').on('click', function (e) { e.preventDefault(); activate('register'); });
    $('#go-login').on('click', function (e) { e.preventDefault(); activate('login'); });

    $('#btn-login').on('click', function () {
      setMessage(messageEl, '');
      var email = $('#login-email').val().trim();
      var password = $('#login-password').val();
      if (!email || !password) {
        setMessage(messageEl, 'Please enter email and password', true);
        return;
      }
      $.ajax({
        method: 'POST',
        url: API_BASE + '/Auth/login',
        contentType: 'application/json',
        data: JSON.stringify({ email: email, password: password })
      }).done(function (res) {
        if (res && res.token) { saveToken(res.token); loadProductsAndShow(); }
        else { setMessage(messageEl, 'Unexpected response from server', true); }
      }).fail(function (xhr) {
        var msg = (xhr && xhr.responseText) ? xhr.responseText : 'Login failed';
        setMessage(messageEl, msg.replace(/"/g, ''), true);
      });
    });

    $('#btn-register').on('click', function () {
      setMessage(messageEl, '');
      var uid = $('#reg-uid').val().trim();
      var email = $('#reg-email').val().trim();
      var password = $('#reg-password').val();
      if (!uid || !email || !password) {
        setMessage(messageEl, 'Please fill all fields', true);
        return;
      }
      $.ajax({
        method: 'POST',
        url: API_BASE + '/Functions/RegisterUser',
        contentType: 'application/json',
        data: JSON.stringify({ uid: uid, email: email, password: password })
      }).done(function () {
        setMessage(messageEl, 'Account created. Please login.');
        activate('login');
      }).fail(function (xhr) {
        var msg = (xhr && xhr.responseText) ? xhr.responseText : 'Registration failed';
        setMessage(messageEl, msg.replace(/"/g, ''), true);
      });
    });
  }

  function renderProductCard(p) {
    var imgSrc = p.image ? ('../image/' + p.image) : 'https://via.placeholder.com/400x300?text=No+Image';
    var desc = p.description || 'No description';
    var qty = (p.stock == null ? 0 : p.stock);
    var price = (p.price != null ? p.price : 0);
    return (
      '<div class="card" data-id="' + (p.pid || '') + '">' +
        '<img src="' + imgSrc + '" alt="' + (p.pname || 'Product') + '">' +
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

  function initProductsPage() {
    var messageEl = document.getElementById('products-message');
    var token = getToken();
    if (!token) { showAuth(); return; }

    $('#btn-logout').on('click', function () { clearToken(); showAuth(); });

    setMessage(messageEl, 'Loading products...');
    $.ajax({
      method: 'GET',
      url: API_BASE + '/Functions/ViewProducts',
      headers: authHeaders()
    }).done(function (res) {
      setMessage(messageEl, '');
      var list = Array.isArray(res) ? res : (res && res.value ? res.value : []);
      var html = list.map(renderProductCard).join('');
      $('#products-grid').html(html);
    }).fail(function (xhr) {
      if (xhr.status === 401 || xhr.status === 403) {
        clearToken(); showAuth(); return;
      }
      setMessage(messageEl, 'Failed to load products', true);
    });

    $(document).on('click', '.btn-buy', function () {
      showModal('Order placed successfully 🎉', 'Thank you for your purchase.');
    });

    $('#modal-close').on('click', hideModal);
  }

  function showModal(title, text) {
    $('#modal-title').text(title);
    $('#modal-text').text(text);
    $('#modal').removeClass('hidden');
  }
  function hideModal() { $('#modal').addClass('hidden'); }

  function loadProductsAndShow() {
    showProducts();
    initProductsPage();
  }

  $(function () {
    initAuthPage();
    if (getToken()) {
      loadProductsAndShow();
    } else {
      showAuth();
    }
  });
})();



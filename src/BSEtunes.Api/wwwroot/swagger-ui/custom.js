/* paste your custom script here (the login panel + authorize logic).
   Example: the script you used earlier that stores token in localStorage
   and calls the Swagger UI programmatic authorize API.
*/

(function () {
    function addLoginPanel() {
        var topbar = document.querySelector('.swagger-ui .topbar') || document.querySelector('.topbar');
        if (!topbar) { setTimeout(addLoginPanel, 300); return; }

        var panel = document.createElement('div');
        panel.style.display = 'inline-flex';
        panel.style.alignItems = 'center';
        panel.style.marginLeft = '12px';

        panel.innerHTML =
            '<input id="swagger-username" placeholder="email" style="margin-right:6px;padding:4px;" />' +
            '<input id="swagger-password" type="password" placeholder="password" style="margin-right:6px;padding:4px;" />' +
            '<button id="swagger-login" style="margin-right:6px;padding:4px;">Login</button>' +
            '<button id="swagger-logout" style="margin-right:6px;padding:4px;">Logout</button>' +
            '<span id="swagger-login-msg" style="color:#999;font-size:smaller;margin-left:8px"></span>';

        topbar.appendChild(panel);

        document.getElementById('swagger-login').addEventListener('click', doLogin);
        document.getElementById('swagger-logout').addEventListener('click', doLogout);
        updateMsg();
    }

    async function doLogin() {
        var email = document.getElementById('swagger-username').value;
        var password = document.getElementById('swagger-password').value;
        var msg = document.getElementById('swagger-login-msg');
        msg.textContent = 'Logging in...';

        try {
            var res = await fetch('/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Email: email, Password: password })
            });

            if (!res.ok) {
                msg.textContent = 'Login failed';
                return;
            }

            var data = await res.json();
            var token = data?.access_token || data?.AccessToken || data?.accessToken;
            if (!token) {
                msg.textContent = 'No token received';
                console.warn('Login response', data);
                return;
            }

            localStorage.setItem('swagger_access_token', token);
            msg.textContent = 'Logged in';
            setSwaggerAuth(token);
        } catch (err) {
            console.error(err);
            msg.textContent = 'Error';
        }
    }

    function doLogout() {
        localStorage.removeItem('swagger_access_token');
        clearSwaggerAuth();
        updateMsg();
    }

    function updateMsg() {
        var msg = document.getElementById('swagger-login-msg');
        if (!msg) return;
        var token = localStorage.getItem('swagger_access_token');
        msg.textContent = token ? 'Authenticated' : 'Not logged in';
    }

    function setSwaggerAuth(token) {
        try {
            if (window.ui && typeof window.ui.preauthorizeApiKey === 'function') {
                window.ui.preauthorizeApiKey('Bearer', 'Bearer ' + token);
                return;
            }

            var system = window.ui && typeof window.ui.getSystem === 'function' ? window.ui.getSystem() : (window.ui || (window.ui && window.ui.getSystem && window.ui.getSystem()));
            if (system && system.authActions && typeof system.authActions.authorize === 'function') {
                var auth = {};
                auth['Bearer'] = {
                    name: 'Authorization',
                    schema: { type: 'http', scheme: 'bearer', bearerFormat: 'JWT' },
                    value: 'Bearer ' + token
                };
                system.authActions.authorize(auth);
                return;
            }

            attachRequestInterceptor();
        } catch (e) {
            console.warn('setSwaggerAuth fallback', e);
            attachRequestInterceptor();
        }
    }

    function clearSwaggerAuth() {
        try {
            if (window.ui && typeof window.ui.preauthorizeApiKey === 'function') {
                window.ui.preauthorizeApiKey('Bearer', '');
            } else {
                var system = window.ui && typeof window.ui.getSystem === 'function' ? window.ui.getSystem() : window.ui;
                if (system && system.authActions && typeof system.authActions.logout === 'function') {
                    system.authActions.logout();
                }
            }
        } catch (e) {
            console.warn('clearSwaggerAuth fallback', e);
        }
    }

    function attachRequestInterceptor() {
        try {
            if (!window.ui || !window.ui.getConfigs) {
                setTimeout(attachRequestInterceptor, 300);
                return;
            }
            var cfg = window.ui.getConfigs();
            var existing = cfg.requestInterceptor;
            cfg.requestInterceptor = function (req) {
                var token = localStorage.getItem('swagger_access_token');
                if (token) {
                    req.headers = req.headers || {};
                    req.headers['Authorization'] = 'Bearer ' + token;
                }
                if (existing && typeof existing === 'function') {
                    return existing(req) || req;
                }
                return req;
            };
        } catch (e) {
            console.warn('attachRequestInterceptor error', e);
        }
    }

    window.addEventListener('load', function () {
        setTimeout(addLoginPanel, 300);
        attachRequestInterceptor();
        var token = localStorage.getItem('swagger_access_token');
        if (token) {
            setTimeout(function () { setSwaggerAuth(token); }, 500);
        }
    });
})();
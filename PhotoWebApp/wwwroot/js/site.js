const originalFetch = window.fetch;

window.fetch = async (url, options = { }) => {
    const csrfToken = document.querySelector('meta[name="csrf-token"]').getAttribute('content');

// Add the CSRF token to the headers
const headers = options.headers || { };
headers['X-CSRF-TOKEN'] = csrfToken;

const defaultOptions = {
...options,
headers: {
...headers,
'Content-Type': headers['Content-Type'] || 'application/x-www-form-urlencoded',
        },
    };

return originalFetch(url, defaultOptions);
};

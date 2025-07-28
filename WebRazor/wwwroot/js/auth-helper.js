// Auth Helper for managing authentication tokens
class AuthHelper {
  constructor() {
    this.initializeToken();
  }

  initializeToken() {
    // Lấy token từ URL parameter (khi redirect từ login)
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get("token");

    if (token) {
      // Lưu token vào localStorage và sessionStorage
      localStorage.setItem("accessToken", token);
      sessionStorage.setItem("accessToken", token);

      // Xóa token khỏi URL
      const newUrl = window.location.pathname;
      window.history.replaceState({}, document.title, newUrl);

      console.log("Token saved successfully");
    }
  }

  getToken() {
    return (
      localStorage.getItem("accessToken") ||
      sessionStorage.getItem("accessToken") ||
      this.getCookie("AccessToken")
    );
  }

  setToken(token) {
    localStorage.setItem("accessToken", token);
    sessionStorage.setItem("accessToken", token);
  }

  clearToken() {
    localStorage.removeItem("accessToken");
    sessionStorage.removeItem("accessToken");
    document.cookie =
      "AccessToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
  }

  getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(";").shift();
    return null;
  }

  isAuthenticated() {
    return !!this.getToken();
  }

  logout() {
    this.clearToken();
    window.location.href = "/Account/Login";
  }
}

// Global instance
window.authHelper = new AuthHelper();

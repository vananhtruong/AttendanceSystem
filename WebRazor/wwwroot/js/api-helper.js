// API Helper for client-side API calls
class ApiHelper {
  constructor() {
    // Get base URL from current page or use default
    this.baseUrl = this.getBaseUrl();
    this.token = this.getToken();
  }

  getBaseUrl() {
    // Try to get from meta tag first
    const metaTag = document.querySelector('meta[name="api-base-url"]');
    if (metaTag && metaTag.content) {
      return metaTag.content;
    }

    // Fallback to current origin for development
    if (
      window.location.hostname === "localhost" ||
      window.location.hostname === "127.0.0.1"
    ) {
      return "https://localhost:7117/";
    }

    // Production fallback
    return "http://attendance-system.runasp.net/";
  }

  getToken() {
    return window.authHelper
      ? window.authHelper.getToken()
      : localStorage.getItem("accessToken") ||
          sessionStorage.getItem("accessToken") ||
          this.getCookie("AccessToken");
  }

  getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(";").shift();
    return null;
  }

  getHeaders() {
    const headers = {
      "Content-Type": "application/json",
    };

    if (this.token) {
      headers["Authorization"] = `Bearer ${this.token}`;
    }

    return headers;
  }

  async get(endpoint) {
    try {
      console.log(`API GET: ${this.baseUrl}${endpoint}`);
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: "GET",
        headers: this.getHeaders(),
      });

      console.log(`API GET Response Status: ${response.status}`);

      if (response.status === 401) {
        // Token expired or invalid
        this.handleUnauthorized();
        return null;
      }

      if (!response.ok) {
        console.error(
          `API GET Error: ${response.status} ${response.statusText}`
        );
        const errorText = await response.text();
        console.error(`API GET Error Response: ${errorText}`);
        return null;
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const data = await response.json();
        console.log(`API GET Response Data:`, data);
        return data;
      } else {
        console.warn(`API GET Response is not JSON: ${contentType}`);
        const textData = await response.text();
        console.log(`API GET Response Text:`, textData);
        return null;
      }
    } catch (error) {
      console.error("API GET Error:", error);
      return null;
    }
  }

  async post(endpoint, data) {
    try {
      console.log(`API POST: ${this.baseUrl}${endpoint}`, data);
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: "POST",
        headers: this.getHeaders(),
        body: JSON.stringify(data),
      });

      console.log(`API POST Response Status: ${response.status}`);

      if (response.status === 401) {
        this.handleUnauthorized();
        return null;
      }

      if (!response.ok) {
        console.error(
          `API POST Error: ${response.status} ${response.statusText}`
        );
        const errorText = await response.text();
        console.error(`API POST Error Response: ${errorText}`);
        return null;
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const data = await response.json();
        console.log(`API POST Response Data:`, data);
        return data;
      } else {
        console.warn(`API POST Response is not JSON: ${contentType}`);
        const textData = await response.text();
        console.log(`API POST Response Text:`, textData);
        return null;
      }
    } catch (error) {
      console.error("API POST Error:", error);
      return null;
    }
  }

  async put(endpoint, data) {
    try {
      console.log(`API PUT: ${this.baseUrl}${endpoint}`, data);
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: "PUT",
        headers: this.getHeaders(),
        body: JSON.stringify(data),
      });

      console.log(`API PUT Response Status: ${response.status}`);

      if (response.status === 401) {
        this.handleUnauthorized();
        return null;
      }

      if (!response.ok) {
        console.error(
          `API PUT Error: ${response.status} ${response.statusText}`
        );
        const errorText = await response.text();
        console.error(`API PUT Error Response: ${errorText}`);
        return null;
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const data = await response.json();
        console.log(`API PUT Response Data:`, data);
        return data;
      } else {
        console.warn(`API PUT Response is not JSON: ${contentType}`);
        const textData = await response.text();
        console.log(`API PUT Response Text:`, textData);
        return null;
      }
    } catch (error) {
      console.error("API PUT Error:", error);
      return null;
    }
  }

  async delete(endpoint) {
    try {
      console.log(`API DELETE: ${this.baseUrl}${endpoint}`);
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: "DELETE",
        headers: this.getHeaders(),
      });

      console.log(`API DELETE Response Status: ${response.status}`);

      if (response.status === 401) {
        this.handleUnauthorized();
        return false;
      }

      return response.ok;
    } catch (error) {
      console.error("API DELETE Error:", error);
      return false;
    }
  }

  handleUnauthorized() {
    // Clear tokens and redirect to login
    localStorage.removeItem("accessToken");
    sessionStorage.removeItem("accessToken");
    document.cookie =
      "AccessToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";

    // Show message and redirect
    alert("Session expired. Please login again.");
    window.location.href = "/Account/Login";
  }

  // OData specific methods
  async getOData(endpoint, params = {}) {
    const queryString = new URLSearchParams(params).toString();
    const fullEndpoint = queryString ? `${endpoint}?${queryString}` : endpoint;
    return await this.get(fullEndpoint);
  }
}

// Global instance
window.apiHelper = new ApiHelper();

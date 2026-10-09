// Docker and deployed hosting use a same-origin API proxy. Only the optional
// local static-development server points directly to the ASP.NET launch port.
const localFrontend = ["localhost", "127.0.0.1"].includes(location.hostname)
  && location.port === "5500";
window.SEHATY_API_BASE_URL = localFrontend ? "http://localhost:5295" : "";

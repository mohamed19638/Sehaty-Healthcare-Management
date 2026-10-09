const app = document.querySelector("#app");
let page = location.hash.slice(1) || "dashboard";
const apiBase = window.SEHATY_API_BASE_URL || "";
let language = localStorage.getItem("sehaty-language") || "en";

function languageButton() {
  return `<button class="language-toggle" type="button" onclick="setLanguage('${language === "en" ? "ar" : "en"}')" aria-label="Switch language">${language === "en" ? "العربية" : "English"}</button>`;
}

function setLanguage(nextLanguage) {
  language = nextLanguage === "ar" ? "ar" : "en";
  localStorage.setItem("sehaty-language", language);
  window.SehtyI18n?.applyLanguage();
  view();
}

const me = () => JSON.parse(localStorage.getItem("user") || "{}"),
  esc = (s) =>
    String(s ?? "").replace(
      /[&<>"']/g,
      (c) =>
        ({
          "&": "&amp;",
          "<": "&lt;",
          ">": "&gt;",
          '"': "&quot;",
          "'": "&#39;",
        })[c],
    );

const date = (d) => new Date(d).toLocaleDateString(language === "ar" ? "ar-EG" : "en-US", { day: "numeric", month: "short", year: "numeric" }),
  time = (d) => new Date(d).toLocaleTimeString(language === "ar" ? "ar-EG" : "en-US", { hour: "2-digit", minute: "2-digit" }),
  today = () => {
    const local = new Date();
    local.setMinutes(local.getMinutes() - local.getTimezoneOffset());
    return local.toISOString().slice(0, 10);
  },
  go = (p) => (location.hash = "#" + p);

async function api(path, opt = {}) {
  let r = await fetch(apiBase + "/api" + path, {
    ...opt,
    headers: {
      "Content-Type": "application/json",
      ...(localStorage.token
        ? { Authorization: "Bearer " + localStorage.token }
        : {}),
      ...(opt.headers || {}),
    },
  });
  if (r.status === 401) {
    localStorage.clear();
    go("login");
    throw Error("Please sign in to continue");
  }
  if (r.status === 204) return null;
  let j = await r.json().catch(() => null);
  if (!r.ok) throw Error(j?.message || "Request failed");
  return j;
}

function auth(reg = false) {
  app.innerHTML = `<div class="auth-screen">${languageButton()}<section class="auth-art"><div class="logo">Sehaty<b>+</b></div><div><h1>Your health,<br>in one place.</h1><p>Track your measurements, manage your medications, and book your doctor appointments. Simple steps for better health.</p></div><small>Sehaty+ · Your daily health companion</small></section><section class="auth-card"><div class="logo" style="color:#123f3e">Sehaty<b>+</b></div><h2>${reg ? "Create your account" : "Welcome back 👋"}</h2><p>Track your health and daily data</p><form id="authForm">${reg ? '<div class="field"><label>Full Name</label><input name="fullName" required></div>' : ""}<div class="field"><label>Email</label><input name="email" type="email" required></div><div class="field"><label>Password</label><input name="password" type="password" required minlength="8"></div>${reg ? '<div class="field"><label>Date of Birth</label><input name="dateOfBirth" type="date" required></div><div class="field"><label>Gender</label><select name="gender"><option value="">Select</option><option>Female</option><option>Male</option><option>Prefer not to say</option></select></div>' : ""}<div id="err"></div><button class="btn block">${reg ? "Create Account" : "Sign In"}</button></form><div class="auth-foot">${reg ? "Already have an account?" : "Don't have an account?"} <span class="link" onclick="go('${reg ? "login" : "register"}')">${reg ? "Sign In" : "Create Account"}</span></div>${!reg ? '<div class="auth-foot">Demo account: demo@sehaty.com · Demo123!</div>' : ""}</section></div>`;
  window.SehtyI18n?.applyLanguage();
  document.querySelector("#authForm").onsubmit = async (e) => {
    e.preventDefault();
    let f = Object.fromEntries(new FormData(e.target));
    try {
      if (reg) {
        await api("/auth/register", {
          method: "POST",
          body: JSON.stringify(f),
        });
        go("login");
      } else {
        let r = await api("/auth/login", {
          method: "POST",
          body: JSON.stringify(f),
        });
        localStorage.token = r.token;
        localStorage.user = JSON.stringify(r.user);
        go("dashboard");
      }
    } catch (x) {
      document.querySelector("#err").innerHTML =
        `<div class="notice">${esc(x.message)}</div>`;
    }
  };
}

const nav = [
  ["dashboard", "⌂", "Dashboard"],
  ["health", "♡", "Health Measurements"],
  ["bmi", "⚖", "BMI Calculator"],
  ["fitness", "↗", "Activity & Exercises"],
  ["medications", "✚", "Medications"],
  ["records", "▤", "Medical Records"],
  ["doctors", "⚕", "Doctors & Appointments"],
  ["pharmacies", "⌖", "Pharmacies"],
  ["meals", "🍽", "Healthy Meals"],
];

function signOut() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  page = "login";
  location.hash = "#login";
  auth(false);
}

function shell(title, subtitle, html) {
  app.innerHTML = `<aside class="sidebar"><div class="logo">Sehaty<b>+</b></div><div class="nav-label">Main Menu</div>${nav.map(([id, i, n]) => `<a class="nav-item ${page === id ? "active" : ""}" href="#${id}"><i>${i}</i>${n}</a>`).join("")}<div class="sidebar-bottom">Your health data is private.<br>Track it regularly.</div><a class="nav-item" href="#logout"><i>↪</i>Sign Out</a></aside><main class="main"><header class="topbar"><div class="welcome"><h1>${title}</h1><p>${subtitle || ""}</p></div><div class="top-actions">${languageButton()}<div class="profile"><div class="avatar">${esc((me().fullName || "U")[0])}</div><div><b>${esc(me().fullName || "User")}</b><small>Personal Account</small></div></div><button class="signout-btn" onclick="signOut()" title="Sign Out" aria-label="Sign Out">↪ Sign Out</button></div></header>${html}</main>`;
  window.SehtyI18n?.applyLanguage();
}

function metric(n, v, u, i, note = "Latest reading") {
  return `<article class="metric"><div class="metric-head"><span>${n}</span><span class="metric-icon">${i}</span></div><div class="metric-number">${v ?? "—"} <em>${u || ""}</em></div><div class="metric-note">${note}</div></article>`;
}

function chart(rows) {
  let a = [...rows].reverse().slice(-8);
  if (a.length < 2)
    return '<div class="empty">Add two or more measurements to view trends over time.</div>';
  let w = 650,
    h = 180,
    p = 24,
    series = [
      ["Pulse", (x) => x.heartRate == null ? NaN : Number(x.heartRate), "#16877f"],
      ["Sugar", (x) => x.bloodSugar == null ? NaN : Number(x.bloodSugar), "#ef8c75"],
      ["Weight", (x) => x.weight == null ? NaN : Number(x.weight), "#e8b65e"],
      [
        "Systolic BP",
        (x) => x.bloodPressure ? Number(String(x.bloodPressure).split("/")[0]) : NaN,
        "#8b78c8",
      ],
    ],
    lines = series
      .map(([name, get, color]) => {
        let vv = a.map(get),
          valid = vv.filter(Number.isFinite);
        if (valid.length < 2) return "";
        let lo = Math.min(...valid),
          hi = Math.max(...valid),
          ps = vv.map((v, i) =>
            Number.isFinite(v)
              ? [
                  p + (i * (w - 2 * p)) / (a.length - 1),
                  h -
                    p -
                    (hi === lo ? 0.5 : (v - lo) / (hi - lo)) * (h - 2 * p),
                ]
              : null,
          ),
          d = ps
            .map((q, i) => (q ? (i ? "L" : "M") + q[0] + "," + q[1] : null))
            .filter(Boolean)
            .join(" ");
        return `<path d="${d}" fill="none" stroke="${color}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>${ps.map((q, i) => (q ? `<circle cx="${q[0]}" cy="${q[1]}" r="4" fill="white" stroke="${color}" stroke-width="3"><title>${name}: ${get(a[i])} · ${date(a[i].date)} ${time(a[i].date)}</title></circle>` : "")).join("")}`;
      })
      .join("");
  return `<svg viewBox="0 0 ${w} ${h}" preserveAspectRatio="none">${[0.2, 0.5, 0.8].map((y) => `<line class="chart-grid" x1="${p}" x2="${w - p}" y1="${h * y}" y2="${h * y}"/>`).join("")}${lines}</svg><div class="chart-legend"><span>Pulse</span><span>Sugar</span><span>Weight (kg)</span><span>Systolic BP</span></div>`;
}

function mlist(rows) {
  return rows.length
    ? `<div>${rows.map((x) => `<div class="record"><div><div class="record-main">${x.bloodPressure ? "BP " + esc(x.bloodPressure) : ""}${x.heartRate ? " · Pulse " + x.heartRate : ""}${x.bloodSugar ? " · Sugar " + x.bloodSugar : ""}${x.weight ? " · Weight " + x.weight + " kg" : ""}</div><div class="record-sub">Recorded reading</div></div><span class="record-value">${date(x.date)}</span><span class="record-time">${time(x.date)}</span></div>`).join("")}</div>`
    : '<div class="empty">No measurements recorded yet.</div>';
}

async function dashboard() {
  let d = await api("/dashboard"),
    m = d.measurements[0] || {},
    next = d.appointments[0];

  let bmiData = null;
  try { bmiData = await api("/bmi"); } catch { }

  shell(
    "Hello, " + esc(me().fullName || "") + " 👋",
    "Your health, in one place.",
    `<div class="metrics">${metric("Heart Rate", m.heartRate, "bpm", "♡", m.date ? date(m.date) + " · " + time(m.date) : "No reading")}${metric("Blood Pressure", m.bloodPressure, "mmHg", "⌁", m.date ? date(m.date) + " · " + time(m.date) : "No reading")}${metric("Blood Sugar", m.bloodSugar, "mg/dL", "◉", m.date ? date(m.date) + " · " + time(m.date) : "No reading")}${metric("Weight", m.weight, "kg", "↕", m.date ? date(m.date) + " · " + time(m.date) : "No reading")}</div><div class="dash-grid"><div><section class="panel"><div class="panel-head"><h3>Measurement Trends</h3><span>Last 8 readings · Hover over dots for time</span></div><div class="chart-wrap">${chart(d.measurements)}</div></section><section class="panel"><div class="panel-head"><h3>Recent Measurements</h3><a class="link" href="#health">View All</a></div>${mlist(d.measurements.slice(0, 5))}</section></div><div><section class="panel"><div class="panel-head"><h3>BMI Calculator</h3><a class="link" href="#bmi">Details</a></div>
    ${bmiData && bmiData.bmi ? `
      <div style="text-align:center">
        <div class="bmi-ring" style="--progress:${Math.min(100, (bmiData.bmi / 40) * 100)}%">
          <div class="bmi-center"><b>${bmiData.bmi.toFixed(1)}</b><small>${esc(bmiData.category)}</small></div>
        </div>
        <p style="margin:12px 0;font-size:13px">Height: <b>${bmiData.height} cm</b> · Weight: <b>${bmiData.weight} kg</b></p>
      </div>` : `
      <div class="empty">Update your height and weight to see your BMI.</div>`}
      <button class="btn light" style="width:100%;margin-top:10px" onclick="bmiModal()">Update Details</button>
    </section><section class="panel"><div class="panel-head"><h3>Next Appointment</h3><a class="link" href="#doctors">Book</a></div>${next ? `<div class="record"><div><div class="record-main">${esc(next.doctor?.name)}</div><div class="record-sub">${esc(next.doctor?.specialization)}</div></div><span class="record-value">${date(next.appointmentDate)}</span><span class="record-time">${time(next.appointmentDate)}</span></div>` : '<div class="empty">No upcoming appointments.</div>'}</section><section class="panel"><div class="panel-head"><h3>Quick Actions</h3></div><div class="quick-grid"><button class="quick" onclick="modal('measurement')"><b>＋</b>New Measurement</button><button class="quick" onclick="go('medications')"><b>＋</b>Add Medication</button><button class="quick" onclick="go('doctors')"><b>＋</b>Book Doctor</button><button class="quick" onclick="go('bmi')"><b>＋</b>Check BMI</button></div></section></div></div>`,
  );
}

const forms = {
  measurement: [
    "Add Health Measurement",
    "/healthmeasurements",
    [
      ["weight", "Weight (kg)", "number"],
      ["bloodPressure", "Blood Pressure e.g. 120/80", "text"],
      ["bloodSugar", "Blood Sugar mg/dL", "number"],
      ["bloodSugarWasFasting", "Was this a fasting blood glucose test?", "select", [["", "Unknown / not recorded"], ["true", "Yes — fasting"], ["false", "No — not fasting"]]],
      ["heartRate", "Heart Rate bpm", "number"],
      ["heartRateWasAtRest", "Was your pulse measured at rest for at least 10 minutes?", "select", [["", "Unknown / not recorded"], ["true", "Yes — rested at least 10 minutes"], ["false", "No — after activity"]]],
      ["date", "Date & Time", "datetime-local"],
    ],
  ],
  medication: [
    "Add Medication",
    "/medications",
    [
      ["name", "Medication Name", "text"],
      ["dosage", "Dosage", "text"],
      ["instructions", "Instructions", "text"],
      ["startDate", "Start Date", "date"],
      ["endDate", "End Date", "date"],
    ],
  ],
  fitness: [
    "Log Activity",
    "/fitness",
    [
      ["activityName", "Activity Type", "text"],
      ["durationMinutes", "Duration (minutes)", "number"],
      ["date", "Date & Time", "datetime-local"],
      ["notes", "Notes", "text"],
    ],
  ],
  record: [
    "Add Medical Record",
    "/medicalrecords",
    [
      ["condition", "Medical Condition", "text"],
      ["description", "Description", "text"],
      ["diagnosisDate", "Diagnosis Date", "date"],
      ["notes", "Notes", "text"],
    ],
  ],
  lab: [
    "Add Lab Result",
    "/labresults",
    [
      ["testName", "Test Name", "text"],
      ["result", "Result", "text"],
      ["unit", "Unit", "text"],
      ["testDate", "Test Date", "date"],
      ["notes", "Notes", "text"],
    ],
  ],
};

function modal(k) {
  let [title, url, fields] = forms[k];
  document.querySelector(".modal-bg")?.remove();
  document.body.insertAdjacentHTML(
    "beforeend",
    `<div class="modal-bg" onclick="if(event.target===this)this.remove()"><form class="modal" id="entry"><h3>${title}</h3><div class="form-grid">${fields.map(([n, label, type, options]) => `<div class="field ${["notes", "instructions"].includes(n) ? "wide" : ""}"><label for="${n}">${label}</label>${type === "select" ? `<select id="${n}" name="${n}">${options.map(([value, text]) => `<option value="${value}">${text}</option>`).join("")}</select>` : `<input id="${n}" name="${n}" type="${type}" ${type === "datetime-local" ? `value="${(() => { const local = new Date(); local.setMinutes(local.getMinutes() - local.getTimezoneOffset()); return local.toISOString().slice(0, 16); })()}"` : ["date", "diagnosisDate", "testDate", "startDate"].includes(n) ? `value="${today()}"` : ""} ${["name", "activityName", "condition", "testName", "result"].includes(n) ? "required" : ""} ${type === "number" ? 'min="0" step="0.1"' : ""}>`}</div>`).join("")}</div><div id="formError"></div><button class="btn">Save</button> <button type="button" class="btn ghost" onclick="document.querySelector('.modal-bg').remove()">Cancel</button></form></div>`,
  );
  document.querySelector("#entry").onsubmit = async (e) => {
    e.preventDefault();
    let o = Object.fromEntries(new FormData(e.target));
    fields.forEach(([n, , t]) => {
      if (t === "number") o[n] = o[n] ? Number(o[n]) : null;
      if (t === "datetime-local") o[n] = new Date(o[n]).toISOString();
      if (t === "select") o[n] = o[n] === "" ? null : o[n] === "true";
    });
    try {
      await api(url, { method: "POST", body: JSON.stringify(o) });
      document.querySelector(".modal-bg").remove();
      view();
    } catch (x) {
      document.querySelector("#formError").innerHTML = `<div class="notice">${esc(x.message)}</div>`;
    }
  };
}

function bmiModal() {
  document.querySelector(".modal-bg")?.remove();
  document.body.insertAdjacentHTML(
    "beforeend",
    `<div class="modal-bg" onclick="if(event.target===this)this.remove()"><form class="modal" id="bmiForm"><h3>Update Height & Weight</h3><div class="field"><label>Height (cm)</label><input name="height" type="number" required min="50" max="300" step="0.1"></div><div class="field"><label>Weight (kg)</label><input name="weight" type="number" required min="2" max="500" step="0.1"></div><button class="btn">Save</button> <button type="button" class="btn ghost" onclick="document.querySelector('.modal-bg').remove()">Cancel</button></form></div>`,
  );
  document.querySelector("#bmiForm").onsubmit = async (e) => {
    e.preventDefault();
    let f = Object.fromEntries(new FormData(e.target));
    try {
      await api("/bmi", { method: "PUT", body: JSON.stringify({ height: Number(f.height), weight: Number(f.weight) }) });
      document.querySelector(".modal-bg").remove();
      view();
    } catch (x) {
      alert(x.message);
    }
  };
}

function measurementSummary(rows) {
  return rows.map(x => [
    `${date(x.date)} ${time(x.date)}`,
    `Blood pressure: ${x.bloodPressure || "Not recorded"} mmHg (${x.bloodPressureStatus || "not assessed"})`,
    `Blood sugar: ${x.bloodSugar ?? "Not recorded"} mg/dL (${x.bloodSugarStatus || "not assessed"})`,
    `Heart rate: ${x.heartRate ?? "Not recorded"} bpm (${x.heartRateStatus || "not assessed"})`,
    `Weight: ${x.weight ?? "Not recorded"} kg`,
  ].join("\n")).join("\n\n");
}

async function shareHealth(destination) {
  if (!confirm("This will share your health measurements and timestamps. Continue only if you are comfortable sharing this sensitive information.")) return;
  try {
    const rows = await api("/healthmeasurements");
    const summary = `My health measurements\n\n${measurementSummary(rows) || "No measurements recorded."}`;
    const text = window.SehtyI18n?.translate(summary) || summary;
    if (destination === "whatsapp") {
      window.open(`https://wa.me/?text=${encodeURIComponent(text)}`, "_blank", "noopener,noreferrer");
      return;
    }
    if (destination === "messenger") {
      await navigator.clipboard.writeText(text);
      window.open("https://www.messenger.com/", "_blank", "noopener,noreferrer");
      alert("Your summary was copied. Choose a Messenger conversation and paste it yourself.");
      return;
    }
    if (navigator.share) await navigator.share({ title: "My Health Measurements", text });
    else await navigator.clipboard.writeText(text);
  } catch (error) {
    if (error.name !== "AbortError") alert(error.message || "Sharing is not available in this browser.");
  }
}

async function downloadMeasurementsXlsx() {
  const response = await fetch(`${apiBase}/api/export/measurements/xlsx`, {
    headers: { Authorization: `Bearer ${localStorage.token}` },
  });
  if (!response.ok) throw new Error("Could not export measurements.");
  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement("a");
  link.href = url;
  link.download = "health-measurements.xlsx";
  link.click();
  URL.revokeObjectURL(url);
}

async function printMeasurements() {
  if (!confirm("Your health measurement history will be opened in a print view. Choose ‘Save as PDF’ in the browser to save a PDF. Continue?")) return;
  const printWindow = window.open("", "_blank");
  if (!printWindow) {
    alert("Allow pop-ups for this site to create a PDF.");
    return;
  }
  try {
    const t = (value) => window.SehtyI18n?.translate(value) || value;
    const rows = await api("/healthmeasurements");
    const tableRows = rows.map(x => `<tr><td>${esc(date(x.date))} ${esc(time(x.date))}</td><td>${esc(x.weight ?? "—")} kg</td><td>${esc(x.bloodPressure || "—")} mmHg<br>${esc(x.bloodPressureStatus || "")}</td><td>${esc(x.bloodSugar ?? "—")} mg/dL<br>${esc(x.bloodSugarStatus || "")}</td><td>${esc(x.heartRate ?? "—")} bpm<br>${esc(x.heartRateStatus || "")}</td></tr>`).join("");
    const isArabic = language === "ar";
    printWindow.document.write(`<!doctype html><html lang="${language}" dir="${isArabic ? "rtl" : "ltr"}"><head><meta charset="utf-8"><title>${t("Health Measurements")}</title><style>body{font:14px Arial,sans-serif;padding:24px;color:#123f3e;direction:${isArabic ? "rtl" : "ltr"}}h1{font-size:22px}p{color:#555}table{width:100%;border-collapse:collapse}th,td{border:1px solid #ccd8d6;padding:9px;text-align:start}th{background:#eaf3f1}@media print{body{padding:0}}</style></head><body><h1>${t("Health Measurements")}</h1><p>${t("Private health information")} · ${t("Generated")} ${new Date().toLocaleString(isArabic ? "ar-EG" : "en-US")}</p><table><thead><tr><th>${t("Date and time")}</th><th>${t("Weight")}</th><th>${t("Blood pressure")}</th><th>${t("Blood sugar")}</th><th>${t("Heart rate")}</th></tr></thead><tbody>${tableRows || `<tr><td colspan="5">${t("No measurements recorded.")}</td></tr>`}</tbody></table><script>window.onload=()=>window.print()<\/script></body></html>`);
    printWindow.document.close();
  } catch (error) {
    printWindow.close();
    alert(error.message);
  }
}

async function health() {
  let r = await api("/healthmeasurements");
  shell(
    "Health Measurements",
    "Record measurements when taken to accurately compare readings.",
    `<div class="page-title"><div><h2>Indicators over time</h2><p>The time of each measurement appears next to its date.</p></div>
    <div class="export-actions">
      <button class="btn light" onclick="printMeasurements()">Print / Save as PDF</button>
      <button class="btn light" onclick="downloadMeasurementsXlsx()">Download Excel</button>
      <button class="btn light" onclick="shareHealth('whatsapp')">WhatsApp</button>
      <button class="btn light" onclick="shareHealth('messenger')">Messenger</button>
      <button class="btn" onclick="modal('measurement')">＋ Add Measurement</button>
    </div></div>
    <details class="notice medical-note"><summary>Health reference ranges and safety information</summary>
      <div class="medical-note-content"><b>General adult reference information (not a diagnosis):</b>
      <ul class="reference-ranges">
        <li><b>Blood pressure:</b> about 90/60 mmHg to below 120/80 is a usual resting adult range. The American Heart Association labels 120–129 and below 80 as elevated; 130/80 or above is above its normal category.</li>
        <li><b>Fasting blood glucose:</b> below 100 mg/dL is within the usual fasting-test range; below 70 is a low reading. 100–125 is above the usual fasting range; 126 or above needs clinical testing and is not a diagnosis from this app.</li>
        <li><b>Resting heart rate:</b> 60–100 beats per minute for a typical adult at rest.</li>
      </ul>
      Blood sugar is classified only when marked as a fasting test, and pulse only when marked as measured after resting for at least 10 minutes. These screening references are not personalized targets; home-meter readings and diabetes treatment targets may differ. Readings for users under 18 are not classified. A single result can vary with technique, pregnancy, medicines, health history, and other circumstances. Repeat measurements correctly and contact a healthcare professional for personal advice. If you have severe or concerning symptoms, seek urgent medical care.
      <div class="source-links">Sources: <a href="https://www.heart.org/en/health-topics/high-blood-pressure/understanding-blood-pressure-readings" target="_blank" rel="noopener noreferrer">American Heart Association — blood pressure categories</a> · <a href="https://medlineplus.gov/ency/article/002341.htm" target="_blank" rel="noopener noreferrer">MedlinePlus — typical resting adult vital signs</a> · <a href="https://www.cdc.gov/diabetes/diabetes-testing/index.html" target="_blank" rel="noopener noreferrer">CDC — fasting glucose tests</a> · <a href="https://medlineplus.gov/ency/article/003399.htm" target="_blank" rel="noopener noreferrer">MedlinePlus — resting pulse</a></div>
      </div>
    </details>
    <section class="panel"><div class="panel-head"><h3>Readings Chart</h3><span>Last 8 records</span></div><div class="chart-wrap">${chart(r)}</div></section>
    <section class="panel"><div class="panel-head"><h3>Measurement Log · Newest first</h3><span>${r.length} readings</span></div>
    ${r.length ? `<div>${r.map((x) => `<div class="record"><div><div class="record-main">${x.bloodPressure ? "BP " + esc(x.bloodPressure) : ""}${x.heartRate ? " · Pulse " + x.heartRate : ""}${x.bloodSugar ? " · Sugar " + x.bloodSugar : ""}${x.weight ? " · Weight " + x.weight + " kg" : ""}</div><div class="record-sub">${x.bloodPressure ? `BP: ${esc(x.bloodPressureStatus)} · ` : ""}${x.bloodSugar ? `Sugar: ${esc(x.bloodSugarStatus)} · ` : ""}${x.heartRate ? `Pulse: ${esc(x.heartRateStatus)}` : ""}</div></div><span class="record-value">${date(x.date)}</span><span class="record-time">${time(x.date)}</span></div>`).join("")}</div>` : '<div class="empty">No measurements recorded yet.</div>'}</section>`,
  );
}

async function meds() {
  let r = await api("/medications");
  shell(
    "Medications",
    "Track your doses and start dates.",
    `<div class="page-title"><h2>Medication List</h2><button class="btn" onclick="modal('medication')">＋ Add Medication</button></div><section class="panel">${r.length ? `<div class="table-wrap"><table><thead><tr><th>Medication Name</th><th>Dosage</th><th>Instructions</th><th>Start Date</th><th></th></tr></thead><tbody>${r.map((x) => `<tr><td><b>${esc(x.name)}</b></td><td>${esc(x.dosage || "—")}</td><td>${esc(x.instructions || "—")}</td><td>${date(x.startDate)}</td><td><button class="btn danger" onclick="del('/medications/${x.id}')">Delete</button></td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">No medications added yet.</div>'}</section>`,
  );
}

async function bmi() {
  let data;
  try { data = await api("/bmi"); } catch { data = null; }
  shell("BMI Calculator", "Body Mass Index based on your saved height and latest recorded weight.",
    `<div class="page-title"><div><h2>Your BMI</h2><p>BMI = weight (kg) ÷ height (m)²</p></div><button class="btn" onclick="bmiModal()">Update Height & Weight</button></div>
    <div class="dash-grid">
      <section class="panel">
        ${data && data.bmi ? `
          <div style="text-align:center;padding:20px">
            <div class="bmi-ring" style="--progress:${Math.min(100, (data.bmi / 40) * 100)}%">
              <div class="bmi-center"><b>${data.bmi.toFixed(1)}</b><small>${esc(data.category)}</small></div>
            </div>
            <p style="margin:12px 0;font-size:13px">Height: <b>${data.height} cm</b> · Weight: <b>${data.weight} kg</b></p>
            <div class="notice" style="text-align:left">
              <b>Important:</b> BMI is a screening measure, not a diagnosis. It may be interpreted differently for children, pregnant individuals, athletes, and some other circumstances. BMI alone does not determine your health. Please consult a healthcare professional for personalized advice.
            </div>
          </div>` : `
          <div class="empty">Please update your height and weight to calculate your BMI.</div>`}
      </section>
      <section class="panel">
        <h3>BMI Categories (Adults)</h3>
        <div style="font-size:12px;line-height:2.2;padding:10px">
          <div>⬜ <b>Underweight:</b> BMI below 18.5</div>
          <div>🟩 <b>Healthy range:</b> BMI 18.5 – 24.9</div>
          <div>🟨 <b>Overweight:</b> BMI 25.0 – 29.9</div>
          <div>🟥 <b>Obesity:</b> BMI 30.0 or above</div>
          <p style="color:#718381;font-size:11px;margin-top:8px">For adults age 20+. <a href="https://www.cdc.gov/bmi/adult-calculator/bmi-categories.html" target="_blank" rel="noopener noreferrer">CDC adult BMI categories</a>.</p>
        </div>
      </section>
    </div>`);
}

async function fitness() {
  let r = await api("/fitness");
  let ex = [];
  try { ex = await api("/exercises"); } catch { }

  shell(
    "Activity & Exercises",
    "Record your workouts and activity.",
    `<div class="page-title"><h2>Activity Summary</h2><button class="btn" onclick="modal('fitness')">＋ Log Activity</button></div><div class="metrics">${metric("Activities", r.length, "", "↗", "Total logs")}${metric(
      "Workout Time",
      r.reduce((s, x) => s + x.durationMinutes, 0),
      "min",
      "◷",
      "Total duration",
    )}${metric("Latest Activity", r[0]?.activityName || "—", "", "🏃", r[0] ? date(r[0].date) + " · " + time(r[0].date) : "None")}</div>
    <section class="panel"><div class="panel-head"><h3>Recorded Activities</h3></div>${r.length ? `<div class="table-wrap"><table><thead><tr><th>Activity</th><th>Time</th><th>Duration</th><th>Notes</th></tr></thead><tbody>${r.map((x) => `<tr><td>${esc(x.activityName)}</td><td>${date(x.date)} · ${time(x.date)}</td><td>${x.durationMinutes} min</td><td>${esc(x.notes || "—")}</td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">You haven\'t logged an activity yet.</div>'}</section>
    <section class="panel"><div class="panel-head"><h3>Exercise Videos</h3></div>
    ${ex && ex.length ? `<div class="cards-grid">${ex.map(x => `<article class="doctor-card" style="padding:10px">
        <h4 style="margin:5px 0 10px">${esc(x.name)}</h4>
        ${x.description ? `<p>${esc(x.description)}</p>` : ""}
        ${x.videoId ? `<div style="position:relative;padding-bottom:56.25%;height:0;overflow:hidden;border-radius:8px"><iframe src="https://www.youtube-nocookie.com/embed/${encodeURIComponent(x.videoId)}" title="${esc(x.name)}" style="position:absolute;top:0;left:0;width:100%;height:100%;border:0" loading="lazy" referrerpolicy="strict-origin-when-cross-origin" allowfullscreen></iframe></div>` : '<p class="metric-note">No valid YouTube video is linked yet.</p>'}
      </article>`).join("")}</div>` : '<div class="empty">Exercise videos will be added soon. Stay tuned!</div>'}
    </section>`,
  );
}

async function records() {
  let [a, b] = await Promise.all([api("/medicalrecords"), api("/labresults")]);
  shell(
    "Medical Records",
    "Medical history and lab results.",
    `<div class="page-title"><h2>Records and Labs</h2><div><button class="btn light" onclick="modal('record')">＋ Medical Record</button> <button class="btn" onclick="modal('lab')">＋ Lab Result</button></div></div><section class="panel"><div class="panel-head"><h3>Medical Conditions</h3></div>${a.map((x) => `<div class="record"><div><div class="record-main">${esc(x.condition)}</div><div class="record-sub">${esc(x.description || x.notes || "")}</div></div><span class="record-value">${date(x.diagnosisDate)}</span></div>`).join("") || '<div class="empty">No medical records.</div>'}</section><section class="panel"><div class="panel-head"><h3>Lab Results</h3></div>${b.length ? `<div class="table-wrap"><table><thead><tr><th>Test</th><th>Result</th><th>Unit</th><th>Date</th><th>Notes</th></tr></thead><tbody>${b.map((x) => `<tr><td>${esc(x.testName)}</td><td>${esc(x.result)}</td><td>${esc(x.unit || "—")}</td><td>${date(x.testDate)}</td><td>${esc(x.notes || "—")}</td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">No lab results.</div>'}</section>`,
  );
}

function whatsAppUrl(phone) {
  if (!phone || phone === "PENDING") return null;
  const digits = String(phone).replace(/\D/g, "");
  return /^[1-9]\d{6,14}$/.test(digits) ? `https://wa.me/${digits}` : null;
}

async function doctors() {
  let [d, a] = await Promise.all([api("/doctors"), api("/appointments")]);
  shell(
    "Doctors & Appointments",
    "Learn about specialties and book a time.",
    `<div class="page-title"><h2>Doctors</h2></div><div class="cards-grid">${d.map((x) => {
      let waUrl = whatsAppUrl(x.whatsAppPhone);

      return `<article class="doctor-card"><div class="doctor-head"><div class="doctor-avatar">⚕</div><div><h3>${esc(x.name)}</h3><p>${esc(x.specialization)}</p></div></div><p>${esc(x.description || "Health consultations and follow-up")}</p><p>☎ ${esc(x.phone || "Contact clinic")}</p><button class="btn" onclick="book(${x.id},'${esc(x.name)}')">Book Appointment</button>
      ${waUrl ? `<a class="btn light" target="_blank" rel="noopener noreferrer" href="${waUrl}">WhatsApp</a>` : `<button class="btn light" disabled title="A verified international WhatsApp number has not been provided">WhatsApp unavailable</button>`}</article>`
    }).join("")}</div><section class="panel" style="margin-top:16px"><div class="panel-head"><h3>My Appointments</h3></div>${a.map((x) => `<div class="record"><div><div class="record-main">${esc(x.doctor?.name)}</div><div class="record-sub">${esc(x.doctor?.specialization)} · ${esc(x.status)}</div></div><span class="record-value">${date(x.appointmentDate)}</span><span class="record-time">${time(x.appointmentDate)}</span></div>`).join("") || '<div class="empty">No appointments booked.</div>'}</section>`,
  );
}

async function loadAppointmentSlots(doctorId) {
  const dateInput = document.querySelector("#appointmentDate");
  const slotSelect = document.querySelector("#appointmentSlot");
  if (!dateInput || !slotSelect || !dateInput.value) return;
  slotSelect.innerHTML = '<option value="">Loading available times…</option>';
  try {
    const slots = await api(`/appointments/availability?doctorId=${doctorId}&date=${dateInput.value}`);
    slotSelect.innerHTML = slots.length
      ? '<option value="">Select an available time</option>' + slots.map(x => `<option value="${esc(x.appointmentDate)}">${esc(time(x.appointmentDate))}</option>`).join("")
      : '<option value="">No times available for this date</option>';
  } catch (error) {
    slotSelect.innerHTML = '<option value="">Could not load availability</option>';
    document.querySelector("#bookingError").textContent = error.message;
  }
}

function book(id, name) {
  document.body.insertAdjacentHTML(
    "beforeend",
    `<div class="modal-bg" onclick="if(event.target===this)this.remove()"><form class="modal" id="booking"><h3>Request an appointment with ${esc(name)}</h3><p>Default schedule: 9:00 AM–5:00 PM Cairo time, in 30-minute slots. A request is not a doctor confirmation.</p><div class="field"><label for="appointmentDate">Date</label><input id="appointmentDate" type="date" min="${today()}" value="${today()}" required></div><div class="field"><label for="appointmentSlot">Available time</label><select id="appointmentSlot" required><option value="">Choose a date first</option></select></div><div class="field"><label for="appointmentNotes">Notes for doctor (optional)</label><textarea id="appointmentNotes" name="notes"></textarea></div><div id="bookingError"></div><button class="btn">Request Booking</button> <button type="button" class="btn ghost" onclick="document.querySelector('.modal-bg').remove()">Cancel</button></form></div>`,
  );
  document.querySelector("#appointmentDate").onchange = () => loadAppointmentSlots(id);
  loadAppointmentSlots(id);
  document.querySelector("#booking").onsubmit = async (e) => {
    e.preventDefault();
    const form = new FormData(e.target);
    try {
      await api("/appointments", {
        method: "POST",
        body: JSON.stringify({
          doctorId: id,
          appointmentDate: document.querySelector("#appointmentSlot").value,
          notes: form.get("notes"),
        }),
      });
      document.querySelector(".modal-bg").remove();
      doctors();
    } catch (error) {
      document.querySelector("#bookingError").innerHTML = `<div class="notice">${esc(error.message)}</div>`;
      loadAppointmentSlots(id);
    }
  };
}

let timer;
async function pharmacies() {
  let q = document.querySelector("#pharmacySearch")?.value || "",
    r = await api("/pharmacies?search=" + encodeURIComponent(q));
  shell(
    "Pharmacies",
    "Search by name or address.",
    `<div class="page-title"><h2>Pharmacies</h2><input id="pharmacySearch" class="search" style="width:min(300px,60%)" placeholder="Search pharmacy name or address" value="${esc(q)}" oninput="clearTimeout(timer);timer=setTimeout(pharmacies,300)"></div><div class="cards-grid">${r.map((x) => {
      let waUrl = whatsAppUrl(x.whatsAppPhone);

      return `<article class="doctor-card"><div class="doctor-head"><div class="doctor-avatar">✚</div><div><h3>${esc(x.name)}</h3><p>Pharmacy</p></div></div><p>⌖ ${esc(x.address)}</p><p>☎ ${esc(x.phone || "Not available")}</p><p>◷ ${esc(x.openingHours || "Call for hours")}</p><div style="display:flex;gap:8px;margin-top:12px">${x.phone ? `<a class="btn" href="tel:${esc(x.phone)}">Call</a>` : ""} ${waUrl ? `<a class="btn light" target="_blank" rel="noopener noreferrer" href="${waUrl}">WhatsApp</a>` : `<button class="btn light" disabled title="A verified international WhatsApp number has not been provided">WhatsApp unavailable</button>`}</div></article>`
    }).join("") || '<div class="empty">No results found.</div>'}</div>`,
  );
}

async function meals() {
  let r = [];
  try { r = await api("/meals"); } catch { }
  shell("Healthy Meals", "Meal information for a balanced diet.",
    `<div class="page-title"><h2>Meal List</h2></div>
    <section class="panel">
      <div class="notice" style="margin-bottom:12px">
        <b>Note:</b> Calorie values shown are estimates unless stated otherwise. This information is for general reference only and does not constitute medical or dietary advice.
      </div>
      ${r && r.length ? `<div class="cards-grid">${r.map(x => `
        <article class="doctor-card">
          <div class="doctor-head">
            <div class="doctor-avatar">🍽</div>
            <div><h3>${esc(x.name)}</h3><p>${esc(x.description || '')}</p></div>
          </div>
          ${x.calories ? `<p>Calories: <b>${x.calories} kcal</b>${x.isEstimate ? ' <em>(estimate)</em>' : ''}</p>` : '<p style="color:var(--muted)">Calorie info not available</p>'}
        </article>`).join('')}</div>` : '<div class="empty">No meal data available yet. Meals will be added soon.</div>'}
    </section>`);
}

async function exercises() { return fitness(); }

async function del(url) {
  if (confirm("Delete this record?")) {
    await api(url, { method: "DELETE" });
    view();
  }
}

async function view() {
  page = location.hash.slice(1).split("?")[0] || "dashboard";
  if (page === "logout") {
    signOut();
    return;
  }
  if (["login", "register"].includes(page)) {
    auth(page === "register");
    return;
  }
  if (!localStorage.token) {
    auth();
    return;
  }
  try {
    await (
      {
        dashboard,
        health,
        medications: meds,
        bmi,
        fitness,
        records,
        doctors,
        pharmacies,
        meals,
        exercises: fitness
      }[page] || dashboard
    )();
  } catch (e) {
    app.innerHTML = `<main class="main"><div class="notice">${esc(e.message)}</div><button class="btn" onclick="view()">Retry</button></main>`;
  }
}
addEventListener("hashchange", view);
view();

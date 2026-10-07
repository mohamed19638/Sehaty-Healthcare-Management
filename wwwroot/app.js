const app = document.querySelector("#app");
let page = location.hash.slice(1) || "dashboard";
document.addEventListener("input", (e) => {
  if (["protein", "carbohydrates", "fat"].includes(e.target.name)) {
    const form = document.querySelector("#entry");
    if (form) {
      const value = (n) => Number(form.elements[n].value) || 0;
      form.elements.calories.value = Math.round(
        value("protein") * 4 + value("carbohydrates") * 4 + value("fat") * 9,
      );
    }
  }
});
new MutationObserver(() => {
  const calories = document.querySelector('#entry [name="calories"]');
  if (calories) calories.readOnly = true;
}).observe(document.body, { childList: true, subtree: true });
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
const date = (d) =>
    new Date(d).toLocaleDateString("ar-EG", {
      day: "numeric",
      month: "short",
      year: "numeric",
    }),
  time = (d) =>
    new Date(d).toLocaleTimeString("ar-EG", {
      hour: "2-digit",
      minute: "2-digit",
    }),
  today = () => new Date().toISOString().slice(0, 10),
  go = (p) => (location.hash = "#" + p);
async function api(path, opt = {}) {
  let r = await fetch("/api" + path, {
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
    throw Error("سجّل الدخول للمتابعة");
  }
  if (r.status === 204) return null;
  let j = await r.json().catch(() => null);
  if (!r.ok) throw Error(j?.message || "تعذر تنفيذ الطلب");
  return j;
}
function auth(reg = false) {
  app.innerHTML = `<div class="auth-screen"><section class="auth-art"><div class="logo">Sehaty<b>+</b></div><div><h1>صحتك،<br>في مكان واحد.</h1><p>تابع قياساتك، نظّم أدويتك، واحجز موعدك مع الطبيب. خطوات بسيطة لصحة أوضح.</p></div><small>Sehaty+ · رفيقك الصحي اليومي</small></section><section class="auth-card"><div class="logo" style="color:#123f3e">Sehaty<b>+</b></div><h2>${reg ? "أنشئ حسابك" : "أهلًا بعودتك 👋"}</h2><p>تابع صحتك وبياناتك اليومية</p><form id="authForm">${reg ? '<div class="field"><label>الاسم الكامل</label><input name="fullName" required></div>' : ""}<div class="field"><label>البريد الإلكتروني</label><input name="email" type="email" required></div><div class="field"><label>كلمة المرور</label><input name="password" type="password" required minlength="8"></div>${reg ? '<div class="field"><label>تاريخ الميلاد</label><input name="dateOfBirth" type="date" required></div><div class="field"><label>النوع</label><select name="gender"><option value="">اختر</option><option>أنثى</option><option>ذكر</option><option>أفضل عدم التحديد</option></select></div>' : ""}<div id="err"></div><button class="btn block">${reg ? "إنشاء حساب" : "تسجيل الدخول"}</button></form><div class="auth-foot">${reg ? "لديك حساب؟" : "ليس لديك حساب؟"} <span class="link" onclick="go('${reg ? "login" : "register"}')">${reg ? "سجّل الدخول" : "أنشئ حسابًا"}</span></div>${!reg ? '<div class="auth-foot">حساب التجربة: demo@sehaty.com · Demo123!</div>' : ""}</section></div>`;
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
  ["dashboard", "⌂", "الرئيسية"],
  ["health", "♡", "قياساتي الصحية"],
  ["nutrition", "◉", "التغذية والسعرات"],
  ["fitness", "↗", "النشاط الرياضي"],
  ["medications", "✚", "الأدوية"],
  ["records", "▤", "ملفي الطبي"],
  ["doctors", "⚕", "الأطباء والمواعيد"],
  ["pharmacies", "⌖", "الصيدليات"],
  ["chat", "☏", "محادثة الطبيب"],
];
function signOut() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  page = "login";
  location.hash = "#login";
  auth(false);
}
function shell(title, subtitle, html) {
  app.innerHTML = `<aside class="sidebar"><div class="logo">Sehaty<b>+</b></div><div class="nav-label">القائمة الرئيسية</div>${nav.map(([id, i, n]) => `<a class="nav-item ${page === id ? "active" : ""}" href="#${id}"><i>${i}</i>${n}</a>`).join("")}<div class="sidebar-bottom">بياناتك الصحية خاصة بك.<br>تابعها باستمرار.</div><a class="nav-item" href="#logout"><i>↪</i>تسجيل الخروج</a></aside><main class="main"><header class="topbar"><div class="welcome"><h1>${title}</h1><p>${subtitle || ""}</p></div><div class="top-actions"><div class="profile"><div class="avatar">${esc((me().fullName || "م")[0])}</div><div><b>${esc(me().fullName || "مستخدم")}</b><small>حساب شخصي</small></div></div><button class="signout-btn" onclick="signOut()" title="تسجيل الخروج" aria-label="تسجيل الخروج">↪ خروج</button></div></header>${html}</main>`;
}
function metric(n, v, u, i, note = "آخر قراءة مسجلة") {
  return `<article class="metric"><div class="metric-head"><span>${n}</span><span class="metric-icon">${i}</span></div><div class="metric-number">${v ?? "—"} <em>${u || ""}</em></div><div class="metric-note">${note}</div></article>`;
}
function chart(rows) {
  let a = [...rows].reverse().slice(-8);
  if (a.length < 2)
    return '<div class="empty">أضف قياسين أو أكثر لعرض تغيّر المؤشرات عبر الوقت.</div>';
  let w = 650,
    h = 180,
    p = 24,
    series = [
      ["النبض", (x) => Number(x.heartRate), "#16877f"],
      ["السكر", (x) => Number(x.bloodSugar), "#ef8c75"],
      ["الوزن", (x) => Number(x.weight), "#e8b65e"],
      [
        "الضغط الانقباضي",
        (x) => Number(String(x.bloodPressure || "").split("/")[0]),
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
  return `<svg viewBox="0 0 ${w} ${h}" preserveAspectRatio="none">${[0.2, 0.5, 0.8].map((y) => `<line class="chart-grid" x1="${p}" x2="${w - p}" y1="${h * y}" y2="${h * y}"/>`).join("")}${lines}</svg><div class="chart-legend"><span>النبض</span><span>السكر</span><span>الوزن (كجم)</span><span>الضغط الانقباضي</span></div>`;
}
function mlist(rows) {
  return rows.length
    ? `<div>${rows.map((x) => `<div class="record"><div><div class="record-main">${x.bloodPressure ? "ضغط " + esc(x.bloodPressure) : ""}${x.heartRate ? " · نبض " + x.heartRate : ""}${x.bloodSugar ? " · سكر " + x.bloodSugar : ""}${x.weight ? " · وزن " + x.weight + " كجم" : ""}</div><div class="record-sub">قراءة مسجلة</div></div><span class="record-value">${date(x.date)}</span><span class="record-time">${time(x.date)}</span></div>`).join("")}</div>`
    : '<div class="empty">لا توجد قياسات حتى الآن.</div>';
}
async function dashboard() {
  let d = await api("/dashboard"),
    m = d.measurements[0] || {},
    nt = d.nutrition.filter(
      (x) => new Date(x.date).toDateString() === new Date().toDateString(),
    ),
    cal = nt.reduce((s, x) => s + x.calories, 0),
    next = d.appointments[0];
  shell(
    "صباح الخير، " + esc(me().fullName || "") + " 👋",
    "صحتك، في مكان واحد.",
    `<div class="metrics">${metric("معدل نبضات القلب", m.heartRate, "نبضة/دقيقة", "♡", m.date ? date(m.date) + " · " + time(m.date) : "لا توجد قراءة")}${metric("ضغط الدم", m.bloodPressure, "مم زئبق", "⌁", m.date ? date(m.date) + " · " + time(m.date) : "لا توجد قراءة")}${metric("سكر الدم", m.bloodSugar, "mg/dL", "◉", m.date ? date(m.date) + " · " + time(m.date) : "لا توجد قراءة")}${metric("الوزن", m.weight, "كجم", "↕", m.date ? date(m.date) + " · " + time(m.date) : "لا توجد قراءة")}</div><div class="dash-grid"><div><section class="panel"><div class="panel-head"><h3>تغيّر القياسات</h3><span>آخر 8 قراءات · مرّر على النقطة للوقت</span></div><div class="chart-wrap">${chart(d.measurements)}</div></section><section class="panel"><div class="panel-head"><h3>سجل القياسات الأخير</h3><a class="link" href="#health">عرض الكل</a></div>${mlist(d.measurements.slice(0, 5))}</section></div><div><section class="panel"><div class="panel-head"><h3>سعرات اليوم</h3><span>مجموع الوجبات المسجلة</span></div><div class="calorie-ring" style="--progress:${Math.min(100, (cal / 2000) * 100)}%"><div class="calorie-center"><b>${cal}</b><small>من 2000 سعرة</small></div></div><p class="metric-note" style="text-align:center">${nt.reduce((s, x) => s + x.waterIntake, 0)} لتر ماء · ${nt.reduce((s, x) => s + x.protein, 0)} جم بروتين</p><button class="btn light" style="width:100%" onclick="modal('nutrition')">＋ سجّل وجبة</button></section><section class="panel"><div class="panel-head"><h3>الموعد القادم</h3><a class="link" href="#doctors">احجز</a></div>${next ? `<div class="record"><div><div class="record-main">${esc(next.doctor?.name)}</div><div class="record-sub">${esc(next.doctor?.specialization)}</div></div><span class="record-value">${date(next.appointmentDate)}</span><span class="record-time">${time(next.appointmentDate)}</span></div>` : '<div class="empty">لا توجد مواعيد قادمة.</div>'}</section><section class="panel"><div class="panel-head"><h3>إجراءات سريعة</h3></div><div class="quick-grid"><button class="quick" onclick="modal('measurement')"><b>＋</b>قياس جديد</button><button class="quick" onclick="go('medications')"><b>＋</b>إضافة دواء</button><button class="quick" onclick="go('doctors')"><b>＋</b>حجز طبيب</button><button class="quick" onclick="modal('nutrition')"><b>＋</b>تسجيل وجبة</button></div></section></div></div>`,
  );
}
const forms = {
  measurement: [
    "إضافة قياس صحي",
    "/healthmeasurements",
    [
      ["weight", "الوزن (كجم)", "number"],
      ["bloodPressure", "ضغط الدم مثال 120/80", "text"],
      ["bloodSugar", "سكر الدم mg/dL", "number"],
      ["heartRate", "النبض في الدقيقة", "number"],
      ["date", "تاريخ ووقت القياس", "datetime-local"],
    ],
  ],
  medication: [
    "إضافة دواء",
    "/medications",
    [
      ["name", "اسم الدواء", "text"],
      ["dosage", "الجرعة", "text"],
      ["instructions", "التعليمات", "text"],
      ["startDate", "تاريخ البدء", "date"],
      ["endDate", "تاريخ الانتهاء", "date"],
    ],
  ],
  nutrition: [
    "تسجيل وجبة",
    "/nutrition",
    [
      ["calories", "السعرات kcal", "number"],
      ["protein", "البروتين جم", "number"],
      ["carbohydrates", "الكربوهيدرات جم", "number"],
      ["fat", "الدهون جم", "number"],
      ["waterIntake", "الماء لتر", "number"],
      ["date", "التاريخ والوقت", "datetime-local"],
    ],
  ],
  fitness: [
    "تسجيل نشاط رياضي",
    "/fitness",
    [
      ["activityName", "نوع النشاط", "text"],
      ["durationMinutes", "المدة بالدقائق", "number"],
      ["caloriesBurned", "السعرات المحروقة", "number"],
      ["date", "التاريخ والوقت", "datetime-local"],
      ["notes", "ملاحظات", "text"],
    ],
  ],
  record: [
    "إضافة سجل طبي",
    "/medicalrecords",
    [
      ["condition", "الحالة الطبية", "text"],
      ["description", "الوصف", "text"],
      ["diagnosisDate", "تاريخ التشخيص", "date"],
      ["notes", "ملاحظات", "text"],
    ],
  ],
  lab: [
    "إضافة نتيجة تحليل",
    "/labresults",
    [
      ["testName", "اسم التحليل", "text"],
      ["result", "النتيجة", "text"],
      ["unit", "الوحدة", "text"],
      ["testDate", "تاريخ التحليل", "date"],
      ["notes", "ملاحظات", "text"],
    ],
  ],
};
function modal(k) {
  let [title, url, fields] = forms[k];
  document.querySelector(".modal-bg")?.remove();
  document.body.insertAdjacentHTML(
    "beforeend",
    `<div class="modal-bg" onclick="if(event.target===this)this.remove()"><form class="modal" id="entry"><h3>${title}</h3><div class="form-grid">${fields.map(([n, label, type]) => `<div class="field ${["notes", "instructions"].includes(n) ? "wide" : ""}"><label>${label}</label><input name="${n}" type="${type}" ${type === "datetime-local" ? `value="${new Date().toISOString().slice(0, 16)}"` : ["date", "diagnosisDate", "testDate", "startDate"].includes(n) ? `value="${today()}"` : ""} ${["name", "activityName", "condition", "testName", "result"].includes(n) ? "required" : ""} ${type === "number" ? 'min="0" step="0.1"' : ""}></div>`).join("")}</div><button class="btn">حفظ</button> <button type="button" class="btn ghost" onclick="document.querySelector('.modal-bg').remove()">إلغاء</button></form></div>`,
  );
  document.querySelector("#entry").onsubmit = async (e) => {
    e.preventDefault();
    let o = Object.fromEntries(new FormData(e.target));
    fields.forEach(([n, , t]) => {
      if (t === "number") o[n] = o[n] ? Number(o[n]) : null;
      if (t === "datetime-local") o[n] = new Date(o[n]).toISOString();
    });
    try {
      await api(url, { method: "POST", body: JSON.stringify(o) });
      document.querySelector(".modal-bg").remove();
      view();
    } catch (x) {
      alert(x.message);
    }
  };
}
async function health() {
  let r = await api("/healthmeasurements");
  shell(
    "قياساتي الصحية",
    "سجّل القياس وقت أخذه لمقارنة القراءات بدقة.",
    `<div class="page-title"><div><h2>المؤشرات عبر الزمن</h2><p>يظهر وقت كل قياس بجانب تاريخه.</p></div><button class="btn" onclick="modal('measurement')">＋ إضافة قياس</button></div><section class="panel"><div class="panel-head"><h3>رسم القراءات</h3><span>آخر 8 سجلات</span></div><div class="chart-wrap">${chart(r)}</div></section><section class="panel"><div class="panel-head"><h3>سجل القياسات · الأحدث أولًا</h3><span>${r.length} قراءة</span></div>${mlist(r)}</section>`,
  );
}
async function meds() {
  let r = await api("/medications");
  shell(
    "الأدوية",
    "تابع الجرعات وتواريخ البدء.",
    `<div class="page-title"><h2>قائمة الأدوية</h2><button class="btn" onclick="modal('medication')">＋ إضافة دواء</button></div><section class="panel">${r.length ? `<div class="table-wrap"><table><thead><tr><th>اسم الدواء</th><th>الجرعة</th><th>التعليمات</th><th>تاريخ البدء</th><th></th></tr></thead><tbody>${r.map((x) => `<tr><td><b>${esc(x.name)}</b></td><td>${esc(x.dosage || "—")}</td><td>${esc(x.instructions || "—")}</td><td>${date(x.startDate)}</td><td><button class="btn danger" onclick="del('/medications/${x.id}')">حذف</button></td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">لم تضف أدوية بعد.</div>'}</section>`,
  );
}
async function nutrition() {
  let r = await api("/nutrition"),
    t = r.filter(
      (x) => new Date(x.date).toDateString() === new Date().toDateString(),
    ),
    sum = (k) => t.reduce((s, x) => s + (Number(x[k]) || 0), 0),
    c = sum("calories");
  shell(
    "التغذية والسعرات",
    "السعرات والمغذيات محسوبة من الوجبات التي تسجلها.",
    `<div class="page-title"><div><h2>ملخص اليوم · ${date(new Date())}</h2><p>الإجمالي هو مجموع مدخلات اليوم.</p></div><button class="btn" onclick="modal('nutrition')">＋ تسجيل وجبة</button></div><div class="dash-grid"><section class="panel"><div class="calorie-layout"><div class="calorie-ring" style="--progress:${Math.min(100, (c / 2000) * 100)}%"><div class="calorie-center"><b>${c}</b><small>من 2000 kcal</small></div></div><div>${[
      ["بروتين", "protein", 100],
      ["كربوهيدرات", "carbohydrates", 250],
      ["دهون", "fat", 70],
      ["ماء", "waterIntake", 2.5],
    ]
      .map(
        ([n, k, max]) =>
          `<div class="macro"><div class="macro-label"><span>${n}</span><b>${sum(k)} ${k === "waterIntake" ? "لتر" : "جم"}</b></div><div class="bar"><i style="width:${Math.min(100, (sum(k) / max) * 100)}%"></i></div></div>`,
      )
      .join(
        "",
      )}</div></div></section><section class="panel"><h3>حساب السعرات</h3><p style="font-size:12px;color:#718381;line-height:2">أدخل سعرات كل وجبة من ملصق المنتج أو تقديرك. الإجمالي يجمع مدخلات اليوم. لن يخمّن التطبيق مكونات الوجبة.</p></section></div><section class="panel"><div class="panel-head"><h3>سجل الطعام</h3></div>${r.length ? `<div class="table-wrap"><table><thead><tr><th>التاريخ والوقت</th><th>السعرات</th><th>بروتين</th><th>كربوهيدرات</th><th>دهون</th><th>ماء</th></tr></thead><tbody>${r.map((x) => `<tr><td>${date(x.date)} · ${time(x.date)}</td><td><b>${x.calories} kcal</b></td><td>${x.protein} جم</td><td>${x.carbohydrates} جم</td><td>${x.fat} جم</td><td>${x.waterIntake} لتر</td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">لا توجد وجبات. سجّل أول وجبة لبدء الحساب.</div>'}</section>`,
  );
}
async function fitness() {
  let r = await api("/fitness");
  shell(
    "النشاط الرياضي",
    "سجّل التمارين ووقتها والسعرات المحروقة.",
    `<div class="page-title"><h2>ملخص النشاط</h2><button class="btn" onclick="modal('fitness')">＋ تسجيل نشاط</button></div><div class="metrics">${metric("عدد الأنشطة", r.length, "نشاط", "↗", "كل السجلات")}${metric(
      "وقت التمرين",
      r.reduce((s, x) => s + x.durationMinutes, 0),
      "دقيقة",
      "◷",
      "إجمالي المدة",
    )}${metric(
      "سعرات محروقة",
      r.reduce((s, x) => s + x.caloriesBurned, 0),
      "kcal",
      "⌁",
      "حسب المدخلات",
    )}${metric("آخر نشاط", r[0]?.activityName || "—", "", "🏃", r[0] ? date(r[0].date) + " · " + time(r[0].date) : "لا يوجد")}</div><section class="panel"><div class="panel-head"><h3>الأنشطة المسجلة</h3></div>${r.length ? `<div class="table-wrap"><table><thead><tr><th>النشاط</th><th>التوقيت</th><th>المدة</th><th>سعرات محروقة</th><th>ملاحظات</th></tr></thead><tbody>${r.map((x) => `<tr><td>${esc(x.activityName)}</td><td>${date(x.date)} · ${time(x.date)}</td><td>${x.durationMinutes} دقيقة</td><td>${x.caloriesBurned} kcal</td><td>${esc(x.notes || "—")}</td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">لم تسجل تمرينًا بعد.</div>'}</section>`,
  );
}
async function records() {
  let [a, b] = await Promise.all([api("/medicalrecords"), api("/labresults")]);
  shell(
    "ملفي الطبي",
    "تاريخ الحالات ونتائج التحاليل.",
    `<div class="page-title"><h2>السجلات والتحاليل</h2><div><button class="btn light" onclick="modal('record')">＋ سجل طبي</button> <button class="btn" onclick="modal('lab')">＋ نتيجة تحليل</button></div></div><section class="panel"><div class="panel-head"><h3>الحالات الطبية</h3></div>${a.map((x) => `<div class="record"><div><div class="record-main">${esc(x.condition)}</div><div class="record-sub">${esc(x.description || x.notes || "")}</div></div><span class="record-value">${date(x.diagnosisDate)}</span></div>`).join("") || '<div class="empty">لا توجد سجلات طبية.</div>'}</section><section class="panel"><div class="panel-head"><h3>نتائج التحاليل</h3></div>${b.length ? `<div class="table-wrap"><table><thead><tr><th>التحليل</th><th>النتيجة</th><th>الوحدة</th><th>التاريخ</th><th>ملاحظات</th></tr></thead><tbody>${b.map((x) => `<tr><td>${esc(x.testName)}</td><td>${esc(x.result)}</td><td>${esc(x.unit || "—")}</td><td>${date(x.testDate)}</td><td>${esc(x.notes || "—")}</td></tr>`).join("")}</tbody></table></div>` : '<div class="empty">لا توجد نتائج تحاليل.</div>'}</section>`,
  );
}
async function doctors() {
  let [d, a] = await Promise.all([api("/doctors"), api("/appointments")]);
  shell(
    "الأطباء والمواعيد",
    "تعرّف على التخصصات واختر توقيت الحجز.",
    `<div class="page-title"><h2>الأطباء</h2></div><div class="cards-grid">${d.map((x) => `<article class="doctor-card"><div class="doctor-head"><div class="doctor-avatar">⚕</div><div><h3>${esc(x.name)}</h3><p>${esc(x.specialization)}</p></div></div><p>${esc(x.description || "استشارات ومتابعة صحية")}</p><p>☎ ${esc(x.phone || "تواصل مع العيادة")}</p><button class="btn" onclick="book(${x.id},'${esc(x.name)}')">احجز موعدًا</button> ${x.phone ? `<a class="btn light" target="_blank" rel="noopener" href="https://wa.me/${x.phone.replace(/\D/g, "").replace(/^0/, "20")}">WhatsApp</a>` : ""}</article>`).join("")}</div><section class="panel" style="margin-top:16px"><div class="panel-head"><h3>مواعيدي القادمة</h3></div>${a.map((x) => `<div class="record"><div><div class="record-main">${esc(x.doctor?.name)}</div><div class="record-sub">${esc(x.doctor?.specialization)} · ${esc(x.status)}</div></div><span class="record-value">${date(x.appointmentDate)}</span><span class="record-time">${time(x.appointmentDate)}</span></div>`).join("") || '<div class="empty">لا توجد مواعيد محجوزة.</div>'}</section>`,
  );
}
function book(id, name) {
  document.body.insertAdjacentHTML(
    "beforeend",
    `<div class="modal-bg" onclick="if(event.target===this)this.remove()"><form class="modal" id="booking"><h3>حجز موعد مع ${esc(name)}</h3><div class="field"><label>تاريخ ووقت الموعد</label><input name="date" type="datetime-local" required min="${new Date(Date.now() + 3600000).toISOString().slice(0, 16)}"></div><div class="field"><label>ملاحظات للطبيب</label><textarea name="notes"></textarea></div><button class="btn">تأكيد الحجز</button></form></div>`,
  );
  document.querySelector("#booking").onsubmit = async (e) => {
    e.preventDefault();
    let f = Object.fromEntries(new FormData(e.target));
    await api("/appointments", {
      method: "POST",
      body: JSON.stringify({
        doctorId: id,
        appointmentDate: new Date(f.date).toISOString(),
        notes: f.notes,
      }),
    });
    doctors();
  };
}
let timer;
async function pharmacies() {
  let q = document.querySelector("#pharmacySearch")?.value || "",
    r = await api("/pharmacies?search=" + encodeURIComponent(q));
  shell(
    "الصيدليات",
    "ابحث بالاسم أو العنوان واتصل بالصيدلية.",
    `<div class="page-title"><h2>الصيدليات</h2><input id="pharmacySearch" class="search" style="width:min(300px,60%)" placeholder="اسم أو عنوان الصيدلية" value="${esc(q)}" oninput="clearTimeout(timer);timer=setTimeout(pharmacies,300)"></div><div class="cards-grid">${r.map((x) => `<article class="doctor-card"><div class="doctor-head"><div class="doctor-avatar">✚</div><div><h3>${esc(x.name)}</h3><p>صيدلية</p></div></div><p>⌖ ${esc(x.address)}</p><p>☎ ${esc(x.phone || "غير متوفر")}</p><p>◷ ${esc(x.openingHours || "اتصل للاستفسار")}</p>${x.phone ? `<a class="btn light" href="tel:${esc(x.phone)}">اتصل بالصيدلية</a>` : ""}</article>`).join("") || '<div class="empty">لا توجد نتائج.</div>'}</div>`,
  );
}
async function chat() {
  let d = await api("/doctors");
  if (!d.length) {
    shell(
      "محادثة الطبيب",
      "رسائل محفوظة في قاعدة البيانات.",
      '<div class="empty">لا يوجد أطباء.</div>',
    );
    return;
  }
  let id =
      Number(
        new URLSearchParams(location.hash.split("?")[1] || "").get("doctor"),
      ) || d[0].id,
    m = await api("/chat/" + id);
  shell(
    "محادثة الطبيب",
    "مراسلة تجريبية محفوظة وليست فورية.",
    `<section class="panel"><div class="field"><label>الطبيب</label><select onchange="location.hash='#chat?doctor='+this.value">${d.map((x) => `<option value="${x.id}" ${x.id === id ? "selected" : ""}>${esc(x.name)} · ${esc(x.specialization)}</option>`).join("")}</select></div><div class="chat-box">${m.map((x) => `<div class="bubble ${x.isFromDoctor ? "" : "mine"}">${esc(x.message)}<small>${date(x.sentAt)} · ${time(x.sentAt)}</small></div>`).join("") || '<div class="empty">أرسل رسالتك للطبيب.</div>'}</div><form id="chatForm" style="display:flex;gap:8px;margin-top:12px"><input class="search" name="message" required placeholder="اكتب رسالتك"><button class="btn">إرسال</button></form></section>`,
  );
  document.querySelector("#chatForm").onsubmit = async (e) => {
    e.preventDefault();
    let message = new FormData(e.target).get("message");
    await api("/chat", {
      method: "POST",
      body: JSON.stringify({ doctorId: id, message }),
    });
    chat();
  };
}
async function del(url) {
  if (confirm("هل تريد حذف السجل؟")) {
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
        nutrition,
        fitness,
        records,
        doctors,
        pharmacies,
        chat,
      }[page] || dashboard
    )();
  } catch (e) {
    app.innerHTML = `<main class="main"><div class="notice">${esc(e.message)}</div><button class="btn" onclick="view()">إعادة المحاولة</button></main>`;
  }
}
addEventListener("hashchange", view);
view();


# Sehaty+

تطبيق تخرج بسيط لمتابعة البيانات الصحية الشخصية. الواجهة عربية ومتجاوبة، ويخدمها ASP.NET Core Web API مباشرة من نفس المشروع.

## التقنيات

- ASP.NET Core Web API وC# على .NET 8
- Entity Framework Core وSQL Server
- JWT لتسجيل الدخول وحماية بيانات المستخدم
- HTML وCSS وJavaScript عادي داخل `wwwroot`

## التشغيل

### تشغيل المشروع كاملًا باستخدام Docker (الأسهل)

المتطلبات: [Docker Desktop](https://docs.docker.com/desktop/setup/install/windows-install/) مع Docker Compose مفعّلًا.

نزّل المشروع من زر **Code > Download ZIP** في صفحة GitHub وفك الضغط، أو استخدم Git:

```powershell
git clone https://github.com/mohamed19638/Sehaty-Healthcare-Management.git
cd Sehaty-Healthcare-Management
```

1. من داخل مجلد المشروع، انسخ ملف الإعدادات التجريبي إلى `.env`:

   ```powershell
   Copy-Item .env.example .env
   ```

2. شغّل التطبيق وقاعدة البيانات:

   ```powershell
   docker compose up --build
   ```

3. افتح [http://localhost:8080](http://localhost:8080). قاعدة البيانات تُجهّز تلقائيًا عند أول تشغيل.

لو أردت الاتصال بقاعدة البيانات من SSMS على جهازك، استخدم `localhost,14330`. التطبيق نفسه يتصل بها داخليًا عبر Docker على `sqlserver,1433`.

لإيقاف الحاويات اضغط `Ctrl+C` ثم شغّل `docker compose down`. البيانات تظل محفوظة في volume اسمه `sqlserver-data`. حذف البيانات نهائيًا يكون فقط عند تشغيل `docker compose down --volumes`.

القيم الموجودة في `.env.example` للتجربة المحلية فقط؛ لا تستخدمها على خادم عام. ملف `.env` وملفات `appsettings*.json` المحلية مستثناة من Git، لذلك لا ترفع كلمات مرورك أو مفاتيح JWT.

### تشغيل المشروع مباشرة على Windows

1. شغّل SQL Server واضبط متغيري البيئة `ConnectionStrings__DefaultConnection` و`Jwt__Key` في جلستك المحلية.
2. افتح Terminal داخل مجلد المشروع وشغّل:

   ```powershell
   dotnet restore
   dotnet run
   ```

3. افتح عنوان التشغيل الذي يظهر في Terminal. عادةً يكون `http://localhost:5295`، وواجهة Swagger على `/swagger`.

قاعدة البيانات الافتراضية `SehatyDb`. عند أول تشغيل، تطبّق EF Core ملفات Migrations وتضيف بيانات العرض. Swagger متاح على `/swagger`.

## حساب العرض

- البريد: `demo@sehaty.com`
- كلمة المرور: `Demo123!`

يُخزّن المشروع كلمة المرور كـ hash. حساب العرض والبيانات المضافة مخصّصان للتجربة التعليمية.

## المزايا والـ API

| المسار | الاستخدام |
| --- | --- |
| `POST /api/auth/register` و`POST /api/auth/login` | إنشاء حساب وتسجيل الدخول |
| `GET /api/auth/me` | بيانات الحساب الحالي |
| `GET /api/dashboard` | القياسات الحديثة والتغذية والمواعيد |
| `/api/healthmeasurements` | قراءة وإضافة وتعديل وحذف القياسات |
| `/api/medications` | قراءة وإضافة وحذف الأدوية |
| `/api/medicalrecords` و`/api/labresults` | السجل الطبي والتحاليل |
| `/api/nutrition` و`/api/fitness` | سجلات التغذية والتمارين |
| `/api/doctors` و`/api/appointments` | الأطباء وحجز المواعيد |
| `/api/pharmacies?search=` | الصيدليات والبحث بالاسم أو العنوان |
| `/api/chat/{doctorId}` و`POST /api/chat` | رسائل محفوظة في قاعدة البيانات |

كل مسارات بيانات المستخدم تتعرف على مالك البيانات من JWT. لا ترسل الواجهة `UserId` للتحكم في ملكية السجلات. سجلات القياس تحفظ التاريخ والوقت؛ وتعرض الواجهة القراءات على الرسم وفي سجل زمني. السعرات تحسب من المغذيات الكبرى عند إدخال الوجبة: البروتين × 4 + الكربوهيدرات × 4 + الدهون × 9، ويجمع ملخص اليوم الوجبات المسجلة في نفس اليوم.

## هيكل المشروع

- `Controllers/`: Controller منفصل لكل مجموعة API.
- `DTOs/`: نماذج استقبال طلبات التسجيل والقياس والتغذية وغيرها.
- `Models/`: كيانات قاعدة البيانات.
- `Data/AppDbContext.cs`: DbContext وعلاقات EF Core.
- `Migrations/`: إنشاء مخطط قاعدة البيانات وتحديثه.
- `wwwroot/`: صفحات الواجهة وسكربتاتها وتنسيقاتها.

الأطباء والصيدليات وبيانات الحساب التجريبي بيانات توضيحية. المحادثة لا تعمل لحظيًا ولا ترسل رسائل حقيقية، وزر WhatsApp يفتح رابط تواصل مبنيًا على رقم الطبيب المسجل. المشروع تعليمي وليس نظامًا طبيًا للاستخدام الإنتاجي.


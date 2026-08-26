# Halaqa Desktop

تطبيق **WPF بلغة C# على Windows** لمنصة **Quran Halaqa Live**. يستهلك العميل خدمة Laravel عبر HTTPS وREST، ولا يعرّف عقد API مستقلاً ولا يغير منصة التطبيق أو بنية طبقاته.

> لا يساوي وجود شاشة أو DTO أو واجهة أو mock اكتمال ميزة. الحالة الدقيقة المبنية على الكود والتسجيل والملاحة والعقد والاختبارات موجودة في [تقرير المراجعة التصحيحية](.agent/FRONTEND_IMPLEMENTATION_REVIEW.md).

## المتطلبات والتشغيل

يتطلب التشغيل Windows مع **.NET SDK 8.0** و**Visual Studio 2022 الإصدار 17.8 أو أحدث** عند استعمال Visual Studio. يستهدف المشروعان `Halaqa.Desktop` و`Halaqa.Desktop.Tests` الإطار `net8.0-windows` عمداً؛ لا تغيّر الهدف إلى .NET 6 لحل خطأ البيئة. يطلب [`global.json`](global.json) سلسلة SDK 8.0 ويتيح التقدم داخل سلسلة ميزات 8.0 فقط. تؤكد Microsoft أن استهداف `net8.0` يتطلب Visual Studio 17.8 أو أحدث، وأن تثبيت SDK يتضمن بيئة تشغيل سطح المكتب اللازمة لـ WPF.[1] [2]

### إصلاح خطأ Visual Studio: SDK لا يدعم .NET 8 وملف `Halaqa.Desktop.exe` مفقود

> إذا ظهرت `NETSDK... The current .NET SDK does not support targeting .NET 8.0` في مشروع التطبيق والاختبارات، فالبناء فشل قبل إنتاج الملف التنفيذي. إن غياب `bin\\Debug\\net8.0-windows\\Halaqa.Desktop.exe` **نتيجة** للفشل وليس ملفاً ينبغي إنشاؤه أو نسخه يدوياً.

| الخطوة | الإجراء المطلوب |
|---|---|
| 1 | من **Help > About Microsoft Visual Studio** تحقق من أن الإصدار هو Visual Studio 2022 **17.8+**. حدّثه عبر Visual Studio Installer إن كان أقدم. |
| 2 | في Visual Studio Installer اختر **Modify** وثبّت حمل العمل **.NET desktop development** وSDK .NET 8.0. لا يكتفي تثبيت Runtime وحده بالبناء والتصحيح. |
| 3 | أغلق Visual Studio تماماً وافتح PowerShell جديداً في جذر المستودع، ثم نفّذ `dotnet --list-sdks`. يجب أن يظهر SDK يبدأ بـ `8.0.`. |
| 4 | نفّذ `dotnet restore .\\Halaqa.sln` ثم `dotnet build .\\Halaqa.sln`. بعد نجاح البناء استخدم **Build > Rebuild Solution** في Visual Studio، ثم شغّل مشروع `Halaqa.Desktop` كـ Startup Project. |
| 5 | إذا استمر أثر بناء قديم بعد ظهور SDK 8.0، أغلق Visual Studio واحذف مجلدي `bin` و`obj` داخل `src\\Halaqa.Desktop` و`tests\\Halaqa.Desktop.Tests`، ثم نفّذ restore/build مرة أخرى. لا تحذف الملفات قبل تثبيت SDK الصحيح. |

يحدد `Bootstrapper` الإعداد من `appsettings.json`، ثم يحمّل متغيرات البيئة التي تبدأ بـ `HALAQA_`. يجب ضبط عنوان Laravel حقيقي قبل التشغيل؛ لا تستخدم عناوين الأمثلة الموجودة في OpenAPI كعناوين تشغيلية.

```json
// src/Halaqa.Desktop/appsettings.json
{
  "Api": {
    "BaseUrl": "https://your-domain.example/api/v1/"
  }
}
```

أو في PowerShell على Windows:

```powershell
$env:HALAQA_Api__BaseUrl = "https://your-domain.example/api/v1/"
dotnet run --project src/Halaqa.Desktop/Halaqa.Desktop.csproj
```

يتحقق العميل من أن العنوان URI مطلق عند إعداد `HttpClient`، ويضيف `BearerTokenHandler` للطلبات المحمية. لا تحفظ كلمات المرور؛ تحفظ جلسة الوصول عبر Windows DPAPI.

## المعمارية والتخزين المحلي

كل ميزة تتبع طبقات `Presentation` ثم `Domain` ثم `Data`. يحتوي `Config` على التركيب وعميل REST والجلسة المشفرة، بينما يحتوي `Shared` على الثيم والمكونات وأنواع النتيجة. راجع `.agent/ARCHITECTURE.md` و`.agent/references/canonical-tree.md` قبل تعديل أي ميزة.

يستخدم العميل ملف `Assets/Quran/QuranV3.sqlite`، وينسخه إلى `%LocalAppData%\Halaqa\Data\QuranV3.sqlite` ثم يفتحه للقراءة فقط. ويستخدم `halaqa-local.db` لـ `mistake_outbox` وبيانات المزامنة فقط. هذه أصول قراءة محلية؛ لا تمثل الحالة التعليمية الرسمية ولا تثبت وجود جلسة أو نقل فوري عامل.

## عقد Laravel المراجع

العقد الملزم موجود في المستودع الخاص [samalnashamy780-art/quran-halaqa](https://github.com/samalnashamy780-art/quran-halaqa) في `.agent/openapi.yaml`. يشير [مرجع العقد](.agent/CONTRACT_REFERENCE.md) إلى التزام Laravel المراجع `f84e9dde84cb6b4239ccd341adcaf8e0d84e7c4e` والإصدار `1.1.0`، ويبين كل استهلاك وفجوة معروفة. قبل تعديل أي DTO أو mapper أو repository أو استدعاء HTTP، راجع نسخة `openapi.yaml` الحالية في Laravel وتحقق من المسار و`operationId` والمخططات والصلاحيات والأخطاء.

## الحالة الفعلية للميزات

| الفئة | المتاح من رحلة التطبيق الحالية | القيود المهمة |
|---|---|---|
| المصادقة | الدخول، تسجيل الطالب/المعلم، نسيان كلمة المرور وإعادة الضبط، واستعادة الجلسة. | تغيير كلمة المرور مسجل وله View لكنه غير موصول من الغلاف. لا توجد نتائج API حية موثقة. |
| الملفات والوثائق | ملفات الطالب/المعلم التفصيلية ووثائق المعلم متاحة من لوحة الدور. | ما زال يلزم تحقق حي للصلاحيات و422 ورفع/حذف الوثيقة. صفحة الملف العام غير قابلة للوصول من لوحة التطبيق الحالية. |
| الحلقات والطلبات | إدارة الحلقات وطلبات الحلقة، ودليل المعلمين وطلبات الطالب، وصندوق `student-applications` العام للمعلم. | يعرض صندوق المعلم العام الملخصات العامة فقط قبل القبول. قائمة العضويات الحالية تستخدم مساراً غير معلن في عقد Laravel الحالي. |
| المصحف | قارئ مستقل قابل للوصول يعرض المصدر المحلي أولاً بخط QCF المناسب لكل صفحة، مع بديل صفحة HTTP. | البديل البعيد نص توافق لا رموز QCF، ولا يثبت القارئ حالة مصحف رسمية أو جلسة تفاعلية. |
| الجلسات الحية | توجد عقود ونماذج عرض وعميل REST محدود لإعداد realtime/تفويض القناة/حفظ الحالة الرسمية. | لا يوجد تنفيذ أو تسجيل DI لـ WebRTC أو WebSocket أو DataChannel أو تسجيل فيديو؛ صفحة الجلسة ليست طريقاً قابلاً للحل من الغلاف. |
| المتابعة والإشعارات والأخطاء والتقارير | رحلة الطالب لخطة المتابعة والحضور، وصندوق الإشعارات الرسمي للمستخدم الحالي، وoutbox محلي لإنشاء الأخطاء. | لا توجد واجهة للأخطاء، ولا تنفيذ للملاحظات أو التقييم أو التقارير أو التقدم، كما يلزم تحقق Laravel حي للمتابعة والإشعارات. |

## التحقق المطلوب

تشغّل التغييرات بوابة الجودة الموجودة في `.github/workflows/` في بيئة Windows للتحقق من restore وbuild والاختبارات وفحص المسافات وحدود الطبقات. نجاح هذه البوابة لا يغني عن اختبارات Laravel الحية: يجب توثيق حالات النجاح والفشل والصلاحيات و422 و404 و409 حسب المسار قبل رفع حالة أي ميزة.

## المراجع

[1]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/8.0/version-requirements "Microsoft: Version requirements for .NET 8 SDK"
[2]: https://learn.microsoft.com/en-us/dotnet/core/install/windows "Microsoft: Install .NET on Windows"

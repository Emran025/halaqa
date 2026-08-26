# Halaqa Desktop

تطبيق **WPF بلغة C# على Windows** لمنصة **Quran Halaqa Live**. يستهلك العميل خدمة Laravel عبر HTTPS وREST، ولا يعرّف عقد API مستقلاً ولا يغير منصة التطبيق أو بنية طبقاته.

> لا يساوي وجود شاشة أو DTO أو واجهة أو mock اكتمال ميزة. الحالة الدقيقة المبنية على الكود والتسجيل والملاحة والعقد والاختبارات موجودة في [تقرير المراجعة التصحيحية](.agent/FRONTEND_IMPLEMENTATION_REVIEW.md).

## المتطلبات والتشغيل

يتطلب التشغيل Windows مع **Visual Studio 2022 الإصدار 17.0 أو أحدث** وحمل العمل **.NET desktop development** و**.NET SDK 6.0.1xx**. يستهدف المشروعان `Halaqa.Desktop` و`Halaqa.Desktop.Tests` الإطار `net6.0-windows`، وتثبت ملفات المشروع لغة **C# 10.0**. هذا هو نطاق SDK الذي يدعمه Visual Studio/MSBuild 17.0؛ تؤكد Microsoft أن Visual Studio 2022 الإصدار 17.0 يدعم SDK .NET 6.0.100.[1]

يضبط [`global.json`](global.json) الإصدار الأساسي `6.0.100` مع `latestPatch` فقط، ولذلك يختار SDK مثبتاً من سلسلة `6.0.1xx` ولا ينتقل إلى سلاسل ميزات أحدث قد تتطلب Visual Studio أحدث. يجب استعمال **SDK** لا Runtime فقط، لأن SDK يتضمن أدوات البناء اللازمة لـ WPF.[2]

### إصلاح خطأ SDK 8.0 مع MSBuild 17.0

> كان ظهور رسالة تفيد بأن SDK `8.0.406` يحتاج MSBuild `17.8.3` أو أحدث سبباً في فشل الاستعادة قبل ترجمة المشروع، ومن ثم ظهور `Microsoft.NET.Sdk` غير موجود وغياب الملف التنفيذي. بعد هذا التغيير لم يعد المشروع يطلب SDK 8.0؛ لا تنشئ أو تنسخ `Halaqa.Desktop.exe` يدوياً لأن غيابه كان **نتيجة** لفشل البناء.

| الخطوة | الإجراء المطلوب على الجهاز |
|---|---|
| 1 | من **Help > About Microsoft Visual Studio** تحقق من Visual Studio 2022 **17.0** أو إصدار أحدث، ومن وجود MSBuild 17.0 أو أحدث. لا يلزم الترقية إلى 17.8 لاستهداف هذا المشروع. |
| 2 | افتح **Visual Studio Installer > Modify** وثبّت حمل العمل **.NET desktop development** ومكوّن **.NET 6.0 SDK (6.0.1xx)**. لا يكفي تثبيت .NET Runtime للبناء أو التصحيح. |
| 3 | أغلق Visual Studio تماماً وافتح PowerShell جديداً في جذر المستودع، ثم نفّذ `dotnet --list-sdks`. يجب أن يظهر SDK يبدأ بـ `6.0.1`. إذا لم يظهر، ثبّت SDK 6.0 من Visual Studio Installer ثم افتح PowerShell جديداً. |
| 4 | نفّذ `dotnet --info` للتأكد أن SDK الفعّال 6.0.1xx، ثم `dotnet restore .\\Halaqa.sln` و`dotnet build .\\Halaqa.sln`. بعد نجاح البناء استخدم **Build > Rebuild Solution** وشغّل مشروع `Halaqa.Desktop` بوصفه Startup Project. |
| 5 | إذا استمر أثر بناء سابق، أغلق Visual Studio واحذف مجلدي `bin` و`obj` داخل `src\\Halaqa.Desktop` و`tests\\Halaqa.Desktop.Tests`، ثم نفّذ الاستعادة والبناء مرة أخرى. |

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
| الملفات والوثائق | ملفات الطالب/المعلم التفصيلية ووثائق المعلم والملف العام المتاح من لوحة التطبيق. | ما زال يلزم تحقق حي للصلاحيات و422 ورفع/حذف الوثيقة. |
| الحلقات والطلبات | إدارة الحلقات وطلبات الحلقة، ودليل المعلمين وطلبات الطالب، وصندوق `student-applications` العام للمعلم. | يعرض صندوق المعلم العام الملخصات العامة فقط قبل القبول. قائمة العضويات الحالية تستخدم مساراً غير معلن في عقد Laravel الحالي. |
| المصحف | قارئ مستقل قابل للوصول يعرض المصدر المحلي أولاً بخط QCF المناسب لكل صفحة، مع بديل صفحة HTTP. | البديل البعيد نص توافق لا رموز QCF، ولا يثبت القارئ حالة مصحف رسمية أو جلسة تفاعلية. |
| الجلسات الحية | توجد عقود ونماذج عرض وعميل REST محدود لإعداد realtime/تفويض القناة/حفظ الحالة الرسمية. | لا يوجد تنفيذ أو تسجيل DI لـ WebRTC أو WebSocket أو DataChannel أو تسجيل فيديو؛ صفحة الجلسة ليست طريقاً قابلاً للحل من الغلاف. |
| المتابعة والإشعارات والأخطاء والتقارير | رحلة الطالب لخطة المتابعة والحضور، وصندوق الإشعارات الرسمي للمستخدم الحالي، وoutbox محلي لإنشاء الأخطاء. | لا توجد واجهة للأخطاء، ولا تنفيذ للملاحظات أو التقييم أو التقارير أو التقدم، كما يلزم تحقق Laravel حي للمتابعة والإشعارات. |

## التحقق المطلوب

تشغّل التغييرات بوابة الجودة الموجودة في `.github/workflows/` في بيئة Windows للتحقق من restore وbuild والاختبارات وفحص المسافات وحدود الطبقات. نجاح هذه البوابة لا يغني عن اختبارات Laravel الحية: يجب توثيق حالات النجاح والفشل والصلاحيات و422 و404 و409 حسب المسار قبل رفع حالة أي ميزة.

## المراجع

[1]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/6.0/version-requirements "Microsoft: Version requirements for .NET 6 SDK"
[2]: https://learn.microsoft.com/en-us/dotnet/core/install/windows "Microsoft: Install .NET on Windows"

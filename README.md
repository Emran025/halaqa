# Halaqa Desktop

تطبيق **WPF Application بلغة C# لسطح Windows** لمنصة **Quran Halaqa Live**. يتصل التطبيق بخدمة ويب Laravel عبر عقد OpenAPI في مشروع الخادم المرجعي، ولا يعرّف واجهات API مستقلة.

## البدء

يتطلب التشغيل Windows مع .NET SDK 8.0 أو Visual Studio 2022. قبل التشغيل عيّن عنوان Laravel الحقيقي بدلاً من العنوان التمثيلي في أحد الخيارين التاليين:

```json
// src/Halaqa.Desktop/appsettings.json
{
  "Api": {
    "BaseUrl": "https://your-domain.example/api/v1/"
  }
}
```

أو عبر متغير البيئة في Windows:

```powershell
$env:HALAQA_Api__BaseUrl = "https://your-domain.example/api/v1/"
dotnet run --project src/Halaqa.Desktop/Halaqa.Desktop.csproj
```

## المعمارية

كل ميزة تستخدم ثلاث طبقات مستقلة: `Presentation`، ثم `Domain`، ثم `Data`. يضم `Config` التهيئة وعميل REST والجلسة المشفرة، ويضم `Shared` الثيم والمكونات البصرية وأنواع النتيجة العامة. راجع `.agent/ARCHITECTURE.md` و`.agent/references/canonical-tree.md` قبل تعديل أي ميزة.

## الاتصال والتخزين

التطبيق متصل بالشبكة افتراضياً. يستعمل `Assets/Quran/QuranV3.sqlite` لنسخ محتوى المصحف محلياً إلى `%LocalAppData%\\Halaqa\\Data` وفتحه للقراءة فقط. كما يستعمل `halaqa-local.db` لتخزين عمليات الأخطاء غير المتزامنة في SQLite حتى تأكيد الخادم. تحفظ جلسة الوصول بتشفير Windows DPAPI، ولا تحفظ كلمات المرور. تستخدم عمليات الأخطاء `client_operation_id` ثابتاً لضمان إعادة المحاولة الآمنة وفق العقد.

تأتي قاعدة المصحف والخطوط المختارة من [shafeea_teach](https://github.com/Emran025/shafeea_teach) بوصفها أصولاً مرجعية للعميل فقط؛ لا ينقل المشروع تنفيذ Flutter ولا يستبدل بها خدمة Laravel. راجع `.agent/SHAFEEA_REFERENCE_ADOPTION.md` قبل تغيير هذه الأصول أو نشر خطوطها.

## المرجع الخلفي

العقد المُلزم موجود في مستودع [quran-halaqa](https://github.com/samalnashamy780-art/quran-halaqa) ضمن `.agent/openapi.yaml`. يوضّح الملف `.agent/CONTRACT_REFERENCE.md` نسخة المرجع ومسارات كل وحدة.

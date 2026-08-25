# Halaqa Desktop

عميل WPF عربي لمنصة **Quran Halaqa Live**. يتبع هذا المستودع عقد OpenAPI في مشروع الخادم المرجعي ولا يعرّف واجهات API مستقلة.

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

التطبيق متصل بالشبكة افتراضياً. لا يُخزن محلياً من بيانات المجال إلا cache المصحف، وعمليات الأخطاء التي لم تتم مزامنتها بعد. تحفظ جلسة الوصول بتشفير Windows DPAPI، ولا تحفظ كلمات المرور. تستخدم عمليات الأخطاء `client_operation_id` ثابتاً لضمان إعادة المحاولة الآمنة وفق العقد.

## المرجع الخلفي

العقد المُلزم موجود في مستودع [quran-halaqa](https://github.com/samalnashamy780-art/quran-halaqa) ضمن `.agent/openapi.yaml`. يوضّح الملف `.agent/CONTRACT_REFERENCE.md` نسخة المرجع ومسارات كل وحدة.

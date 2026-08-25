# المعمارية المعتمدة لعميل Halaqa

## القرار

يُبنى التطبيق كعميل WPF يعمل **على الإنترنت افتراضياً** وبنمط Clean Architecture متكرر داخل كل ميزة. تتبع كل ميزة ثلاث طبقات واضحة: `Presentation` لإدارة العرض والحالة، و`Domain` للسياسات والكائنات وحالات الاستخدام وعقود المستودعات، و`Data` لمصادر البيانات وتحويل JSON وتنفيذ المستودعات. يوجد مجلد `Config` عابر للميزات للتهيئة فقط، ومجلد `Shared` لمكونات العرض والسمات والتفاعل المشتركة فقط.

| الطبقة | المسؤوليات المسموحة | المحظور |
|---|---|---|
| Presentation | Views، ViewModels، Commands، Stores، التنقل، التحقق السطحي، حالات التحميل والخطأ والفراغ | HTTP، JSON، DTO، SQL، قرارات المجال |
| Domain | Entities، Value Objects، Repository Ports، Use Cases، قواعد التحول، Results | WPF، HttpClient، ملفات، JSON، DI container |
| Data | Remote Sources، Local Sources عند السماح، DTOs، Json converters، Repository implementations، mapping | عناصر WPF أو وصول مباشر إلى Views |
| Config | خيارات البيئة، DI composition، Http pipeline، التخزين الآمن، مراقبة الاتصال، إعداد السجل | منطق ميزة أو Views |
| Shared | الثيم، الأيقونات، Controls، Converters، عناصر UI، Dialogs، أدوات عرض بسيطة | HTTP أو DTO أو منطق عمل خاص بميزة |

## تدفق البيانات

```text
View (XAML)
  -> ViewModel / Feature Store
  -> Use Case
  -> Domain Repository Port
  -> Data Repository
  -> Remote Source / Local Source
  -> DTO <-> JSON / Local record
  -> Domain Entity / Result
  -> ViewModel state
  -> View
```

تكون الاستجابة الشبكية حقلاً دلالياً كما يحدده OpenAPI، مثل `user` أو `halaqa` أو `mistakes`، ولا تُلف في `data`. يطبق `ApiClient` العام المصادقة وCorrelation ID والمعالجة الموحدة للأخطاء، لكنه لا يعرف Types خاصة بميزة بعينها.

## التكوين المشترك

تُحقن جميع التبعيات عبر `Microsoft.Extensions.DependencyInjection`. تسجل `Config` الخيارات و`HttpClient` و`IAuthSessionStore` و`IConnectivityService` و`INavigationService` و`IDialogService` ثم يستدعي كل `FeatureModule.Register(IServiceCollection)` تسجيلاته. يعتمد نطاق الواجهة على MVVM، مع `CommunityToolkit.Mvvm` لتقليل التكرار و`MaterialDesignThemes` لمكتبة أيقونات SVG/Material Design ونظام تحكم WPF متسق.

يُحدد `ApiOptions.BaseUrl` من `appsettings.json` أو متغير البيئة `HALAQA_API_BASE_URL`؛ قيمة الأمثلة في OpenAPI ليست عنوان خادم فعلياً ولا يجوز إدراجها عنواناً إنتاجياً. تخزن جلسة الوصول بتشفير Windows DPAPI، ولا تسجل قيمتها.

## مبدأ الاستمرارية المحلية

لا يعد التطبيق تطبيق Offline-first عاماً. القراءة والكتابة في الحسابات والحلقات والخطط والجلسات والتقارير والإشعارات تتم عبر الشبكة فقط. الاستثناءان الوظيفيان الدائمان هما **المصحف** و**الأخطاء**:

| المجال | محلياً | عند وجود الشبكة | القاعدة |
|---|---|---|---|
| المصحف | فهرس السور، الصفحات، الآيات والكلمات بحسب `edition_id`، مع وقت صلاحية ومفتاح `(edition_id, page_number)` | يجلب عند الحاجة ويحدث cache بصورة ذرية | يسمح بالقراءة من cache عند غياب الشبكة؛ لا يعامل cache كمصدر حقيقة رسمي لحالة الجلسة. |
| الأخطاء | نسخة الخطأ وحالة المزامنة وOutbox للإنشاء/التعديل/الحذف و`client_operation_id` | يزامن العملية وفق ترتيبها ويحترم 409 و422 | لا تضيع العلامة؛ تعرض Pending/Conflict/Failed بوضوح، ولا تحذف العملية إلا بعد تأكيد الخادم. |
| الجلسة | Access token مشفر، المستخدم، الانتهاء، تفضيلات بسيطة | يجدد/يبطل عبر الخادم | لا يخزن كلمات المرور أو بيانات حساسة للفرد قبل القبول. |

يكون لكل عملية خطأ `client_operation_id` ثابت يولد عند الإنشاء. عند فشل الشبكة، تسجل في Outbox مع نوع العملية والحمولة المشفرة اللازمة ووقت الإنشاء وحالة المزامنة. يعيد `MistakesSyncService` المحاولات عند عودة الاتصال أو عند طلب المستخدم، مع تسلسل operations للمورد نفسه. لا يعاد تطبيق عملية إلا إذا كانت قابلة للإعادة وفق العقد.

## تصنيف أخطاء الاتصال

| الحالة | التحويل في Data | سلوك Presentation |
|---|---|---|
| لا شبكة / مهلة | `AppError.Network` | رسالة اتصال وخيار إعادة المحاولة؛ للمصحف يقرأ cache؛ وللأخطاء يحفظ العملية محلياً. |
| 401 | `AppError.Unauthorized` | مسح الجلسة بأمان وتوجيه المستخدم لتسجيل الدخول. |
| 403 أو 404 محمي | `AppError.Forbidden` أو `NotFound` | عدم عرض مورد أو بيانات محمية. |
| 409 | `AppError.Conflict` | عرض تعارض مع خيار إعادة تحميل المورد أو مراجعة العملية. |
| 422 | `AppError.Validation` مع `field_errors` | إسناد الرسائل إلى حقول النموذج. |
| 5xx | `AppError.Server` | توضيح عدم توفر الخدمة وإتاحة إعادة المحاولة. |

## وحدات المزايا

تبدأ الشريحة التنفيذية بالمصادقة، الجلسة، والتنقل، ثم ميزات القراءة الرئيسة. لكل Feature `FeatureModule` خاص بها. تستعمل الميزات عقود OpenAPI المطابقة، وتخضع أي شاشة لاختلاف الدور إلى `Role` القادم من `GET /me` أو استجابة المصادقة بدلاً من افتراض الدور محلياً.

> لا يعتمد العميل على حالته المحلية بوصفها مصدر الحقيقة للحلقات أو الجلسات أو حالة المصحف الرسمية. يحفظ نطاق المصحف الرسمي للجلسة عبر REST وفق عقد الخادم، بينما أي مؤشر لحظي يبقى مؤقتاً.

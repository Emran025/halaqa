# مرجع عقود الخادم

## حدود الأنظمة

`quran-halaqa` هو مشروع **خدمة ويب Laravel** يقدّم REST API والحالة التعليمية الرسمية. `halaqa` هو مشروع **WPF Application بلغة C# لسطح Windows** يستهلك هذا العقد. قاعدة `QuranV3.sqlite` والأنماط البصرية المأخوذة من `shafeea_teach` أصول محلية للعميل، وليست خدمة API ولا مرجعاً بديلاً للحالة الرسمية.

## المصدر المُلزم

مصدر الحقيقة لخادم Laravel هو مستودع [quran-halaqa](https://github.com/samalnashamy780-art/quran-halaqa)، وتحدد نسخة العقد المراجعة في commit:

```text
aee11df9eb4f58800455b600c3fc94128f0d7a76
```

الملف المُلزم لعميل WPF هو `.agent/openapi.yaml` في المستودع المرجعي، والإصدار المعلن فيه `1.1.0`. لا تنسخ OpenAPI إلى هذا المستودع بوصفها عقداً مستقلاً؛ عند بدء ميزة أو تغير الخادم، يجلب المطور النسخة الحالية ويقارن المسار والمخططات ذات الصلة قبل تعديل DTO أو Repository.

## قواعد الاستهلاك

| العنصر | قاعدة العميل |
|---|---|
| عنوان API | يأخذ من `HALAQA_API_BASE_URL` أو `appsettings.json`. لا يُعامل `api.example.com` الوارد في وثيقة العقد على أنه عنوان تشغيلي. |
| المصادقة | يضيف `BearerTokenHandler` ترويسة `Authorization: Bearer <token>` للطلبات المحمية فقط. |
| JSON | تستخدم `System.Text.Json` مع أسماء الحقول كما في العقد (`snake_case`) وDTO صريح لكل Request وResponse. |
| الاستجابات | تستهلك المفتاح الدلالي المباشر (`user`, `session`, `mistakes`...) ولا تنشئ كائناً عاماً باسم `data`. |
| التحقق | تحوّل 422 إلى `field_errors` حسب اسم الحقل المعلن، ولا تستنتج حقولاً من نص الرسالة. |
| التكرار | تثبت `client_operation_id` في التسجيل وطلب التسجيل وحفظ حالة المصحف ومسودات المهام وعمليات مزامنة الأخطاء وفق العقد. |
| الخصوصية | لا تفترض توفر بيانات الملف الكامل قبل قبول العلاقة، حتى لو كانت بعض الحقول null. |

## خريطة وحدات العميل إلى المسارات

| الوحدة | المسارات/العمليات ذات الصلة |
|---|---|
| Auth | `/auth/register/student`, `/auth/register/teacher`, `/auth/login`, `/auth/logout`, `/auth/password/*` |
| Account | `/me`, `/me/student-profile`, `/me/teacher-profile`, `/me/teacher-documents` |
| Halaqas | `/halaqas`, `/halaqas/{halaqaId}`, عضويات الحلقة وطلابها |
| Registrations | `/registration-requests` و`/halaqas/{halaqaId}/registration-requests` |
| FollowUp | خطط الطالب والحضور و`/follow-up-items` والتتبعات |
| Quran | `QuranV3.sqlite` المحلي لعرض المحتوى الثابت؛ ومسارات `/quran/*` و`/sessions/{sessionId}/mushaf-state` لمواءمة المحتوى وحفظ الحالة الرسمية عند الحاجة |
| Sessions | `/sessions`، إعدادات realtime، وتفويض القناة الداخلية |
| Mistakes | `/sessions/{sessionId}/tasks/{taskId}/mistakes` ومزامنة outbox |
| Reports/Progress | التقرير، التقييمات، التقارير والتقدم والأخطاء التاريخية |
| Notifications | `/notifications` وعمليات القراءة |

## واجب المراجعة قبل كل تعديل

يُراجع المطور تعريف المسار، `operationId`، نوع الاستجابة، الأكواد المتوقعة، والمخططات المشار إليها. إذا تغير `openapi.yaml` في الخلفية، يجب مراجعة DTOs وMappers وحالات الـ UI المتأثرة. لا يمكن أن يسبق العميل الخادم باختراع endpoint أو حقل أو حالة.

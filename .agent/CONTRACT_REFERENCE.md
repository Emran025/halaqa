# مرجع عقد خادم Laravel

## حدود الأنظمة

`quran-halaqa` هو مشروع **خدمة ويب Laravel** يملك REST API، التفويض، الحالة التعليمية الرسمية، وعقد النقل الفوري. `halaqa` هو **WPF Application بلغة C# لسطح Windows** يستهلك العقد ولا يعيد تعريفه. أصول القراءة المحلية في العميل، ومنها `QuranV3.sqlite` وخطوط صفحة المصحف الموجودة داخل هذا المستودع، ليست خدمة API ولا دليلاً على حالة جلسة حية أو بديلاً عن الحالة الرسمية.

## المصدر المُلزم ونقطة المراجعة

المصدر الملزم للخلفية هو [samalnashamy780-art/quran-halaqa](https://github.com/samalnashamy780-art/quran-halaqa). راجعت هذه الوثيقة الملف `.agent/openapi.yaml` من:

```text
Laravel branch: main
Laravel commit: f84e9dde84cb6b4239ccd341adcaf8e0d84e7c4e
OpenAPI version: 1.1.0
```

وسجلت حالة العميل وفق كود:

```text
Frontend branch reviewed: main
Frontend commit reviewed: 74b32ed8f774432967dd166ac4a36a0c67424a90
```

هذه القيم **مراجع تدقيق** وليست طلباً لتغيير منصة العميل أو بنية التطبيق. عند بدء ميزة أو تعديل DTO أو Mapper أو Repository أو API call، يجلب المطور نسخة `openapi.yaml` الحالية من فرع Laravel المرجعي ويتحقق من path و`operationId` وrequest/response schema وأسماء الحقول والقيم المعدودة والصلاحية واستجابات الخطأ قبل تعديل العميل.

## قواعد الاستهلاك

| العنصر | قاعدة العميل |
|---|---|
| عنوان API | يأخذ من `HALAQA_API_BASE_URL` أو `appsettings.json`. لا يُعامل `api.example.com` في وثيقة العقد عنواناً تشغيلياً. |
| المصادقة | يضيف `BearerTokenHandler` ترويسة `Authorization: Bearer <token>` للطلبات المحمية فقط. |
| JSON | تستخدم `System.Text.Json` وأسماء الحقول المعلنة في العقد، مع DTO صريح للطلب والاستجابة. |
| الاستجابات | لا ينشئ العميل غلاف `data` عاماً ما لم يعلنه مخطط الاستجابة. |
| التحقق | يحوّل 422 إلى `field_errors` باسم الحقل المعلن، ولا يستنتج حقولاً من نص الرسالة. |
| التكرار | يستخدم `client_operation_id` فقط حيث يعلنه العقد أو تدعمه عملية العميل، ولا يعوض به endpoint أو حقل غير موجود. |
| الخصوصية | لا يعرض بيانات ملف تفصيلية قبل قبول العلاقة؛ يلتزم ببطاقات المعلمين والمتقدمين العامة كما يحدد العقد. |
| الحالة الرسمية | تبقى الحلقات والطلبات والجلسات والتقارير وحالة المصحف الرسمية في Laravel. قاعدة المصحف المحلية للقراءة فقط. |

## خريطة وحدات العميل إلى العقد الحالي

| الوحدة | المسارات المعلنة ذات الصلة | حالة الاستهلاك الموثقة |
|---|---|---|
| Auth | `/auth/register/student`، `/auth/register/teacher`، `/auth/login`، `/auth/logout`، `/auth/password/*` | عميل REST موجود؛ تغيير كلمة المرور ليس له مسار غلاف حالي. |
| Account | `/me`، `/me/student-profile`، `/me/teacher-profile`، `/me/teacher-documents` | عملاء وواجهات موجودة؛ الملف العام ليس قابلاً للوصول من لوحة التطبيق الحالية. |
| Halaqas | `/halaqas`، `/halaqas/{halaqaId}`، `activate`، `deactivate` | مستهلكة في عميل إدارة الحلقات. |
| Memberships | `GET/POST /halaqas/{halaqaId}/students`، `PATCH/DELETE /halaqas/{halaqaId}/memberships/{membershipId}` | **فجوة:** عميل القائمة يطلب `GET /halaqas/{halaqaId}/memberships`، لكن هذا المسار غير معلن في العقد المراجع. القائمة المعلنة هي `students` بمرشحات `search` و`status` و`page` و`per_page`. |
| Registrations | `/teachers`، `/registration-requests`، `/halaqas/{halaqaId}/registration-requests`، `/student-applications` | استهلاك طلبات التسجيل الموجهة/الخاصة بالحلقة موجود. لا يوجد استهلاك لصندوق المعلم العام `student-applications`. |
| Quran | `/quran/surahs`، `/quran/pages/{pageNumber}`، `/quran/ayahs/{ayahId}` | عميل الصفحة البعيدة موجود؛ عارض مستقل قابل للوصول غير موجود. لا يعلن مخطط `Ayah` حقلاً لرموز العرض الصفحي. |
| Sessions | `/sessions/*`، `/sessions/{sessionId}/mushaf-state`، `/sessions/{sessionId}/realtime`، `/realtime/channels/authorize` | يستهلك العميل الحالي realtime config وchannel authorization و**حفظ** mushaf state فقط؛ لا يثبت ذلك WebRTC أو WebSocket أو مهام أو جلسة حية قابلة للوصول. |
| Mistakes | `/sessions/{sessionId}/tasks/{taskId}/mistakes` | مصدر أخطاء وoutbox موجودان بلا Presentation أو تنقل. |
| Reports/Progress | report و`/students/{studentId}/progress` و`reports` و`mistakes` | غير مستهلكة حالياً. |
| Notifications | `/notifications` وread/read-all | غير مستهلكة حالياً. |

## واجب المراجعة قبل كل تعديل

لا يسبق العميل الخادم باختراع endpoint أو حقل أو enum أو شكل استجابة أو قاعدة تفويض. إذا ظهر عدم تطابق، يسجّل في تقرير المراجعة مع الملف والمسار المتأثرين، ثم يُنسق حله في مستودع Laravel. راجع [تقرير التنفيذ التصحيحي](FRONTEND_IMPLEMENTATION_REVIEW.md) لتصنيف الأدلة وفجوات العقد والتحققات الحية المطلوبة.

# المراجعة التصحيحية لحالة تنفيذ عميل Halaqa WPF

> **الغرض.** هذه مراجعة أدلة لكود العميل، وليست خطة ترحيل ولا بياناً بأن أي واجهة مرئية مكتملة. لا تعد الميزة مكتملة إلا إذا وجدت طبقاتها المطلوبة، وسجّلت اعتماداتها في جذر التركيب، ووصل إليها المستخدم من الغلاف، وطابقت استدعاءاتها العقد الحالي، ونجحت اختبارات الجودة المناسبة.

## نقطة المراجعة ومصادر الحقيقة

| عنصر المراجعة | القيمة |
|---|---|
| مستودع العميل | `Emran025/halaqa` |
| الفرع الذي راجعه التدقيق | `main` |
| التزام العميل المراجع | `74b32ed8f774432967dd166ac4a36a0c67424a90` |
| مستودع العقد الخلفي | `samalnashamy780-art/quran-halaqa` |
| فرع العقد | `main` |
| التزام العقد المراجع | `f84e9dde84cb6b4239ccd341adcaf8e0d84e7c4e` |
| ملف العقد وإصداره | `.agent/openapi.yaml`، الإصدار `1.1.0` |

اعتمدت هذه المراجعة على كود `halaqa` بوصفه الدليل الوحيد لحالة العميل، وعلى `openapi.yaml` في التزام Laravel أعلاه بوصفه الدليل الوحيد للمسارات والمخططات والصلاحيات. لم تستخدم أي مستودعات أو تطبيقات خارجية كمصدر لتنفيذ العميل. [1] [2]

## معيار الحالة

| الحالة | معناها في هذه الوثيقة |
|---|---|
| **Partial** | يوجد جزء ذو أدلة من التنفيذ، لكنه يفتقد طبقة أو تسجيلاً أو مسار وصول أو تغطية أو تحققاً من العقد/التكامل يمنع اعتباره رحلة مكتملة. |
| **Planned** | لا توجد مجموعة قابلة للاستخدام من طبقات Domain/Data/Presentation المسجلة والملاحة والاختبارات. |
| **غير قابل للوصول** | قد يوجد ملف أو `DataTemplate` أو عقد، لكن الغلاف لا ينشئه أو لا يضعه في `CurrentPage` ضمن رحلة مستخدم فعلية. |
| **عقد فقط** | توجد واجهة أو DTO أو محلل رسائل، من دون تطبيق إنتاجي مسجل وقابل للحل. |

## جدول الأدلة لكل ميزة

| الرمز والنتيجة الفعلية | الملفات والطبقات الموجودة | مسارات Laravel التي يستهلكها العميل | الوصول من الغلاف | تنفيذ إنتاجي أم عقد فقط | الاختبارات الموجودة | التحقق الحي المتبقي وفجوات العقد |
|---|---|---|---|---|---|---|
| **AU-01 — Partial**<br>المصادقة والتسجيل وكلمة المرور | `Auth` يحتوي Data/Domain/Presentation و`AuthFeatureModule` ويسجل المصادر والمستودع وحالات الاستخدام ونماذج العرض. | `POST /auth/login`، `/auth/register/student`، `/auth/register/teacher`، `/auth/password/forgot`، `/auth/password/reset`، `/auth/password/change`، `/auth/logout`. | تسجيل الدخول والتسجيل والنسيت/إعادة الضبط موصولة من `MainShellViewModel`. نموذج تغيير كلمة المرور مسجل وله `DataTemplate`، لكن لا يوجد حدث لوحة أو مسار غلاف يعرضه. | عميل REST فعلي ومسجل؛ لا توجد نتيجة تشغيل حيّة موثقة. | `PasswordUseCaseTests`، `RegisterStudentUseCaseTests`، `RegisterTeacherUseCaseTests`، `RestoreSessionUseCaseTests`. | اختبار حي للنجاح و401 و409 و422 وحفظ/استعادة الجلسة والخروج. إضافة مسار وصول لتغيير كلمة المرور إن كان ضمن النطاق. |
| **AC-01 — Partial**<br>الملفات التفصيلية ووثائق المعلم | `Profile` و`TeacherDocuments` يحتويان Data/Domain/Presentation وموديولات تسجيل. رفع الوثيقة يستخدم `MultipartFormDataContent`. | `GET/PATCH /me/student-profile`، `GET/PATCH /me/teacher-profile`، `GET/POST/DELETE /me/teacher-documents`. | ملفات الطالب والمعلم متاحة من لوحة الدور؛ وثائق المعلم متاحة من حدث `DocumentsRequested` في ملف المعلم. | عملاء REST فعليون ومسجلون. | `ProfileMapperTests`، `TeacherProfileMapperTests`، `UpdateCurrentProfileUseCaseTests`، واختبارات mapper/use case لوثائق المعلم. | اختبار حي للصلاحية 403، الحقول 422، وتدفق رفع/عرض/حذف ملف `multipart/form-data`. لا يوجد دليل اختبار API حي. |
| **AC-02 — Partial، غير قابل للوصول**<br>الملف العام | Data/Domain/Presentation و`ProfileFeatureModule` تسجل `GeneralProfileViewModel`. | `GET/PATCH /me`. | `MainShellViewModel` يشترك في `ProfileRequested`، لكن `DashboardViewModel` لا يطلق هذا الحدث؛ لوحة الدور تفتح الملف التفصيلي فقط. | عميل REST فعلي ومسجل، لكن لا توجد رحلة غلاف فعلية للصفحة العامة. | `ProfileMapperTests` و`UpdateCurrentProfileUseCaseTests`. | إضافة نقطة وصول صريحة إن كانت الصفحة مطلوبة، ثم اختبار حي لـ 422 والاستجابة الناجحة. |
| **HA-01 — Partial مع فجوة عقد مانعة للقائمة**<br>الحلقات والعضويات | `Halaqas` و`Memberships` يحتويان Data/Domain/Presentation وموديولي DI؛ واجهة العضويات موجودة. | الحلقات: `GET/POST /halaqas`، `PATCH /halaqas/{id}`، `POST /activate` و`/deactivate`.<br>العضوية: `POST /halaqas/{id}/students`، `PATCH/DELETE /halaqas/{id}/memberships/{membershipId}`. | إدارة الحلقات من لوحة المعلم؛ قائمة العضويات من حدث الحلقة في الغلاف. | عملاء REST فعليون ومسجلون، لكن مسار قائمة العضويات ليس مطابقاً للعقد الحالي. | `HalaqaMapperTests`، `HalaqaUseCaseTests`، `HalaqaMembershipMapperTests`، `HalaqaMembershipUseCaseTests`. | العميل يستدعي `GET /halaqas/{id}/memberships?page=&status=`، بينما العقد الحالي يعلن `GET /halaqas/{id}/students` مع `search` و`status` و`page` و`per_page` واستجابة طلاب. لا يجوز افتراض المسار أو شكل `MembershipCollection`؛ يلزم قرار/تصحيح عقدي في Laravel أو مواءمة العميل لاحقاً، ثم اختبار 403/404/409 ومرشحات الخادم الحية. |
| **RG-01 — Partial**<br>طلبات التسجيل وتصفح المعلمين | Data/Domain/Presentation مسجلة للطلبات الموجهة ولطلبات الحلقة ولواجهة طلبات الطالب. | `GET /teachers`، `GET /teachers/{teacherId}`، `GET/POST /registration-requests`، `GET/POST /halaqas/{id}/registration-requests`، وعمليات `accept` و`reject` و`request-completion` و`DELETE /registration-requests/{id}`. | الطالب يصل إلى دليل المعلمين ثم طلباته. المعلم يصل إلى طلبات حلقة محددة من إدارة حلقاته. | عملاء REST فعليون ومسجلون. لا يوجد عميل لصندوق المعلم العام `GET /student-applications`. | mapper/use-case واختبارات ViewModel للطلبات والدليل. | اختبار حي للخصوصية قبل القبول ولـ 403/404/409/422 وسحب الطلب. فجوة وظيفية: عقد Laravel يعلن صندوق `student-applications`، لكن لا توجد طبقات أو تنقل لاستهلاكه. |
| **FU-01 — Planned**<br>الخطة والمتابعة والحضور | لا توجد مجلدات ميزة أو طبقات أو ViewModel أو View أو اختبار. | لا يستهلك العميل مسارات `follow-up-plan` أو `availability` أو `follow-up-items` أو tracking. | غير قابل للوصول. | لا يوجد. | لا يوجد. | يلزم تنفيذ طبقات كاملة ومسارات غلاف واختبارات، ثم اختبار صلاحيات العلاقة و409 و422. |
| **QU-01 — Partial، قراءة محلية غير مستقلة**<br>المصحف المحلي والحالة الرسمية | `Quran` يحتوي مصدر SQLite read-only، مصدر HTTP، repository وحالة استخدام `GetQuranPageUseCase` ومسجل في DI؛ لا يوجد ViewModel أو View مستقل للقراءة. | `GET /quran/pages/{pageNumber}?edition_id=`. حفظ الحالة الرسمية موجود فقط ضمن عميل الجلسات عبر `PUT /sessions/{id}/mushaf-state`. | لا توجد صفحة مصحف من لوحة التطبيق. استخدام الصفحة المحلي محصور في `LiveSessionViewModel` غير المسجل وغير القابل للوصول. | مصدر محلي وعميل HTTP فعليان؛ العرض التفاعلي ليس رحلة تشغيلية. | لا توجد اختبارات Quran مخصصة. | اختبار نسخ/فتح SQLite، فشل cache، توافق `edition_id`، و404 للمصدر البعيد. لا يقدم العقد حقل رموز عرض صفحي؛ `Ayah` يعلن `text` و`words` فقط، لذلك لا يجوز وصف بديل HTTP بأنه مطابق لعرض الرموز المحلي. |
| **SE-01 — Partial، غير قابل للتشغيل**<br>الجلسات الحية والمهام | توجد نماذج/محللات/عميل REST/repository/use cases/store و`LiveSessionViewModel` وView. موديول الجلسات لا يسجل `LiveSessionViewModel`. | يستهلك فقط `GET /sessions/{id}/realtime` و`POST /realtime/channels/authorize` و`PUT /sessions/{id}/mushaf-state`. لا يستهلك CRUD الجلسات أو المهام أو مسودة المهمة أو قراءة حالة المصحف. | يوجد `DataTemplate` للصفحة فقط. لا ينشئها `MainShellViewModel` ولا توجد نقطة لوحة أو route أو تسجيل DI لحلها. | `IPeerMediaConnection` و`IMushafRealtimeChannel` و`ILocalVideoRecorder` و`IRealtimeSignalingClient` **عقود فقط**. لا توجد فئة تنفيذ ولا تسجيل DI أو WebSocket/WebRTC/مسجل فيديو إنتاجي. | `PrepareLiveSessionUseCaseTests` و`LiveSessionViewModelTests` باستخدام بدائل اختبار؛ لا تثبت نقل شبكة أو وسائط. | يلزم أولاً تنفيذ، تسجيل، وربط نقطة دخول لجلسة حقيقية؛ ثم تدفق P2P مباشر فعلي: التقاط/تشغيل وسائط، offer/answer، Host ICE فقط، حالات اتصال وإعادة اتصال وفشل آمن. يلزم عميل WebSocket Laravel قابل للتشغيل وتفويض قناة وإرسال/استقبال الرسائل. لا توجد وسائط عبر خادم أو relay أو STUN/TURN/طرف ثالث. |
| **MI-01 — Partial، غير قابل للوصول**<br>الأخطاء والملاحظات والتقييم | `Mistakes` يحتوي Outbox محلياً ومصدراً بعيداً وrepository وخدمة مزامنة وحالة استخدام، وكلها مسجلة؛ لا توجد طبقة Presentation أو ViewModel أو View. لا توجد ميزات Notes/Evaluation. | الأخطاء فقط: `POST /sessions/{sessionId}/tasks/{taskId}/mistakes`. | غير قابل للوصول من الغلاف أو الجلسة. | تنفيذ outbox/HTTP موجود للأخطاء فقط؛ الملاحظات والتقييم غير موجودين. | لا توجد اختبارات Mistakes. | اختبار حي للتكرار و409 و422 وإعادة المحاولة والمزامنة. يلزم طبقات كاملة للملاحظات والتقييم قبل الادعاء بدعمهما. |
| **RP-01 — Planned**<br>التقرير والتقدم والسجل | لا توجد طبقات أو واجهات أو تسجيل أو اختبارات. | لا يستهلك `/sessions/{id}/report` أو approval/acknowledgment/reopen أو `/students/{id}/progress` و`/reports` و`/mistakes`. | غير قابل للوصول. | لا يوجد. | لا يوجد. | يلزم تنفيذ كامل ثم اختبارات العلاقة التعليمية والحالات 403/404/409/422. |
| **NO-01 — Planned**<br>الإشعارات | لا توجد طبقات أو واجهات أو تسجيل أو اختبارات. | لا يستهلك `/notifications` أو read/read-all. | غير قابل للوصول. | لا يوجد. | لا يوجد. | يلزم تنفيذ كامل واختبار صلاحية المستخدم والحالة الفارغة/الفشل والقراءة الحية. |

## حقيقة النقل الفوري والوسائط والتسجيل

لا يوجد في المصدر فئة تنفيذ أو تسجيل DI لـ `IPeerMediaConnection` أو `IMushafRealtimeChannel` أو `ILocalVideoRecorder` أو `IRealtimeSignalingClient`. توجد فقط الواجهات، DTOs، وسياسة Host ICE ومحللات رسائل. كما أن `SessionsFeatureModule` لا يسجل `LiveSessionViewModel`، ولا ينشئه الغلاف. لذلك لا يمكن لحاوية التشغيل حل صفحة الجلسة كما يطلب بناؤها، ولا يمثل وجود أزرار أو تسميات P2P أو رابط WebSocket في DTO دليلاً على WebRTC أو WebSocket أو DataChannel أو تسجيل فيديو عامل.

> لا يوجد ادعاء بتنفيذ اتصال وسائط أو ترحيل أو STUN/TURN أو Proxy أو Media Server. وإذا نفذت الرحلة مستقبلاً فيجب أن تستخدم Laravel للسيطرة والتفويض والإشارة والحالة الرسمية فقط، وأن تبقى الوسائط والبيانات الفورية بين الطرفين مباشرة وفق العقد. [2]

## الفجوات التي يجب تنسيقها مع Laravel، لا اختراعها في العميل

| الفجوة | الدليل | الإجراء الصحيح |
|---|---|---|
| قائمة العضويات | العميل يطلب `GET /halaqas/{id}/memberships` واستجابة عضويات، بينما العقد يعلن `GET /halaqas/{id}/students` واستجابة طلاب مع المرشحات المعلنة. | فتح/ربط مسألة عقدية في مستودع Laravel تحدد endpoint والمخطط والمرشحات، أو مواءمة العميل بعد اعتماد العقد. لا يرسل العميل مساراً افتراضياً. |
| صندوق المعلم العام | العقد يعلن `GET /student-applications`؛ لا توجد طبقات أو رحلة عميل لاستهلاكه. | تنفيذ ميزة منفصلة بعد مراجعة مخطط الاستجابة والصلاحيات الحالية. |
| رموز العرض الصفحي عن بعد | مخطط `Ayah` الحالي لا يعلن مسار رموز عرض صفحي؛ يعلن `text` و`words`. | اعتبار قاعدة القراءة المحلية مستقلة. لا يضاف حقل أو يتم ادعاؤه حتى يقر Laravel تغييره في OpenAPI. |

## متطلبات التحقق قبل رفع أي حالة

يجب أن تنجح بوابة الجودة المحددة في `.github/workflows/` على بيئة Windows: restore وbuild وunit tests وفحص المسافات وحدود الطبقات. لا يعد ذلك بديلاً عن اختبارات API الحية؛ يظل كل تدفق أعلاه محتاجاً إلى عنوان Laravel فعلي وحسابات دورية ونتائج نجاح وفشل مسجلة.

## المراجع

[1]: https://github.com/Emran025/halaqa/tree/74b32ed8f774432967dd166ac4a36a0c67424a90 "نقطة مراجعة عميل Halaqa WPF"
[2]: https://github.com/samalnashamy780-art/quran-halaqa/tree/f84e9dde84cb6b4239ccd341adcaf8e0d84e7c4e "نقطة مراجعة عقد Laravel"
[3]: https://github.com/samalnashamy780-art/quran-halaqa/blob/f84e9dde84cb6b4239ccd341adcaf8e0d84e7c4e/.agent/openapi.yaml "عقد OpenAPI 1.1.0"

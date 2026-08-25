# مصفوفة تنفيذ عميل Halaqa WPF

## قاعدة الانتقال

لا تعد أي ميزة مكتملة لمجرد ظهور واجهتها. تنتقل بطاقة الميزة من `Planned` إلى `Implemented` فقط عندما تحتوي `Presentation` و`Domain` و`Data`، وتطابق DTOs مسار ومخطط OpenAPI، وتنجح اختبارات التحويل وحالات الاستخدام، وتنجح عملية البناء على Windows. تنتقل إلى `Verified` بعد اختبار تدفق ناجح وفشل 401/403/404/409/422 وفشل الشبكة عند انطباقه.

| الرمز | الميزة | مسارات العقد الرئيسية | الوضع الحالي | بوابة الاختبار قبل الدمج |
|---|---|---|---|---|
| SH-00 | النظام المشترك والثيم | غير منطبق | In progress | بناء WPF، اختبار الموارد، فحص RTL والتنقل والوصول. |
| AU-01 | الدخول والخروج والجلسة | `/auth/login`, `/auth/logout`, `/me` | Partial | DTO/login/use case/session store واختبار 401 و422 والخروج. |
| AU-02 | التسجيل واستعادة كلمة المرور | `/auth/register/*`, `/auth/password/*`, `/auth/password/change` | Planned | تحقق متعدد الخطوات، idempotency، رسائل الأخطاء الحقلية، تدفق النجاح. |
| AC-01 | الملف والوثائق | `/me`, `/me/*-profile`, `/me/teacher-documents` | Planned | الخصوصية، الرفع/الحذف، 403/422، إعادة التحميل. |
| HA-01 | الحلقات والعضويات | `/halaqas`, عضوياتها وطلابها | Planned | أدوار المعلم/الطالب، create/update، 403/409، قوائم فارغة. |
| RG-01 | الطلبات والمعلمون | `/teachers`, `/student-applications`, `/registration-requests` | Planned | إخفاء الخصوصية قبل القبول، قبول/رفض/سحب، idempotency. |
| FU-01 | الخطة والحضور والمتابعة | `/students/{id}/follow-up-plan`, availability, `/follow-up-items`, trackings | Planned | تكرار الخطة، timezone، complete/skip/reschedule، 409/422. |
| QU-01 | المصحف المحلي والرسمي | `/quran/*`, `/sessions/{id}/mushaf-state` | Partial | SQLite read-only، تحويل الصفحة، cache failure، حفظ رسمي و422/409. |
| SE-01 | الجلسات والمهام | `/sessions`, realtime, tasks, draft | Partial | صلاحيات الطرفين، تفاوض P2P، Host ICE فقط، reconnect، لا وسائط عبر الخادم. |
| MI-01 | الأخطاء والملاحظات والتقييم | mistakes, notes, evaluation | Partial | SQLite outbox، منع تكرار منطقي، CRUD و409/422، مزامنة. |
| RP-01 | التقرير والتقدم والسجل | report, progress, reports, historical mistakes | Planned | اعتماد الطرفين، reopen، pagination، صلاحيات العلاقة التعليمية. |
| NO-01 | الإشعارات | `/notifications`, read, read-all | Planned | قائمة المستخدم فقط، unread/read-all، empty/error states. |

## سياسة الفروع والمراجعة

لكل بطاقة فرع باسم `feat/<code>-<slug>` ينطلق من `main`. يضيف الفرع تنفيذ طبقات الميزة والاختبارات وملاحظة في هذه المصفوفة. يفتح Pull Request إلى `main` بعد نجاح GitHub Actions. تراجع الفروق بمقارنة المسارات وDTOs مع `quran-halaqa/.agent/openapi.yaml`، ثم يدمج Pull Request فقط إذا كانت الفحوص خضراء ولا توجد تعارضات أو أسرار أو انحراف عن معمارية الطبقات.

## تسلسل التنفيذ المعتمد

1. SH-00 ثم AU-01 وAU-02 لأن بقية الرحلات تحتاج هوية وجلسة وإدارة أخطاء موحدة.
2. AC-01 ثم HA-01 وRG-01 لأن تكوين العلاقة التعليمية يسبق المتابعة.
3. FU-01 ثم QU-01 وMI-01 لأن واجهة الجلسة تحتاج مهمة ونطاقاً وسجل أخطاء.
4. SE-01 ثم RP-01 وNO-01.

لا يغير العميل عقد API ولا يضيف endpoint مفترضاً. إذا ظهر تعارض بين تجربة المستخدم والعقد، يسجل كفجوة عقدية في Pull Request ولا يموه بحالة محلية دائمة.

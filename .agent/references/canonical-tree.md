# الشجرة المعيارية لعميل Halaqa

```text
halaqa/
├── .agent/
│   ├── SKILL.md
│   ├── ARCHITECTURE.md
│   ├── UI_UX_SPEC.md
│   ├── CONTRACT_REFERENCE.md
│   └── references/
│       └── canonical-tree.md
├── src/
│   └── Halaqa.Desktop/
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── Halaqa.Desktop.csproj
│       ├── appsettings.json
│       ├── Config/
│       │   ├── Bootstrapper.cs
│       │   ├── DependencyInjection/
│       │   │   ├── ServiceCollectionExtensions.cs
│       │   │   └── FeatureRegistration.cs
│       │   ├── Http/
│       │   │   ├── ApiOptions.cs
│       │   │   ├── ApiClient.cs
│       │   │   ├── BearerTokenHandler.cs
│       │   │   └── ApiErrorMapper.cs
│       │   ├── Persistence/
│       │   │   ├── IAuthSessionStore.cs
│       │   │   └── WindowsProtectedAuthSessionStore.cs
│       │   ├── Connectivity/
│       │   │   ├── IConnectivityService.cs
│       │   │   └── NetworkConnectivityService.cs
│       │   └── Navigation/
│       │       ├── INavigationService.cs
│       │       └── NavigationService.cs
│       ├── Shared/
│       │   ├── Domain/
│       │   │   ├── Common/
│       │   │   │   ├── Result.cs
│       │   │   │   ├── AppError.cs
│       │   │   │   └── PageResult.cs
│       │   │   └── Time/
│       │   │       └── IClock.cs
│       │   └── Presentation/
│       │       ├── Controls/
│       │       ├── Converters/
│       │       ├── Dialogs/
│       │       ├── Icons/
│       │       ├── Navigation/
│       │       └── Themes/
│       └── Features/
│           ├── Auth/
│           │   ├── AuthFeatureModule.cs
│           │   ├── Presentation/
│           │   │   ├── Views/
│           │   │   ├── ViewModels/
│           │   │   ├── Stores/
│           │   │   └── Navigation/
│           │   ├── Domain/
│           │   │   ├── Entities/
│           │   │   ├── ValueObjects/
│           │   │   ├── Repositories/
│           │   │   └── UseCases/
│           │   └── Data/
│           │       ├── DataSources/
│           │       │   └── Remote/
│           │       ├── Models/
│           │       ├── Mappers/
│           │       └── Repositories/
│           ├── Account/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── Halaqas/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── Registrations/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── FollowUp/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── Sessions/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── Quran/
│           │   ├── QuranFeatureModule.cs
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           │       ├── DataSources/
│           │       │   ├── Remote/
│           │       │   └── Local/
│           │       ├── Models/
│           │       ├── Mappers/
│           │       └── Repositories/
│           ├── Mistakes/
│           │   ├── MistakesFeatureModule.cs
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           │       ├── DataSources/
│           │       │   ├── Remote/
│           │       │   └── Local/
│           │       ├── Models/
│           │       ├── Mappers/
│           │       ├── Repositories/
│           │       └── Sync/
│           ├── Reports/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           ├── Progress/
│           │   ├── Presentation/
│           │   ├── Domain/
│           │   └── Data/
│           └── Notifications/
│               ├── Presentation/
│               ├── Domain/
│               └── Data/
└── tests/
    └── Halaqa.Desktop.Tests/
        ├── Features/
        │   └── {Feature}/
        │       ├── Domain/
        │       ├── Data/
        │       └── Presentation/
        └── Config/
```

## قاعدة الموضع

ينتمي أي ملف لا يخص سوى ميزة واحدة إلى ميزة واحدة، حتى لو بدا قابلاً لإعادة الاستخدام. يستخرج إلى `Shared` فقط بعد وجود حاجتين حقيقيتين أو عند كونه مكون عرض عاماً لا يعرف نطاقاً تعليمياً. لا يسمح لـ`Presentation` بالاعتماد على `Data`؛ تستدعي `Presentation` حالات الاستخدام والعقود ضمن `Domain` فقط. يعتمد `Data` على `Domain` لتنفيذ الـ ports، ولا يعتمد على `Presentation`.

تضيف كل ميزة محلية التخزين (`Quran` و`Mistakes` فقط) `DataSources/Local` وعمليات transaction/serialization الخاصة بها داخل الميزة. التخزين الآمن للجلسة وإعداد العميل العام يظل في `Config` لأنه مشترك ولا يمثل بيانات مجال تعليمية.

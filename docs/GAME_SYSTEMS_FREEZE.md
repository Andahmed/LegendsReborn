# C0 — Game Systems Freeze

**Project:** LegendsReborn / Sands of Legends  
**Repository:** `Andahmed/LegendsReborn`  
**Scope:** مواصفات اللعبة والـPrototype فقط. لا تعديلات Backend أو Server.  
**Status:** Frozen for implementation review — 2026-11-12

## 1. نطاق هذه المهمة

هذه الوثيقة تثبت قواعد نسخة الـMVP القابلة للعب التي سينفذها فريق Unity. لا تعتبر أي قاعدة هنا تنفيذًا مكتملًا؛ الإغلاق الفعلي يحتاج Unity Compile واختبار PlayMode وتوثيق الدليل.

الهدف هو اختبار دورة لعب قصيرة حقيقية:

```text
Boot → Home → اختيار الفريق → Campaign Stage 1 → Battle 4v4 Prototype
→ Victory/Defeat → Result + Reward → Home
```

الهدف النهائي للعبة يظل **5 ضد 5** بتشكيلة من صفين أماميين وثلاثة خلفيين. استخدام 4 ضد 4 هنا محصور في اختبار الـPrototype ولا يغير عقد اللعبة النهائي.

## 2. حدود التنفيذ بين Manus وChatGPT

### ملفات ومهام Manus

- Unity Project وProjectSettings والمشاهد وملفات `.meta`.
- `Assets/Scripts/Presentation/` و`Assets/Scripts/UI/` و`Assets/Scenes/`.
- ربط المشاهد، الكاميرا، الإضاءة، النماذج، الحركة، اللمس، وPlayMode.
- أي تعديل على `HeroView` أو `BattleView` أو `GameFlowController` يجب أن يتضمن اختبارًا أو دليل تشغيل.

### ملفات ChatGPT في C0

- هذه الوثيقة فقط.
- لا تعديل على `Server/`.
- لا تعديل على `BattleSim.cs` أو `HeroCatalog.cs` أو ملفات Unity في هذه المهمة.
- أي اختلاف في القواعد التالية يحتاج تحديثًا منسقًا قبل تعديل الكود.

## 3. أبطال الـPrototype الأربعة

يستخدم الـPrototype أربعة أبطال، بطل واحد لكل فصيل أساسي من الفصائل الأربعة الأولى:

| Slot | Hero ID | الاسم الحالي | الفصيل | الدور | Model ID | المهارة | Ultimate |
|---|---|---|---|---|---|---|---|
| 0 | `warrior` | Iron Vanguard | Fortress | Tank | `Warrior` | Shield Bash | Fortress Wall |
| 1 | `berserker` | Ash Berserker | Wild | DPS | `Monk` | Rending Strike | Rage Tempest |
| 2 | `healer` | Oasis Healer | Forest | Support | `Cleric` | Healing Light | Life Bloom |
| 3 | `sharpshooter` | Dune Sharpshooter | Shadow | DPS | `Ranger` | Piercing Arrow | Rain of Arrows |

### متطلبات العرض

- يجب أن تظهر النماذج الأربعة الحقيقية، لا Capsule أو Cube في مسار الإنتاج.
- يجب أن يملك كل نموذج Idle وAttack وHit وDeath قابلًا للمشاهدة.
- يجب أن تكون نتيجة الحركة مرتبطة بـ`BattleEvent` وليس بتشغيل عشوائي من الواجهة.
- اختلاف الفصيل/البطل يجب أن يكون مرئيًا عبر النموذج أو اللون أو الإكسسوارات عند مشاركة نموذج.
- هذه المواصفات لا تثبت أن FBX أو Clips موجودة حاليًا؛ وجودها والتحقق منها مهمة Unity منفصلة.

## 4. أعداء Prototype الأربعة

التشكيلة التجريبية المقفولة:

| Slot | Enemy ID | Model ID | الدور المبدئي | ملاحظات |
|---|---|---|---|---|
| 0 | `prototype_bone_guard` | `Skeleton` | Tank | يبدأ في الصف الأمامي |
| 1 | `prototype_dune_ooze` | `Slime` | Tank/Durable | يبدأ في الصف الأمامي |
| 2 | `prototype_night_flier` | `Bat` | DPS | يبدأ في الصف الخلفي |
| 3 | `prototype_wasteland_drake` | `Dragon` | Boss-like DPS | يبدأ في الصف الخلفي، ليس زعيم الحملة النهائي |

لا يجوز نقل نظام أبطال أو بيانات لعب من `LegendsStyleRPG`. أسماء ونماذج الأعداء هنا مبنية على الأصول الموجودة في المشروع النشط فقط.

## 5. قاعدة القتال

### التشكيلة

- Prototype: 4 وحدات لكل فريق، slots من `0` إلى `3`.
- الهدف النهائي: 5 وحدات لكل فريق، slots من `0` إلى `4`.
- الصف الأمامي: slots `0` و`1`.
- الصف الخلفي: slots `2` و`3` في Prototype، ويضاف slot `4` في 5v5.
- الفريق 0 هو اللاعب، والفريق 1 هو الخصم.

### الزمن والتنفيذ

- القتال Auto Battle.
- السرعات: x1 وx2 وx4.
- خطوة المحاكاة الحالية: `BattleSim.Dt = 0.1` ثانية.
- الحد الأقصى الحالي: `BattleSim.MaxTime = 120` ثانية.
- انتهاء الوقت يعني فوز المدافع وفق السلوك الحالي، ويجب عرضه بوضوح كـTimeout/Defeat للّاعب.

### دورة الوحدة

1. تنتظر الوحدة حتى يمتلئ `ActionMeter` حسب السرعة.
2. تستخدم Ultimate عند طاقة 100 إذا كان Auto Ultimate مفعّلًا.
3. وإلا تستخدم Skill عند انتهاء Cooldown.
4. وإلا تستخدم Basic Attack.
5. الهجوم يسبب ضررًا ويزيد طاقة المهاجم ويمنح الهدف طاقة عند إصابته.
6. عند وصول HP إلى صفر يصدر `Death` event ولا تعود الوحدة هدفًا صالحًا.
7. عند غياب فريق كامل ينتهي القتال ويصدر `End` event مع Winner.

### الحسابات الحالية المعتمدة للـPrototype

- الضرر الأساسي الحالي في `BattleSim`:
  `Atk × SkillPower × 100 / (100 + TargetDef)`.
- ضربات Critical تستخدم `CritMultiplier = 1.5`.
- مخطط الفصائل الحالي يستخدم `FactionChart.Multiplier`.
- لا نضيف Buffs/Debuffs/Shields/DoT في C0؛ هذه خارج نطاق تجميد المواصفات وتحتاج مهمة مستقلة.

## 6. المرحلة التجريبية

### Stage 1 — The Seven Sands

- رقم المرحلة: `1`.
- البيئة: `The Seven Sands`.
- النوع: Campaign Prototype.
- القتال: 4v4 فقط لهذه المهمة.
- الطاقة: تستخدم القيمة الحالية في `CampaignStageData`، وتظل محلية في Prototype إلى أن تعود أولوية Backend.
- الهدف: هزيمة الفريق المقابل قبل انتهاء 120 ثانية.

### النتيجة

#### Victory

تظهر شاشة نتيجة تحتوي على:

- `VICTORY`.
- زمن المعركة.
- عدد الوحدات الباقية.
- سجل مختصر للضرر/الموت إن كان متاحًا.
- مكافأة اختبار واضحة.
- زر `RETURN TO HOME`.

#### Defeat

تظهر شاشة نتيجة تحتوي على:

- `DEFEAT` أو `TIMEOUT DEFEAT`.
- سبب النهاية.
- زر إعادة المحاولة أو العودة إلى Home.
- لا تمنح مكافأة الفوز.

### مكافأة الاختبار

مكافأة Prototype المحلية المقفولة للاختبار البصري والوظيفي:

```text
Gold: 100
Exp: 50
Gear Materials: 1
```

هذه ليست أرقام اقتصاد إطلاق نهائية، ولا يجوز تقديمها على أنها توازن إنتاجي. هدفها إثبات أن نتيجة المعركة تستدعي مسار مكافأة مرة واحدة وتظهرها في الواجهة.

## 7. معايير قبول C0 للفرق المنفذة

### Game loop

- Boot يفتح Home.
- Home يفتح Stage 1.
- Stage 1 ينشئ معركة Prototype 4v4.
- المعركة تنتهي بـVictory أو Defeat قبل timeout أو عنده.
- تظهر شاشة نتيجة حقيقية.
- زر العودة يعيد اللاعب إلى Home دون NullReference أو تكرار نتيجة.

### Combat presentation

- عدد الوحدات الظاهرة 8 في الـPrototype.
- كل وحدة مرتبطة بوحدة `BattleSim` صحيحة عبر team/slot.
- Attack/Skill/Ultimate/Hit/Death مرتبطة بالأحداث المرئية.
- لا Capsule أو Cube في مسار الأبطال/الوحوش عند توفر الأصول الحقيقية.

### Tests

- EditMode يثبت قواعد النتيجة والحسابات engine-free.
- PlayMode يشغل معركة 4v4 ويثبت:
  - وجود 8 وحدات.
  - انتهاء المعركة.
  - `Winner` يساوي 0 أو 1.
  - وجود `End` event.
  - عدم منح مكافأة أكثر من مرة.
- الاختبار الحالي `FiveAgainstFiveSimulation_ReachesAValidResult` لا يغلق هذا الشرط وحده؛ يجب إضافة أو تحديث اختبار 4v4 في مهمة Manus، مع الحفاظ على اختبار 5v5 للهدف النهائي.

## 8. توافق المواصفات مع الكود الحالي

تمت مراجعة الملفات التالية على `main`:

- `Assets/Scripts/Combat/BattleSim.cs`
- `Assets/Scripts/Core/HeroCatalog.cs`
- `Assets/Scripts/Presentation/HeroView.cs`
- `Assets/Scripts/Presentation/BattleView.cs`
- `Assets/Scripts/Meta/Campaign/CampaignContent.cs`
- `Assets/Tests/PlayMode/MvpBattlePlayModeTests.cs`

النتائج:

1. `BattleSim.TeamSize = 5` يضع حدًا أقصى فقط؛ يسمح عمليًا بتمرير 4 وحدات، لذلك 4v4 ممكن دون تغيير قاعدة 5v5 النهائية.
2. `HeroCatalog` يحتوي على الأبطال الأربعة المختارين وModel IDs المطلوبة، لكن ملفات `HomeHeroes` غير موجودة في baseline المفحوص.
3. الوحوش الأربعة المطلوبة موجودة كـFBX في `Assets/Resources/BattleMonsters` حسب فحص baseline السابق، لكن التحقق داخل Unity وملفات `.meta` غير متاح بعد.
4. `HeroView` الحالي ينشئ Capsule عند غياب النموذج ويستخدم Animator Triggers فقط؛ هذا لا يحقق معيار الحركة الحقيقية، وهو عمل Manus لاحقًا.
5. `BattleView` يشغل المحاكاة وينشر الأحداث، لكنه لا يثبت وحده إنشاء المشهد أو وجود كاميرا/إضاءة/واجهة نتيجة.
6. `CampaignContent.BuildEnemies` يولد خمسة أعداء بتدرج أرقام؛ لا يعتبر ذلك تنويعًا مؤلفًا للمحتوى النهائي.
7. اختبار PlayMode الحالي يختبر 5v5 فقط؛ يحتاج اختبار 4v4 منفصل للـPrototype.
8. لا توجد Scenes أو `.meta` في baseline المفحوص؛ لا يمكن اعتبار التنقل أو Unity Compile مثبتًا من هذه الوثيقة.

## 9. خارج النطاق

- Backend و`Server/`.
- API Contracts أو ربط الشبكة.
- Arena وWorld Boss كتطبيق كامل.
- Economy نهائي أو توازن المراحل 1–200.
- نقل أي gameplay/data من `LegendsStyleRPG`.
- ادعاء Unity Compile أو Android APK أو اختبار جهاز.

## 10. قرار الإغلاق

C0 يغلق فقط بعد مراجعة هذه الوثيقة من Manus وتأكيد عدم وجود تعارض مع تنفيذ Unity. إغلاق الوثيقة لا يعني أن اللعبة قابلة للعب؛ يعني أن نطاق Prototype وقواعده ثابتة ويمكن تنفيذها دون تخمين.

**حالة التحقق الحالية:** `لم يتم التحقق في Unity`.

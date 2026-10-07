using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EsotericEbbCheatMenu
{
    [BepInPlugin("com.jane.esotericebb.cheatmenu", "Esoteric Ebb Cheat Menu", "1.2.0")]
    public class CheatPlugin : BasePlugin
    {
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<CheatBehaviour>();
            GameObject host = new GameObject("EsotericEbbCheatMenu");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            host.AddComponent<CheatBehaviour>();
            Log.LogInfo("Esoteric Ebb Cheat Menu loaded. Press F8 to toggle.");
        }
    }

    public class CheatBehaviour : MonoBehaviour
    {
        public CheatBehaviour(IntPtr ptr) : base(ptr) { }

        private static Rect _window = new Rect(40f, 40f, 720f, 730f);
        private static bool _visible = true;
        private static bool _dragging;
        private static int _tab;
        private static Font _font;
        private static string _deltaText = "1000";
        private static string _setText = "";
        private static bool _errorLogged;

        private static readonly GUILayoutOption[] NoOptions = new GUILayoutOption[0];

        private static readonly string[] SkillLabels =
        {
            "运动 Athletics",
            "体操 Acrobatics",
            "巧手 Sleight of Hand",
            "隐匿 Stealth",
            "奥秘 Arcana",
            "历史 History",
            "调查 Investigation",
            "自然 Nature",
            "宗教 Religion",
            "驯兽 Animal Handling",
            "洞悉 Insight",
            "医药 Medicine",
            "察觉 Perception",
            "求生 Survival",
            "欺瞒 Deception",
            "威吓 Intimidation",
            "表演 Performance",
            "说服 Persuasion"
        };

        private void Update()
        {
            bool pressed = false;
            try
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && keyboard.f8Key.wasPressedThisFrame)
                {
                    pressed = true;
                }
            }
            catch
            {
            }

            if (pressed)
            {
                _visible = !_visible;
            }
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            EnsureFont();
            GUI.Box(_window, "奥秘消退 修改器    F8 开关 / 拖动顶部移动");
            HandleDrag();

            Rect content = new Rect(
                _window.x + 12f,
                _window.y + 30f,
                _window.width - 24f,
                _window.height - 42f);

            GUILayout.BeginArea(content);
            try
            {
                DrawTabs();
            }
            catch (Exception ex)
            {
                LogErrorOnce(ex);
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        private static void LogErrorOnce(Exception ex)
        {
            if (_errorLogged)
            {
                return;
            }

            _errorLogged = true;
            try
            {
                Debug.LogError("[EsotericEbbCheatMenu] " + ex);
            }
            catch
            {
            }
        }

        private void EnsureFont()
        {
            if (_font != null)
            {
                return;
            }

            try
            {
                _font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei", "SimHei", "SimSun", "Arial" }, 15);
                if (_font != null)
                {
                    GUI.skin.font = _font;
                }
            }
            catch
            {
                _font = null;
            }
        }

        private void HandleDrag()
        {
            Event current = Event.current;
            if (current == null)
            {
                return;
            }

            Rect titleBar = new Rect(_window.x, _window.y, _window.width, 26f);
            if (current.type == EventType.MouseDown && titleBar.Contains(current.mousePosition))
            {
                _dragging = true;
            }
            else if (current.type == EventType.MouseUp)
            {
                _dragging = false;
            }
            else if (_dragging && current.type == EventType.MouseDrag)
            {
                _window.x += current.delta.x;
                _window.y += current.delta.y;
                current.Use();
            }
        }

        private void DrawTabs()
        {
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button(_tab == 0 ? "[ 资源 ]" : "资源", GUILayout.Width(170f))) _tab = 0;
                if (GUILayout.Button(_tab == 1 ? "[ 属性 ]" : "属性", GUILayout.Width(170f))) _tab = 1;
                if (GUILayout.Button(_tab == 2 ? "[ 技能 ]" : "技能", GUILayout.Width(170f))) _tab = 2;
                if (GUILayout.Button(_tab == 3 ? "[ 其他 ]" : "其他", GUILayout.Width(170f))) _tab = 3;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");

            Sheet sheet = TryGetSheet();
            PC pc = TryGetPC();
            if (sheet == null || pc == null)
            {
                GUILayout.Label("尚未进入游戏。读取存档、进入场景后即可修改。");
                return;
            }

            switch (_tab)
            {
                case 0:
                    DrawResources(sheet, pc);
                    break;
                case 1:
                    DrawAttributes(sheet, pc);
                    break;
                case 2:
                    DrawSkills(sheet, pc);
                    break;
                default:
                    DrawMisc();
                    break;
            }
        }

        private void DrawResources(Sheet sheet, PC pc)
        {
            GUILayout.Label("自定义增减数值（正数增加，负数减少）：");
            _deltaText = GUILayout.TextField(_deltaText, 12, GUILayout.Width(160f));
            int delta = ParseInt(_deltaText, 1000);

            GUILayout.Label("");
            GUILayout.Label($"金币 Crowns：{GetCrowns(sheet)}");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("+100", GUILayout.Width(120f))) AddCrowns(sheet, 100);
                if (GUILayout.Button("+1000", GUILayout.Width(120f))) AddCrowns(sheet, 1000);
                if (GUILayout.Button("+10000", GUILayout.Width(120f))) AddCrowns(sheet, 10000);
                if (GUILayout.Button($"加 {delta}", GUILayout.Width(140f))) AddCrowns(sheet, delta);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                GUILayout.Label("设为", GUILayout.Width(50f));
                _setText = GUILayout.TextField(_setText, 12, GUILayout.Width(160f));
                if (GUILayout.Button("设置金币", GUILayout.Width(160f)))
                {
                    SetCrowns(sheet, ParseInt(_setText, GetCrowns(sheet)));
                }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");
            GUILayout.Label($"可分配升级点 LVLUP：{sheet.levelUpAvailable}");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("+1", GUILayout.Width(100f))) AddLevelPoints(sheet, 1);
                if (GUILayout.Button("+5", GUILayout.Width(100f))) AddLevelPoints(sheet, 5);
                if (GUILayout.Button("+10", GUILayout.Width(100f))) AddLevelPoints(sheet, 10);
                if (GUILayout.Button($"加 {delta}", GUILayout.Width(140f))) AddLevelPoints(sheet, delta);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                GUILayout.Label("设为", GUILayout.Width(50f));
                _setText = GUILayout.TextField(_setText, 12, GUILayout.Width(140f));
                if (GUILayout.Button("设置升级点", GUILayout.Width(170f)))
                {
                    sheet.levelUpAvailable = Math.Max(0, ParseInt(_setText, sheet.levelUpAvailable));
                }
                if (GUILayout.Button("清零（停止自动升级）", GUILayout.Width(240f)))
                {
                    sheet.levelUpAvailable = 0;
                }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");
            GUILayout.Label($"经验 EXP：{sheet.xp}    等级 LVL：{sheet.level}");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("+1000 EXP", GUILayout.Width(140f))) ChangeXp(pc, sheet, 1000);
                if (GUILayout.Button("+10000 EXP", GUILayout.Width(150f))) ChangeXp(pc, sheet, 10000);
                if (GUILayout.Button($"加 {delta} EXP", GUILayout.Width(160f))) ChangeXp(pc, sheet, delta);
                if (GUILayout.Button("升一级", GUILayout.Width(120f))) LevelUp(pc, sheet);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");
            GUILayout.Label($"生命 HP：{sheet.HP} / {sheet.maxHP}");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("回满血", GUILayout.Width(120f)))
                {
                    sheet.HP = sheet.maxHP;
                    pc.SyncUI();
                }
                if (GUILayout.Button($"+{delta} HP", GUILayout.Width(130f))) AddHp(sheet, pc, delta);
                if (GUILayout.Button($"-{Math.Abs(delta)} HP", GUILayout.Width(130f))) AddHp(sheet, pc, -Math.Abs(delta));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                GUILayout.Label("最大生命", GUILayout.Width(80f));
                if (GUILayout.Button("+10", GUILayout.Width(100f))) { sheet.maxHP += 10; pc.SyncUI(); }
                if (GUILayout.Button("-10", GUILayout.Width(100f))) { sheet.maxHP = Math.Max(1, sheet.maxHP - 10); pc.SyncUI(); }
                if (GUILayout.Button($"设为 {delta}", GUILayout.Width(150f))) { sheet.maxHP = Math.Max(1, delta); pc.SyncUI(); }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        private void DrawAttributes(Sheet sheet, PC pc)
        {
            bool changed = false;
            changed |= SetStat(sheet, StatKind.Strength, StatRow("力量", sheet.Strength));
            changed |= SetStat(sheet, StatKind.Dexterity, StatRow("敏捷", sheet.Dexterity));
            changed |= SetStat(sheet, StatKind.Constitution, StatRow("体质", sheet.Constitution));
            changed |= SetStat(sheet, StatKind.Intelligence, StatRow("智力", sheet.Intelligence));
            changed |= SetStat(sheet, StatKind.Wisdom, StatRow("感知", sheet.Wisdom));
            changed |= SetStat(sheet, StatKind.Charisma, StatRow("魅力", sheet.Charisma));

            if (changed)
            {
                Refresh(sheet, pc);
            }

            GUILayout.Label("");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("全部 10", GUILayout.Width(120f))) SetAllStats(sheet, pc, 10);
                if (GUILayout.Button("全部 15", GUILayout.Width(120f))) SetAllStats(sheet, pc, 15);
                if (GUILayout.Button("全部 20", GUILayout.Width(120f))) SetAllStats(sheet, pc, 20);
                if (GUILayout.Button("全部 25", GUILayout.Width(120f))) SetAllStats(sheet, pc, 25);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");
            GUILayout.Label($"护甲等级 AC：{sheet.ArmorClass}    熟练加值 Proficiency：{sheet.Proficiency}");
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("AC +1", GUILayout.Width(110f))) { sheet.ArmorClass += 1; pc.SyncUI(); }
                if (GUILayout.Button("AC -1", GUILayout.Width(110f))) { sheet.ArmorClass -= 1; pc.SyncUI(); }
                if (GUILayout.Button("AC +5", GUILayout.Width(110f))) { sheet.ArmorClass += 5; pc.SyncUI(); }
                if (GUILayout.Button("熟练 +1", GUILayout.Width(120f))) { sheet.Proficiency += 1; Refresh(sheet, pc); }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("");
            GUILayout.Label("改六维后会自动重算技能与生命。");
        }

        private void DrawSkills(Sheet sheet, PC pc)
        {
            GUILayout.BeginHorizontal(NoOptions);
            try
            {
                if (GUILayout.Button("全部 +1", GUILayout.Width(130f))) AdjustAllSkills(sheet, 1);
                if (GUILayout.Button("全部 +5", GUILayout.Width(130f))) AdjustAllSkills(sheet, 5);
                if (GUILayout.Button("重算（按属性+熟练）", GUILayout.Width(220f))) Refresh(sheet, pc);
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("直接改技能会在游戏重算时被覆盖；点“熟练”会立即按属性重算。");
            SkillRow(sheet, 0, sheet.Athletics, sheet.Athletics0);
            SkillRow(sheet, 1, sheet.Acrobatics, sheet.Acrobatics0);
            SkillRow(sheet, 2, sheet.SleightofHand, sheet.SleightofHand0);
            SkillRow(sheet, 3, sheet.Stealth, sheet.Stealth0);
            SkillRow(sheet, 4, sheet.Arcana, sheet.Arcana0);
            SkillRow(sheet, 5, sheet.History, sheet.History0);
            SkillRow(sheet, 6, sheet.Investigation, sheet.Investigation0);
            SkillRow(sheet, 7, sheet.Nature, sheet.Nature0);
            SkillRow(sheet, 8, sheet.Religion, sheet.Religion0);
            SkillRow(sheet, 9, sheet.AnimalHandling, sheet.AnimalHandling0);
            SkillRow(sheet, 10, sheet.Insight, sheet.Insight0);
            SkillRow(sheet, 11, sheet.Medicine, sheet.Medicine0);
            SkillRow(sheet, 12, sheet.Perception, sheet.Perception0);
            SkillRow(sheet, 13, sheet.Survival, sheet.Survival0);
            SkillRow(sheet, 14, sheet.Deception, sheet.Deception0);
            SkillRow(sheet, 15, sheet.Intimidation, sheet.Intimidation0);
            SkillRow(sheet, 16, sheet.Performance, sheet.Performance0);
            SkillRow(sheet, 17, sheet.Persuasion, sheet.Persuasion0);
        }

        private void DrawMisc()
        {
            GameManager gm = TryGetGameManager();
            if (gm == null)
            {
                GUILayout.Label("GameManager 尚未初始化。");
                return;
            }

            ToggleButton("CheatEnabled（开发者作弊标记）", gm.CheatEnabled, value => gm.CheatEnabled = value);
            ToggleButton("GodMode（上帝模式）", gm.GodMode, value => gm.GodMode = value);
            ToggleButton("JesusMode（复活/不死相关）", gm.JesusMode, value => gm.JesusMode = value);
            ToggleButton("FreecamCheat（自由视角）", gm.FreecamCheat, value => gm.FreecamCheat = value);
            ToggleButton("OutlineOffMode（关闭描边）", gm.OutlineOffMode, value => gm.OutlineOffMode = value);

            GUILayout.Label("");
            GUILayout.Label("改完后请在游戏里正常保存一次，数值才会写进存档。");
        }

        private static void ToggleButton(string label, bool value, Action<bool> setter)
        {
            if (GUILayout.Button(label + "：" + (value ? "开" : "关")))
            {
                setter(!value);
            }
        }

        private static void SkillRow(Sheet sheet, int index, int value, bool proficient)
        {
            GUILayout.BeginHorizontal(NoOptions);
            int newValue = value;
            bool newProf = proficient;
            try
            {
                GUILayout.Label(SkillLabels[index], GUILayout.Width(210f));
                if (GUILayout.Button("-", GUILayout.Width(40f))) newValue -= 1;
                GUILayout.Label(value.ToString(), GUILayout.Width(50f));
                if (GUILayout.Button("+", GUILayout.Width(40f))) newValue += 1;
                if (GUILayout.Button(proficient ? "熟练:是" : "熟练:否", GUILayout.Width(110f))) newProf = !proficient;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            if (newValue != value)
            {
                SetSkill(sheet, index, newValue);
            }
            if (newProf != proficient)
            {
                SetSkillProf(sheet, index, newProf);
                sheet.UpdateSheet();
            }
        }

        private static int StatRow(string label, int value)
        {
            GUILayout.BeginHorizontal(NoOptions);
            int newValue = value;
            try
            {
                GUILayout.Label(label, GUILayout.Width(70f));
                if (GUILayout.Button("-", GUILayout.Width(40f))) newValue -= 1;
                GUILayout.Label(value.ToString(), GUILayout.Width(50f));
                if (GUILayout.Button("+", GUILayout.Width(40f))) newValue += 1;
                GUILayout.Label("修正 " + Modifier(value).ToString("+#;-#;0"), GUILayout.Width(100f));
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            return Mathf.Clamp(newValue, 1, 30);
        }

        private static int Modifier(int score)
        {
            return Mathf.FloorToInt((score - 10) / 2f);
        }

        private static bool SetStat(Sheet sheet, StatKind kind, int value)
        {
            value = Mathf.Clamp(value, 1, 30);
            switch (kind)
            {
                case StatKind.Strength:
                    if (sheet.Strength == value) return false;
                    sheet.Strength = value;
                    return true;
                case StatKind.Dexterity:
                    if (sheet.Dexterity == value) return false;
                    sheet.Dexterity = value;
                    return true;
                case StatKind.Constitution:
                    if (sheet.Constitution == value) return false;
                    sheet.Constitution = value;
                    return true;
                case StatKind.Intelligence:
                    if (sheet.Intelligence == value) return false;
                    sheet.Intelligence = value;
                    return true;
                case StatKind.Wisdom:
                    if (sheet.Wisdom == value) return false;
                    sheet.Wisdom = value;
                    return true;
                default:
                    if (sheet.Charisma == value) return false;
                    sheet.Charisma = value;
                    return true;
            }
        }

        private static void SetAllStats(Sheet sheet, PC pc, int value)
        {
            sheet.Strength = value;
            sheet.Dexterity = value;
            sheet.Constitution = value;
            sheet.Intelligence = value;
            sheet.Wisdom = value;
            sheet.Charisma = value;
            Refresh(sheet, pc);
        }

        private static void Refresh(Sheet sheet, PC pc)
        {
            try
            {
                sheet.UpdateSheet();
                pc.SyncAbilityScores();
                pc.SyncUI();
            }
            catch
            {
            }
        }

        private static void ChangeXp(PC pc, Sheet sheet, int delta)
        {
            try
            {
                pc.ChangeXP(delta);
                pc.SyncUI();
            }
            catch
            {
                sheet.xp = Math.Max(0, sheet.xp + delta);
            }
        }

        private static void LevelUp(PC pc, Sheet sheet)
        {
            try
            {
                pc.LevelUp();
                pc.SyncUI();
            }
            catch
            {
                sheet.levelUpAvailable += 1;
            }
        }

        private static void AddLevelPoints(Sheet sheet, int delta)
        {
            sheet.levelUpAvailable = Math.Max(0, sheet.levelUpAvailable + delta);
        }

        private static void AddHp(Sheet sheet, PC pc, int delta)
        {
            sheet.HP = Mathf.Clamp(sheet.HP + delta, 0, Math.Max(1, sheet.maxHP));
            pc.SyncUI();
        }

        private static void AdjustAllSkills(Sheet sheet, int delta)
        {
            sheet.Athletics += delta;
            sheet.Acrobatics += delta;
            sheet.SleightofHand += delta;
            sheet.Stealth += delta;
            sheet.Arcana += delta;
            sheet.History += delta;
            sheet.Investigation += delta;
            sheet.Nature += delta;
            sheet.Religion += delta;
            sheet.AnimalHandling += delta;
            sheet.Insight += delta;
            sheet.Medicine += delta;
            sheet.Perception += delta;
            sheet.Survival += delta;
            sheet.Deception += delta;
            sheet.Intimidation += delta;
            sheet.Performance += delta;
            sheet.Persuasion += delta;
        }

        private static void SetSkill(Sheet sheet, int index, int value)
        {
            switch (index)
            {
                case 0: sheet.Athletics = value; break;
                case 1: sheet.Acrobatics = value; break;
                case 2: sheet.SleightofHand = value; break;
                case 3: sheet.Stealth = value; break;
                case 4: sheet.Arcana = value; break;
                case 5: sheet.History = value; break;
                case 6: sheet.Investigation = value; break;
                case 7: sheet.Nature = value; break;
                case 8: sheet.Religion = value; break;
                case 9: sheet.AnimalHandling = value; break;
                case 10: sheet.Insight = value; break;
                case 11: sheet.Medicine = value; break;
                case 12: sheet.Perception = value; break;
                case 13: sheet.Survival = value; break;
                case 14: sheet.Deception = value; break;
                case 15: sheet.Intimidation = value; break;
                case 16: sheet.Performance = value; break;
                case 17: sheet.Persuasion = value; break;
            }
        }

        private static void SetSkillProf(Sheet sheet, int index, bool value)
        {
            switch (index)
            {
                case 0: sheet.Athletics0 = value; break;
                case 1: sheet.Acrobatics0 = value; break;
                case 2: sheet.SleightofHand0 = value; break;
                case 3: sheet.Stealth0 = value; break;
                case 4: sheet.Arcana0 = value; break;
                case 5: sheet.History0 = value; break;
                case 6: sheet.Investigation0 = value; break;
                case 7: sheet.Nature0 = value; break;
                case 8: sheet.Religion0 = value; break;
                case 9: sheet.AnimalHandling0 = value; break;
                case 10: sheet.Insight0 = value; break;
                case 11: sheet.Medicine0 = value; break;
                case 12: sheet.Perception0 = value; break;
                case 13: sheet.Survival0 = value; break;
                case 14: sheet.Deception0 = value; break;
                case 15: sheet.Intimidation0 = value; break;
                case 16: sheet.Performance0 = value; break;
                case 17: sheet.Persuasion0 = value; break;
            }
        }

        private static int GetCrowns(Sheet sheet)
        {
            Item item = FindCrowns(sheet);
            return item != null ? item.Amount : 0;
        }

        private static void AddCrowns(Sheet sheet, int delta)
        {
            SetCrowns(sheet, Math.Max(0, GetCrowns(sheet) + delta));
        }

        private static void SetCrowns(Sheet sheet, int amount)
        {
            amount = Math.Max(0, amount);
            Item item = FindCrowns(sheet);
            if (item != null)
            {
                item.Amount = amount;
            }
            else
            {
                try
                {
                    GameManager gm = TryGetGameManager();
                    if (gm != null)
                    {
                        ItemInfo info = new ItemInfo();
                        info.itemName = "Crowns";
                        info.Amount = amount;
                        info.nameOfCurrentBagSpace = "";
                        gm.AddItemFromManager(info, false);
                    }
                }
                catch
                {
                }
            }

            try
            {
                sheet.UpdateCrownVariable();
                Inventory inventory = UnityEngine.Object.FindObjectOfType<Inventory>();
                if (inventory != null)
                {
                    inventory.UpdateInventory();
                }
            }
            catch
            {
            }
        }

        private static Item FindCrowns(Sheet sheet)
        {
            try
            {
                Il2CppSystem.Collections.Generic.List<Item> items = sheet.GetItems();
                if (items == null)
                {
                    return null;
                }

                for (int i = 0; i < items.Count; i++)
                {
                    Item item = items[i];
                    if (item != null && item.itemName == "Crowns")
                    {
                        return item;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private static Sheet TryGetSheet()
        {
            PC pc = TryGetPC();
            if (pc == null)
            {
                return null;
            }

            try
            {
                Sheet sheet = pc.sheet;
                if (sheet == null || sheet.Pointer == IntPtr.Zero)
                {
                    return null;
                }
                return sheet;
            }
            catch
            {
                return null;
            }
        }

        private static PC TryGetPC()
        {
            try
            {
                PC pc = PC.instance;
                if (pc == null || pc.Pointer == IntPtr.Zero)
                {
                    return null;
                }
                return pc;
            }
            catch
            {
                return null;
            }
        }

        private static GameManager TryGetGameManager()
        {
            try
            {
                GameManager gm = GameManager.instance;
                if (gm == null || gm.Pointer == IntPtr.Zero)
                {
                    return null;
                }
                return gm;
            }
            catch
            {
                return null;
            }
        }

        private static int ParseInt(string text, int fallback)
        {
            int value;
            return int.TryParse(text, out value) ? value : fallback;
        }

        private enum StatKind
        {
            Strength,
            Dexterity,
            Constitution,
            Intelligence,
            Wisdom,
            Charisma
        }
    }
}

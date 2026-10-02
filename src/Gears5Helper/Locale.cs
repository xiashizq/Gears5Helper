using System.Globalization;

namespace Gears5Helper;

public sealed record UiLanguage(string Code, string NativeName)
{
    public override string ToString() => NativeName;
}

public static class Locale
{
    public static readonly UiLanguage[] Languages =
    [
        new("en", "English"),
        new("zh-Hans", "简体中文"),
        new("zh-Hant", "繁體中文"),
        new("ja", "日本語"),
        new("ko", "한국어"),
        new("fr", "Français"),
        new("de", "Deutsch"),
        new("it", "Italiano"),
        new("es", "Español"),
        new("pt", "Português"),
    ];

    private static readonly string[] Keys =
    [
        "title", "subtitle", "voice", "platform", "game", "status", "path", "gameUi", "appLang",
        "apply", "restore", "read", "scan", "open", "add",
        "notFound", "found", "noMap", "noGameIni", "pickOne", "running",
        "noDir", "noIni", "unmodified", "written", "set", "noCode", "langCode", "empty",
        "shared", "store", "unspecified", "gears5", "eday", "unknown",
        "created", "updated", "restored", "deleted", "noBackup", "createdCode",
        "culture.en", "culture.fr", "culture.de", "culture.it", "culture.ko", "culture.zh-Hant",
        "culture.pt-BR", "culture.es-419", "culture.es-ES", "culture.zh-Hans", "culture.ja",
    ];

    private static readonly Dictionary<string, Dictionary<string, string>> Text = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = Lang(
            "Gears Language", "Subtitles", "Voice", "Platform", "Game", "Status", "Config folder", "Game language", "App language",
            "Apply", "Restore", "Load", "Scan", "Open folder", "Add folder",
            "No config found", "Found {0}", "No mapping", "No Game.ini.", "Select an item.", "{0} is running. Continue?",
            "Folder missing", "No Game.ini", "Unchanged", "Written", "Set", "No language code", "Code {0}", "empty",
            "Steam / Microsoft Store", "Microsoft Store", "Unspecified", "Gears 5", "Gears of War: E-Day", "Unknown",
            "Created {0}", "Updated {0}", "Restored {0}", "Removed {0}", "No backup {0}", "Created {0}, PreferredLanguageCode={1}",
            "English", "French", "German", "Italian", "Korean", "Traditional Chinese",
            "Portuguese", "Spanish (Latin America)", "Spanish (Spain)", "Simplified Chinese", "Japanese"),
        ["zh-Hans"] = Lang(
            "战争机器 语言", "字幕", "语音", "平台", "游戏", "状态", "配置目录", "游戏界面语言", "软件语言",
            "应用", "恢复", "读取", "扫描", "打开目录", "添加目录",
            "未找到配置", "找到 {0} 个", "无映射", "没有 Game.ini。", "先勾选一项。", "{0} 正在运行，继续？",
            "目录不存在", "无 Game.ini", "未修改", "已写入", "已设置", "未改语言码", "语言码 {0}", "空",
            "Steam / 微软商城", "微软商城", "未标明", "战争机器5", "E-Day", "未识别",
            "已创建 {0}", "已更新 {0}", "已从备份恢复 {0}", "已删除 {0}", "没有备份 {0}", "已创建 {0}，PreferredLanguageCode={1}",
            "英语", "法语", "德语", "意大利语", "韩语", "繁体中文",
            "葡萄牙语", "西班牙语(拉美)", "西班牙语(西班牙)", "简体中文", "日语"),
        ["zh-Hant"] = Lang(
            "戰爭機器 語言", "字幕", "語音", "平台", "遊戲", "狀態", "設定目錄", "遊戲介面語言", "軟體語言",
            "套用", "還原", "讀取", "掃描", "開啟資料夾", "新增資料夾",
            "找不到設定", "找到 {0} 個", "無對應", "沒有 Game.ini。", "請先勾選一項。", "{0} 正在執行，要繼續嗎？",
            "目錄不存在", "無 Game.ini", "未修改", "已寫入", "已設定", "未改語言碼", "語言碼 {0}", "空",
            "Steam / Microsoft Store", "Microsoft Store", "未標明", "戰爭機器5", "E-Day", "未識別",
            "已建立 {0}", "已更新 {0}", "已從備份還原 {0}", "已刪除 {0}", "沒有備份 {0}", "已建立 {0}，PreferredLanguageCode={1}",
            "英語", "法語", "德語", "義大利語", "韓語", "繁體中文",
            "葡萄牙語", "西班牙文（拉丁美洲）", "西班牙文（西班牙）", "簡體中文", "日語"),
        ["ja"] = Lang(
            "Gears 言語", "字幕", "音声", "プラットフォーム", "ゲーム", "状態", "設定フォルダ", "ゲームの言語", "表示言語",
            "適用", "復元", "読込", "検索", "フォルダを開く", "フォルダを追加",
            "設定が見つかりません", "{0} 件", "対応なし", "Game.ini がありません。", "項目を選択してください。", "{0} が実行中です。続行しますか？",
            "フォルダがありません", "Game.ini がありません", "未変更", "書込済", "設定済", "言語コードなし", "コード {0}", "空",
            "Steam / Microsoft Store", "Microsoft Store", "未指定", "Gears 5", "Gears of War: E-Day", "不明",
            "作成 {0}", "更新 {0}", "復元 {0}", "削除 {0}", "バックアップなし {0}", "作成 {0}、PreferredLanguageCode={1}",
            "英語", "フランス語", "ドイツ語", "イタリア語", "韓国語", "繁体中国語",
            "ポルトガル語", "スペイン語（ラテンアメリカ）", "スペイン語（スペイン）", "簡体中国語", "日本語"),
        ["ko"] = Lang(
            "Gears 언어", "자막", "음성", "플랫폼", "게임", "상태", "설정 폴더", "게임 언어", "앱 언어",
            "적용", "복원", "읽기", "검색", "폴더 열기", "폴더 추가",
            "설정을 찾지 못했습니다", "{0}개", "매핑 없음", "Game.ini가 없습니다.", "항목을 선택하세요.", "{0}이(가) 실행 중입니다. 계속할까요?",
            "폴더 없음", "Game.ini 없음", "변경 안 됨", "기록됨", "설정됨", "언어 코드 없음", "코드 {0}", "비어 있음",
            "Steam / Microsoft Store", "Microsoft Store", "미지정", "기어스 5", "기어스 오브 워: E-Day", "알 수 없음",
            "생성 {0}", "업데이트 {0}", "복원 {0}", "삭제 {0}", "백업 없음 {0}", "생성 {0}, PreferredLanguageCode={1}",
            "영어", "프랑스어", "독일어", "이탈리아어", "한국어", "번체 중국어",
            "포르투갈어", "스페인어(라틴아메리카)", "스페인어(스페인)", "간체 중국어", "일본어"),
        ["fr"] = Lang(
            "Langue Gears", "Sous-titres", "Voix", "Plateforme", "Jeu", "État", "Dossier", "Langue du jeu", "Langue de l'app",
            "Appliquer", "Restaurer", "Lire", "Analyser", "Ouvrir", "Ajouter",
            "Aucune config", "{0} trouvé(s)", "Aucun mappage", "Pas de Game.ini.", "Cochez un élément.", "{0} est en cours d'exécution. Continuer ?",
            "Dossier absent", "Pas de Game.ini", "Inchangé", "Écrit", "Défini", "Pas de code", "Code {0}", "vide",
            "Steam / Microsoft Store", "Microsoft Store", "Non indiqué", "Gears 5", "Gears of War: E-Day", "Inconnu",
            "Créé {0}", "Mis à jour {0}", "Restauré {0}", "Supprimé {0}", "Pas de sauvegarde {0}", "Créé {0}, PreferredLanguageCode={1}",
            "Anglais", "Français", "Allemand", "Italien", "Coréen", "Chinois traditionnel",
            "Portugais", "Espagnol (Amérique latine)", "Espagnol (Espagne)", "Chinois simplifié", "Japonais"),
        ["de"] = Lang(
            "Gears-Sprache", "Untertitel", "Stimme", "Plattform", "Spiel", "Status", "Ordner", "Spielsprache", "App-Sprache",
            "Übernehmen", "Wiederherstellen", "Lesen", "Suchen", "Ordner öffnen", "Ordner hinzufügen",
            "Keine Konfiguration", "{0} gefunden", "Keine Zuordnung", "Keine Game.ini.", "Bitte einen Eintrag auswählen.", "{0} läuft. Fortfahren?",
            "Ordner fehlt", "Keine Game.ini", "Unverändert", "Geschrieben", "Gesetzt", "Kein Sprachcode", "Code {0}", "leer",
            "Steam / Microsoft Store", "Microsoft Store", "Nicht angegeben", "Gears 5", "Gears of War: E-Day", "Unbekannt",
            "Erstellt {0}", "Aktualisiert {0}", "Wiederhergestellt {0}", "Entfernt {0}", "Kein Backup {0}", "Erstellt {0}, PreferredLanguageCode={1}",
            "Englisch", "Französisch", "Deutsch", "Italienisch", "Koreanisch", "Traditionelles Chinesisch",
            "Portugiesisch", "Spanisch (Lateinamerika)", "Spanisch (Spanien)", "Vereinfachtes Chinesisch", "Japanisch"),
        ["it"] = Lang(
            "Lingua Gears", "Sottotitoli", "Voce", "Piattaforma", "Gioco", "Stato", "Cartella", "Lingua di gioco", "Lingua dell'app",
            "Applica", "Ripristina", "Leggi", "Cerca", "Apri cartella", "Aggiungi cartella",
            "Nessuna config", "Trovati {0}", "Nessuna mappa", "Nessun Game.ini.", "Seleziona una voce.", "{0} è in esecuzione. Continuare?",
            "Cartella assente", "Nessun Game.ini", "Non modificato", "Scritto", "Impostato", "Nessun codice", "Codice {0}", "vuoto",
            "Steam / Microsoft Store", "Microsoft Store", "Non indicato", "Gears 5", "Gears of War: E-Day", "Sconosciuto",
            "Creato {0}", "Aggiornato {0}", "Ripristinato {0}", "Rimosso {0}", "Nessun backup {0}", "Creato {0}, PreferredLanguageCode={1}",
            "Inglese", "Francese", "Tedesco", "Italiano", "Coreano", "Cinese tradizionale",
            "Portoghese", "Spagnolo (America Latina)", "Spagnolo (Spagna)", "Cinese semplificato", "Giapponese"),
        ["es"] = Lang(
            "Idioma de Gears", "Subtítulos", "Voz", "Plataforma", "Juego", "Estado", "Carpeta", "Idioma del juego", "Idioma de la app",
            "Aplicar", "Restaurar", "Leer", "Buscar", "Abrir carpeta", "Añadir carpeta",
            "No hay configuración", "{0} encontrados", "Sin asignación", "No hay Game.ini.", "Marca un elemento.", "{0} está en ejecución. ¿Continuar?",
            "No existe la carpeta", "No hay Game.ini", "Sin cambios", "Escrito", "Listo", "Sin código", "Código {0}", "vacío",
            "Steam / Microsoft Store", "Microsoft Store", "Sin indicar", "Gears 5", "Gears of War: E-Day", "Desconocido",
            "Creado {0}", "Actualizado {0}", "Restaurado {0}", "Eliminado {0}", "Sin copia {0}", "Creado {0}, PreferredLanguageCode={1}",
            "Inglés", "Francés", "Alemán", "Italiano", "Coreano", "Chino tradicional",
            "Portugués", "Español (Latinoamérica)", "Español (España)", "Chino simplificado", "Japonés"),
        ["pt"] = Lang(
            "Idioma de Gears", "Legendas", "Voz", "Plataforma", "Jogo", "Estado", "Pasta", "Idioma do jogo", "Idioma do app",
            "Aplicar", "Restaurar", "Ler", "Procurar", "Abrir pasta", "Adicionar pasta",
            "Nenhuma config", "{0} encontrados", "Sem mapeamento", "Sem Game.ini.", "Marque um item.", "{0} está em execução. Continuar?",
            "Pasta ausente", "Sem Game.ini", "Sem alterações", "Gravado", "Definido", "Sem código", "Código {0}", "vazio",
            "Steam / Microsoft Store", "Microsoft Store", "Não indicado", "Gears 5", "Gears of War: E-Day", "Desconhecido",
            "Criado {0}", "Atualizado {0}", "Restaurado {0}", "Removido {0}", "Sem backup {0}", "Criado {0}, PreferredLanguageCode={1}",
            "Inglês", "Francês", "Alemão", "Italiano", "Coreano", "Chinês tradicional",
            "Português", "Espanhol (América Latina)", "Espanhol (Espanha)", "Chinês simplificado", "Japonês"),
    };

    public static string Current { get; private set; } = "zh-Hans";

    static Locale()
    {
        foreach (var language in Languages)
        {
            if (!Text.TryGetValue(language.Code, out var map) || map.Count != Keys.Length)
            {
                throw new InvalidOperationException("Incomplete UI language: " + language.Code);
            }
        }
    }

    public static string Get(string key)
    {
        if (Text.TryGetValue(Current, out var map) && map.TryGetValue(key, out var value))
        {
            return value;
        }

        if (Text["en"].TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public static string Format(string key, params object?[] args) => string.Format(CultureInfo.CurrentCulture, Get(key), args);

    public static string CultureName(string code) => Get("culture." + code);

    public static int KeyCount => Keys.Length;

    public static void Load()
    {
        var path = SettingsPath();
        if (File.Exists(path))
        {
            var saved = File.ReadAllText(path).Trim();
            if (Text.ContainsKey(saved))
            {
                Current = saved;
                return;
            }
        }

        Current = FromSystem();
    }

    public static void Set(string code)
    {
        if (!Text.ContainsKey(code))
        {
            return;
        }

        Current = code;
        try
        {
            var path = SettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, code);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static string FromSystem()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
                || name.Contains("TW", StringComparison.OrdinalIgnoreCase)
                || name.Contains("HK", StringComparison.OrdinalIgnoreCase)
                || name.Contains("MO", StringComparison.OrdinalIgnoreCase)
                ? "zh-Hant"
                : "zh-Hans";
        }

        foreach (var language in Languages)
        {
            if (language.Code != "zh-Hans" && language.Code != "zh-Hant"
                && name.StartsWith(language.Code, StringComparison.OrdinalIgnoreCase))
            {
                return language.Code;
            }
        }

        return "en";
    }

    private static string SettingsPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gears5Helper", "ui.lang");

    private static Dictionary<string, string> Lang(params string[] values)
    {
        if (values.Length != Keys.Length)
        {
            throw new InvalidOperationException("Expected " + Keys.Length + " strings, got " + values.Length);
        }

        var map = new Dictionary<string, string>(Keys.Length);
        for (var index = 0; index < Keys.Length; index++)
        {
            map[Keys[index]] = values[index];
        }

        return map;
    }
}

﻿using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Halaqa.Desktop.Features.Quran.Domain.Entities;
using Halaqa.Desktop.Shared.Presentation.Converters;

namespace Halaqa.Desktop.Features.Sessions.Presentation.Controls;

public sealed class InteractiveQuranCanvas : ContentControl
{
    private readonly TextBlock _textBlock;
    private readonly Dictionary<int, string> _currentMistakes = new(); // character/word index -> mistakeType

    public static readonly DependencyProperty QuranPageProperty = DependencyProperty.Register(
        nameof(QuranPage),
        typeof(QuranPage),
        typeof(InteractiveQuranCanvas),
        new PropertyMetadata(null, OnQuranPageChanged));

    public static readonly DependencyProperty StudentIdProperty = DependencyProperty.Register(
        nameof(StudentId),
        typeof(Guid),
        typeof(InteractiveQuranCanvas),
        new PropertyMetadata(Guid.Empty, OnStudentIdChanged));

    public static readonly DependencyProperty StopAyahNumberProperty = DependencyProperty.Register(
        nameof(StopAyahNumber),
        typeof(int?),
        typeof(InteractiveQuranCanvas),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStopAyahNumberChanged));

    public static readonly DependencyProperty MistakesCountProperty = DependencyProperty.Register(
        nameof(MistakesCount),
        typeof(int),
        typeof(InteractiveQuranCanvas),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public QuranPage? QuranPage
    {
        get => (QuranPage?)GetValue(QuranPageProperty);
        set => SetValue(QuranPageProperty, value);
    }

    public Guid StudentId
    {
        get => (Guid)GetValue(StudentIdProperty);
        set => SetValue(StudentIdProperty, value);
    }

    public int? StopAyahNumber
    {
        get => (int?)GetValue(StopAyahNumberProperty);
        set => SetValue(StopAyahNumberProperty, value);
    }

    public int MistakesCount
    {
        get => (int)GetValue(MistakesCountProperty);
        set => SetValue(MistakesCountProperty, value);
    }

    public event EventHandler<(int WordIndex, int AyahNumber, string MistakeType)>? WordMistakeTagged;
    public event EventHandler<int?>? AyahStopPointTagged;

    private static readonly ConcurrentDictionary<Guid, Dictionary<int, Dictionary<int, string>>> StudentPageMistakes = new();

    public InteractiveQuranCanvas()
    {
        _textBlock = new TextBlock
        {
            FlowDirection = FlowDirection.RightToLeft,
            TextAlignment = TextAlignment.Center,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
            LineHeight = 49,
            FontSize = 26,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            TextWrapping = TextWrapping.NoWrap
        };
        Content = _textBlock;
    }

    private static void OnQuranPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveQuranCanvas canvas)
        {
            canvas.LoadStudentMistakes();
            canvas.RenderMushafPage();
        }
    }

    private static void OnStudentIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveQuranCanvas canvas)
        {
            canvas.LoadStudentMistakes();
            canvas.RenderMushafPage();
        }
    }

    private static void OnStopAyahNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveQuranCanvas canvas)
        {
            canvas.RenderMushafPage();
        }
    }

    private void LoadStudentMistakes()
    {
        _currentMistakes.Clear();
        if (QuranPage == null || StudentId == Guid.Empty) return;

        if (StudentPageMistakes.TryGetValue(StudentId, out var pageDict) &&
            pageDict.TryGetValue(QuranPage.PageNumber, out var mistakes))
        {
            foreach (var kvp in mistakes)
            {
                _currentMistakes[kvp.Key] = kvp.Value;
            }
        }
        MistakesCount = _currentMistakes.Count;
    }

    private void SaveStudentMistakes()
    {
        if (QuranPage == null || StudentId == Guid.Empty) return;

        var pageDict = StudentPageMistakes.GetOrAdd(StudentId, _ => new Dictionary<int, Dictionary<int, string>>());
        pageDict[QuranPage.PageNumber] = new Dictionary<int, string>(_currentMistakes);
        MistakesCount = _currentMistakes.Count;
    }

    public void RenderMushafPage()
    {
        _textBlock.Inlines.Clear();
        if (QuranPage == null) return;

        var fontFamily = QuranPageFontFamilyConverter.GetPageFont(QuranPage.PageNumber);
        _textBlock.FontFamily = fontFamily;

        var rawText = QuranPage.PageGlyphText;
        var globalCharWordIndex = 0;

        for (int i = 0; i < rawText.Length; i++)
        {
            var ch = rawText[i];

            if (ch == '\r') continue;

            if (ch == '\n')
            {
                _textBlock.Inlines.Add(new LineBreak());
                continue;
            }

            if (ch == ' ')
            {
                _textBlock.Inlines.Add(new Run(" ") { FontFamily = fontFamily, FontSize = 26 });
                continue;
            }

            var currentIndex = globalCharWordIndex++;
            var wordGlyph = ch.ToString();

            // Find which ayah this word belongs to
            var ayahNum = 1;
            foreach (var a in QuranPage.Ayahs)
            {
                if (a.PageGlyphText.Contains(wordGlyph, StringComparison.Ordinal))
                {
                    ayahNum = a.Number;
                    break;
                }
            }

            var run = new Run(wordGlyph)
            {
                FontFamily = fontFamily,
                FontSize = 26,
                Cursor = Cursors.Hand
            };

            // Check mistake color for this specific character
            if (_currentMistakes.TryGetValue(currentIndex, out var mistakeType))
            {
                ApplyMistakeColorToRun(run, mistakeType);
            }
            else if (StopAyahNumber.HasValue && StopAyahNumber.Value == ayahNum)
            {
                // Check if this character is an end-of-ayah marker symbol
                run.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201)); // Emerald stop
                run.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
            }
            else
            {
                run.Background = Brushes.Transparent;
                run.Foreground = new SolidColorBrush(Color.FromRgb(75, 75, 75));
                run.FontWeight = FontWeights.Normal;
                run.TextDecorations = null;
            }

            // Attach single-word click handler
            var capturedWordIndex = currentIndex;
            var capturedAyahNum = ayahNum;
            var capturedWord = wordGlyph;

            run.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Left)
                {
                    ShowMistakeContextMenu(run, capturedWordIndex, capturedAyahNum, capturedWord);
                    e.Handled = true;
                }
            };

            _textBlock.Inlines.Add(run);
        }
    }

    private void ShowMistakeContextMenu(Run targetRun, int wordIndex, int ayahNumber, string word)
    {
        var contextMenu = new ContextMenu { FlowDirection = FlowDirection.RightToLeft };

        var memItem = new MenuItem { Header = "🔴 خطأ حفظ (تكرار / نسيان)" };
        memItem.Click += (_, _) => TagWord(targetRun, wordIndex, ayahNumber, "حفظ");

        var tajweedItem = new MenuItem { Header = "🟠 خطأ تجويد (أحكام / مخارج)" };
        tajweedItem.Click += (_, _) => TagWord(targetRun, wordIndex, ayahNumber, "تجويد");

        var tashkeelItem = new MenuItem { Header = "🟡 خطأ تشكيل (حركات / إعراب)" };
        tashkeelItem.Click += (_, _) => TagWord(targetRun, wordIndex, ayahNumber, "تشكيل");

        var alertItem = new MenuItem { Header = "🔵 تنبيه / توقف (تردد)" };
        alertItem.Click += (_, _) => TagWord(targetRun, wordIndex, ayahNumber, "تنبيه");

        var stopItem = new MenuItem { Header = $"🛑 تحديد نهاية التسميع عند الآية ({ayahNumber})" };
        stopItem.Click += (_, _) => SetStopPoint(ayahNumber);

        var clearItem = new MenuItem { Header = "✖️ إزالة الخطأ" };
        clearItem.Click += (_, _) => TagWord(targetRun, wordIndex, ayahNumber, "إلغاء");

        contextMenu.Items.Add(memItem);
        contextMenu.Items.Add(tajweedItem);
        contextMenu.Items.Add(tashkeelItem);
        contextMenu.Items.Add(alertItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(stopItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(clearItem);

        contextMenu.IsOpen = true;
    }

    private void TagWord(Run run, int wordIndex, int ayahNumber, string mistakeType)
    {
        if (mistakeType == "إلغاء")
        {
            _currentMistakes.Remove(wordIndex);
            run.Background = Brushes.Transparent;
            run.Foreground = new SolidColorBrush(Color.FromRgb(75, 75, 75));
            run.FontWeight = FontWeights.Normal;
            run.TextDecorations = null;
        }
        else
        {
            _currentMistakes[wordIndex] = mistakeType;
            ApplyMistakeColorToRun(run, mistakeType);
        }

        SaveStudentMistakes();
        WordMistakeTagged?.Invoke(this, (wordIndex, ayahNumber, mistakeType));
    }

    private void SetStopPoint(int ayahNumber)
    {
        StopAyahNumber = ayahNumber;
        SaveStudentMistakes();
        AyahStopPointTagged?.Invoke(this, ayahNumber);
        RenderMushafPage();
    }

    private static void ApplyMistakeColorToRun(Run run, string mistakeType)
    {
        // Keep every word on the same page background. An error is identified only
        // by black, bold text and an underline; its type remains available in the
        // context menu and in the recorded mistake payload.
        run.Background = Brushes.Transparent;
        run.Foreground = Brushes.Black;
        run.FontWeight = FontWeights.Bold;
        run.TextDecorations = TextDecorations.Underline;
    }
}

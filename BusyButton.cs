using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BusyButton.Controls;

public enum ButtonState
{
    Normal,
    Busy,
    Success,
    Error
}

internal sealed class BusyButtonStateContent
{
    public BusyButtonStateContent(BusyButton owner, ButtonState state, object? content, DataTemplate? contentTemplate)
    {
        Owner = owner;
        State = state;
        Content = content;
        ContentTemplate = contentTemplate;
    }

    public BusyButton Owner { get; }

    public ButtonState State { get; }

    public object? Content { get; }

    public DataTemplate? ContentTemplate { get; }
}



public class BusyButton : Button
{
    private const string StateContentTemplateResourceKey = "BusyButtonStateContentTemplate";

    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(BusyButton), new PropertyMetadata(false, OnBusyStateChanged));

    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(ButtonState), typeof(BusyButton), new PropertyMetadata(ButtonState.Normal, OnStateChanged));

    public static readonly DependencyProperty FeedbackDurationProperty = DependencyProperty.Register(nameof(FeedbackDuration), typeof(TimeSpan), typeof(BusyButton), new PropertyMetadata(TimeSpan.FromSeconds(2)));

    public static readonly DependencyProperty BusyContentProperty = DependencyProperty.Register(nameof(BusyContent), typeof(object), typeof(BusyButton), new PropertyMetadata(null, OnStateContentChanged));

    public static readonly DependencyProperty SuccessContentProperty = DependencyProperty.Register(nameof(SuccessContent), typeof(object), typeof(BusyButton), new PropertyMetadata("Done", OnStateContentChanged));

    public static readonly DependencyProperty ErrorContentProperty = DependencyProperty.Register(nameof(ErrorContent), typeof(object), typeof(BusyButton), new PropertyMetadata("Error", OnStateContentChanged));

    public static readonly DependencyProperty ShowStateIndicatorProperty = DependencyProperty.Register(nameof(ShowStateIndicator), typeof(bool), typeof(BusyButton), new PropertyMetadata(true, OnStateContentChanged));

    public static readonly DependencyProperty IndicatorSizeProperty = DependencyProperty.Register(nameof(IndicatorSize), typeof(double), typeof(BusyButton), new PropertyMetadata(14.0, OnStateContentChanged));

    public static readonly DependencyProperty IndicatorSpacingProperty = DependencyProperty.Register(nameof(IndicatorSpacing), typeof(double), typeof(BusyButton), new PropertyMetadata(6.0, OnStateContentChanged));

    public static readonly DependencyProperty PreserveButtonSizeProperty = DependencyProperty.Register(nameof(PreserveButtonSize), typeof(bool), typeof(BusyButton), new PropertyMetadata(true));

    private static readonly DependencyPropertyKey IsStateContentVisiblePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsStateContentVisible), typeof(bool), typeof(BusyButton), new PropertyMetadata(true));

    public static readonly DependencyProperty IsStateContentVisibleProperty = IsStateContentVisiblePropertyKey.DependencyProperty;

    private readonly DispatcherTimer _completeTimer;
    private object? _normalContent;
    private DataTemplate? _normalContentTemplate;
    private bool _isSettingStateContent;
    private bool _isWidthPreserved;
    private bool _isHeightPreserved;

    static BusyButton()
    {
        ContentProperty.OverrideMetadata(typeof(BusyButton), new FrameworkPropertyMetadata(null, OnContentChanged));
        ContentTemplateProperty.OverrideMetadata(typeof(BusyButton), new FrameworkPropertyMetadata(null, OnContentTemplateChanged));
        IsEnabledProperty.OverrideMetadata(typeof(BusyButton), new FrameworkPropertyMetadata(true, null, CoerceIsEnabled));
    }

    public BusyButton()
    {
        _completeTimer = new DispatcherTimer();
        _completeTimer.Tick += (_, _) =>
        {
            _completeTimer.Stop();
            SetCurrentValue(StateProperty, ButtonState.Normal);
        };
    }

    public bool IsBusy
    {
        get => State == ButtonState.Busy;
        set => SetValue(IsBusyProperty, value);
    }

    public ButtonState State
    {
        get => (ButtonState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public TimeSpan FeedbackDuration
    {
        get => (TimeSpan)GetValue(FeedbackDurationProperty);
        set => SetValue(FeedbackDurationProperty, value);
    }

    public object? BusyContent
    {
        get => GetValue(BusyContentProperty);
        set => SetValue(BusyContentProperty, value);
    }

    public object SuccessContent
    {
        get => GetValue(SuccessContentProperty);
        set => SetValue(SuccessContentProperty, value);
    }

    public object ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    public bool ShowStateIndicator
    {
        get => (bool)GetValue(ShowStateIndicatorProperty);
        set => SetValue(ShowStateIndicatorProperty, value);
    }

    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    public double IndicatorSpacing
    {
        get => (double)GetValue(IndicatorSpacingProperty);
        set => SetValue(IndicatorSpacingProperty, value);
    }

    public bool PreserveButtonSize
    {
        get => (bool)GetValue(PreserveButtonSizeProperty);
        set => SetValue(PreserveButtonSizeProperty, value);
    }

    public DataTemplate? NormalContentTemplate => _normalContentTemplate;

    public bool IsStateContentVisible
    {
        get => (bool)GetValue(IsStateContentVisibleProperty);
        private set => SetValue(IsStateContentVisiblePropertyKey, value);
    }

    public void ShowBusy() => State = ButtonState.Busy;

    public void ShowSuccess() => State = ButtonState.Success;

    public void ShowError() => State = ButtonState.Error;

    public void Reset() => State = ButtonState.Normal;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateStateContent();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateStateContentVisibility();
    }

    private static object CoerceIsEnabled(DependencyObject d, object baseValue)
    {
        var button = (BusyButton)d;
        return button.State != ButtonState.Normal
            ? false
            : baseValue;
    }

    private static void OnBusyStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (BusyButton)d;

        if ((bool)e.NewValue)
        {
            button.SetCurrentValue(StateProperty, ButtonState.Busy);
        }
        else if (button.State == ButtonState.Busy)
        {
            button.SetCurrentValue(StateProperty, ButtonState.Normal);
        }

        button.SyncIsBusy();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (BusyButton)d;
        button._completeTimer.Stop();

        if (button.State == ButtonState.Busy)
        {
            button.SetCurrentValue(IsBusyProperty, true);
        }
        else
        {
            button.SetCurrentValue(IsBusyProperty, false);
        }

        if (button.State is ButtonState.Success or ButtonState.Error && button.FeedbackDuration > TimeSpan.Zero)
        {
            button._completeTimer.Interval = button.FeedbackDuration;
            button._completeTimer.Start();
        }

        button.UpdateStateContent();
        button.CoerceValue(IsEnabledProperty);
    }

    private static void OnStateContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BusyButton)d).UpdateStateContent();
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (BusyButton)d;

        if (!button._isSettingStateContent)
        {
            button._normalContent = e.NewValue;
        }
    }

    private void UpdateStateContent()
    {
        if (_isSettingStateContent)
        {
            return;
        }

        object? content = State switch
        {
            ButtonState.Success => new BusyButtonStateContent(this, ShowStateIndicator ? ButtonState.Success : ButtonState.Normal, SuccessContent, null),
            ButtonState.Error => new BusyButtonStateContent(this, ShowStateIndicator ? ButtonState.Error : ButtonState.Normal, ErrorContent, null),
            ButtonState.Busy => new BusyButtonStateContent(this, ShowStateIndicator ? ButtonState.Busy : ButtonState.Normal, BusyContent ?? _normalContent, BusyContent is null ? _normalContentTemplate : null),
            _ => _normalContent
        };

        SetStateContent(content, content is BusyButtonStateContent);
    }

    private static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var button = (BusyButton)d;

        if (!button._isSettingStateContent)
        {
            button._normalContentTemplate = (DataTemplate?)e.NewValue;
        }
    }

    private void SetStateContent(object? content, bool useStateTemplate)
    {
        var stateTemplate = useStateTemplate
            ? TryFindResource(StateContentTemplateResourceKey) as DataTemplate
            : _normalContentTemplate;

        if (ReferenceEquals(Content, content) && ReferenceEquals(ContentTemplate, stateTemplate))
        {
            return;
        }

        _isSettingStateContent = true;

        try
        {
            if (useStateTemplate)
            {
                PreserveCurrentSize();
            }

            SetCurrentValue(ContentTemplateProperty, stateTemplate);
            SetCurrentValue(ContentProperty, content);

            UpdateStateContentVisibility();

            if (!useStateTemplate)
            {
                ReleasePreservedSize();
            }
        }
        finally
        {
            _isSettingStateContent = false;
        }
    }

    private void PreserveCurrentSize()
    {
        if (!PreserveButtonSize || !IsLoaded)
        {
            return;
        }

        if (!_isWidthPreserved
            && double.IsNaN(Width)
            && ReadLocalValue(WidthProperty) == DependencyProperty.UnsetValue
            && ActualWidth > 0)
        {
            SetCurrentValue(WidthProperty, ActualWidth);
            _isWidthPreserved = true;
        }

        if (!_isHeightPreserved
            && double.IsNaN(Height)
            && ReadLocalValue(HeightProperty) == DependencyProperty.UnsetValue
            && ActualHeight > 0)
        {
            SetCurrentValue(HeightProperty, ActualHeight);
            _isHeightPreserved = true;
        }
    }

    private void UpdateStateContentVisibility()
    {
        if (State == ButtonState.Normal || !ShowStateIndicator || !PreserveButtonSize)
        {
            IsStateContentVisible = true;
            return;
        }

        if (Content is not BusyButtonStateContent stateContent || stateContent.Content is null)
        {
            IsStateContentVisible = true;
            return;
        }

        var availableWidth = ActualWidth;

        if (availableWidth <= 0)
        {
            IsStateContentVisible = true;
            return;
        }

        var contentPresenter = new ContentPresenter
        {
            Content = stateContent.Content,
            ContentTemplate = stateContent.ContentTemplate,
            ContentTemplateSelector = ContentTemplateSelector,
            ContentStringFormat = ContentStringFormat
        };

        contentPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var requiredWidth =
            IndicatorSize +
            IndicatorSpacing +
            contentPresenter.DesiredSize.Width +
            Padding.Left +
            Padding.Right +
            BorderThickness.Left +
            BorderThickness.Right;

        IsStateContentVisible = requiredWidth <= availableWidth;
    }

    private void ReleasePreservedSize()
    {
        if (_isWidthPreserved)
        {
            ClearValue(WidthProperty);
            _isWidthPreserved = false;
        }

        if (_isHeightPreserved)
        {
            ClearValue(HeightProperty);
            _isHeightPreserved = false;
        }
    }

    private void SyncIsBusy()
    {
        var expectedIsBusy = State == ButtonState.Busy;

        if ((bool)GetValue(IsBusyProperty) != expectedIsBusy)
        {
            SetCurrentValue(IsBusyProperty, expectedIsBusy);
        }
    }
}

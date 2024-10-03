using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Controls;

public sealed partial class HeaderTile : UserControl
{
    public string Title
    {
        get
        {
            return (string)GetValue(TitleProperty);
        }
        set
        {
            SetValue(TitleProperty, value);
        }
    }

    public HeaderTile()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

    public string Description
    {
        get
        {
            return (string)GetValue(DescriptionProperty);
        }
        set
        {
            SetValue(DescriptionProperty, value);
        }
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

    public object Source
    {
        get
        {
            return GetValue(SourceProperty);
        }
        set
        {
            SetValue(SourceProperty, value);
        }
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register("Source", typeof(object), typeof(HeaderTile), new PropertyMetadata(null));

    public string Link
    {
        get
        {
            return (string)GetValue(LinkProperty);
        }
        set
        {
            SetValue(LinkProperty, value);
        }
    }

    public static readonly DependencyProperty LinkProperty =
        DependencyProperty.Register("Link", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));
}
